using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PekabooManager : MonoBehaviour
{
    public static PekabooManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA; // Victoria
    public GameObject panelMinijuegoB; // Derrota

    [Header("Jugador")]
    public RectTransform jugadorRect;               // Contenedor que rota
    public float maxAnguloCabeceo = -30f;           // Ángulo al dormirse

    [Header("Imágenes de estado")]
    public GameObject imagenJugadorMirando;
    public GameObject imagenJugadorNoMirando;
    public GameObject imagenCriminalAsecho;
    public GameObject imagenCriminalReposo;

    [Header("Criminal")]
    public RectTransform criminalRect;              // Contenedor que se mueve
    public float velocidadAvance = 80f;             // px/s cuando no miras
    public float velocidadRetroceso = 120f;         // px/s cuando miras
    public float distanciaAtaque = 50f;
    public Vector2 posInicialCriminal;
    public Vector2 posAtaqueCriminal;

    [Header("Botón de Mirar")]
    private bool mirando = false;

    [Header("Sueño")]
    public float suenioMaximo = 100f;
    public float tasaSuenioBase = 4f;
    public float reduccionAcierto = 15f;
    public float penalizacionFallo = 10f;

    [Header("Botones para mantenerse despierto")]
    public List<GameObject> botones;
    public float tiempoCambioBoton = 1.8f;

    [Header("Ataque")]
    public GameObject imagenAtaque;

    [Header("Tiempo")]
    public float tiempoLimite = 30f;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoSuenio;

    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    private float tiempoActual;
    private float suenioActual = 0f;
    private GameObject botonActivo;
    private float timerBoton; // No se usa ya que el cambio lo hace la corrutina

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Guardar posiciones iniciales de manera segura
        posInicialCriminal = criminalRect.anchoredPosition;
        if (posAtaqueCriminal == Vector2.zero)
            posAtaqueCriminal = new Vector2(jugadorRect.anchoredPosition.x + 50f, jugadorRect.anchoredPosition.y);

        // Inicializamos todo al estado base desde el primer cuadro
        ReiniciarMinijuego();
    }

    void Update()
    {
        // Iniciar cuando el panel se active
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado && !juegoTerminado)
        {
            juegoIniciado = true;
            ActivarBotonAleatorio();
            StartCoroutine(RutinaCambioBotones());
        }

        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado || !juegoIniciado) return;

        // Temporizador general
        tiempoActual -= Time.deltaTime;
        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(true);
            return;
        }

        // 1. Sueño base
        suenioActual += tasaSuenioBase * Time.deltaTime;
        if (suenioActual >= suenioMaximo)
        {
            suenioActual = suenioMaximo;
            DerrotaPorSuenio();
            return;
        }

        // 2. Movimiento del criminal y actualización de imágenes
        if (mirando)
        {
            // Retrocede
            criminalRect.anchoredPosition = Vector2.MoveTowards(criminalRect.anchoredPosition, posInicialCriminal, velocidadRetroceso * Time.deltaTime);
            ActualizarImagenesEstado(); // Asegura que el criminal esté en reposo
        }
        else
        {
            // Avanza
            criminalRect.anchoredPosition = Vector2.MoveTowards(criminalRect.anchoredPosition, posAtaqueCriminal, velocidadAvance * Time.deltaTime);
            ActualizarImagenesEstado(); // Asecho

            // ¿Alcanzó al jugador?
            if (Vector2.Distance(criminalRect.anchoredPosition, jugadorRect.anchoredPosition) < distanciaAtaque)
            {
                DerrotaPorAtaque();
                return;
            }
        }

        // 3. Rotación del jugador según sueño
        float factorSuenio = suenioActual / suenioMaximo;
        float angle = Mathf.Lerp(0f, maxAnguloCabeceo, factorSuenio);
        jugadorRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        ActualizarTextos();
    }

    // ----- MÉTODO DE REINICIO -----
    public void ReiniciarMinijuego()
    {
        juegoTerminado = false;
        juegoIniciado = false;
        mirando = false;
        tiempoActual = tiempoLimite;
        suenioActual = 0f;
        botonActivo = null;

        // Reset de posiciones y transformaciones fijas
        if (criminalRect != null)
        {
            criminalRect.anchoredPosition = posInicialCriminal;
        }

        if (jugadorRect != null)
        {
            jugadorRect.localRotation = Quaternion.identity;
        }

        // Reset visual de imágenes y pantallas de derrota brusca
        if (imagenAtaque != null)
        {
            imagenAtaque.SetActive(false);
        }

        // Actualizar interfaces y apagar botones
        ActualizarImagenesEstado();
        ActualizarTextos();
        DesactivarTodosBotones();
    }

    // ---------- MIRAR ----------
    public void EmpezarMirar()
    {
        if (juegoTerminado || !juegoIniciado) return;
        mirando = true;
        ActualizarImagenesEstado();
    }

    public void DejarMirar()
    {
        if (juegoTerminado || !juegoIniciado) return;
        mirando = false;
        ActualizarImagenesEstado();
    }

    void ActualizarImagenesEstado()
    {
        if (imagenJugadorMirando != null) imagenJugadorMirando.SetActive(mirando);
        if (imagenJugadorNoMirando != null) imagenJugadorNoMirando.SetActive(!mirando);
        if (imagenCriminalAsecho != null) imagenCriminalAsecho.SetActive(!mirando);
        if (imagenCriminalReposo != null) imagenCriminalReposo.SetActive(mirando);
    }

    // ---------- BOTONES DE SUENIO ----------
    IEnumerator RutinaCambioBotones()
    {
        while (!juegoTerminado && juegoIniciado)
        {
            yield return new WaitForSeconds(tiempoCambioBoton);
            if (juegoTerminado || !panelEsteMinijuego.activeInHierarchy) continue;

            if (botonActivo != null && botonActivo.activeSelf)
            {
                suenioActual += penalizacionFallo;
                if (suenioActual >= suenioMaximo)
                {
                    suenioActual = suenioMaximo;
                    DerrotaPorSuenio();
                    yield break;
                }
            }

            ActivarBotonAleatorio();
        }
    }

    void ActivarBotonAleatorio()
    {
        if (botonActivo != null) botonActivo.SetActive(false);
        if (botones.Count == 0) return;

        int indice = Random.Range(0, botones.Count);
        botonActivo = botones[indice];
        botonActivo.SetActive(true);
    }

    public void PresionarBoton(GameObject botonPresionado)
    {
        if (juegoTerminado) return;

        if (botonPresionado == botonActivo)
        {
            suenioActual = Mathf.Max(0f, suenioActual - reduccionAcierto);
            botonActivo.SetActive(false);
            botonActivo = null;
            ActivarBotonAleatorio();
        }
    }

    // ---------- DERROTAS ----------
    void DerrotaPorSuenio()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        StartCoroutine(DerrotaConImagen(null));
    }

    void DerrotaPorAtaque()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        StartCoroutine(DerrotaConImagen(imagenAtaque));
    }

    IEnumerator DerrotaConImagen(GameObject imagen)
    {
        if (imagen != null) imagen.SetActive(true);
        yield return new WaitForSeconds(1f);
        if (imagen != null) imagen.SetActive(false);
        TerminarMinijuego(false);
    }

    // ---------- FINAL ----------
    void TerminarMinijuego(bool victoria)
    {
        juegoTerminado = true;
        StopAllCoroutines();
        DesactivarTodosBotones();

        panelEsteMinijuego.SetActive(false);

        // << CAMBIO AQUÍ >>: Centralización del reinicio completo de valores de juego
        ReiniciarMinijuego();

        panelDivergencia.SetActive(true);

        StartCoroutine(Transicion(victoria));
    }

    IEnumerator Transicion(bool victoria)
    {
        yield return new WaitForSeconds(2f);
        panelDivergencia.SetActive(false);

        if (victoria)
            panelMinijuegoA.SetActive(true);
        else
            panelMinijuegoB.SetActive(true);
    }

    void DesactivarTodosBotones()
    {
        foreach (GameObject btn in botones)
            btn.SetActive(false);
    }

    void ActualizarTextos()
    {
        if (textoTiempo != null)
            textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();
        if (textoSuenio != null)
            textoSuenio.text = Mathf.FloorToInt(suenioActual) + "%";
    }
}