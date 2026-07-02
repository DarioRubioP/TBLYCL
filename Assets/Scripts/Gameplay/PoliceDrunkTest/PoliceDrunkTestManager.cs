using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PoliceDrunkTestManager : MonoBehaviour
{
    public static PoliceDrunkTestManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA; // Victoria
    public GameObject panelMinijuegoB; // Derrota

    [Header("Jugador")]
    public RectTransform jugadorRect;           // Imagen del borracho (de pie)
    public float amplitudBalanceo = 15f;        // Ángulo máximo de balanceo
    public float velocidadBalanceo = 3f;        // Velocidad del vaivén
    public GameObject imagenCaida;              // Jugador en el suelo (se activa al perder todo)

    [Header("Botones")]
    public List<GameObject> botones;            // Lista de todos los botones (desactivados al inicio)
    public float tiempoLimiteBoton = 1.2f;      // Tiempo máximo para pulsar el botón activo

    [Header("Vidas")]
    public List<Image> imagenesVida;            // 3 imágenes de corazón

    [Header("Tiempo")]
    public float tiempoLimite = 30f;
    public TextMeshProUGUI textoTiempo;

    // Estado interno
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    private float tiempoActual;
    private int vidas;
    private GameObject botonActivo;
    private float timerBoton;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Inicializamos todo al estado base desde el primer cuadro
        ReiniciarMinijuego();
    }

    void Update()
    {
        // Activar el juego en cuanto el panel principal se encienda
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado && !juegoTerminado)
        {
            juegoIniciado = true;
            ActivarBotonAleatorio();
        }

        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado || !juegoIniciado) return;

        // Temporizador general
        tiempoActual -= Time.deltaTime;
        ActualizarTextoTiempo();
        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(true);
            return;
        }

        // Balanceo automático del borracho
        float angle = Mathf.Sin(Time.time * velocidadBalanceo) * amplitudBalanceo;
        jugadorRect.localRotation = Quaternion.Euler(0f, 180f, angle);

        // Temporizador del botón activo
        if (botonActivo != null)
        {
            timerBoton -= Time.deltaTime;
            if (timerBoton <= 0f)
            {
                // No lo pulsó a tiempo → perder vida
                PerderVida();
                if (!juegoTerminado) ActivarBotonAleatorio(); // Solo activa otro si no ha perdido la última vida
            }
        }
    }

    // ----- MÉTODO DE REINICIO -----
    public void ReiniciarMinijuego()
    {
        juegoTerminado = false;
        juegoIniciado = false;
        tiempoActual = tiempoLimite;
        vidas = imagenesVida.Count;
        botonActivo = null;
        timerBoton = 0f;

        // UI y Textos
        ActualizarUI();
        ActualizarTextoTiempo();
        DesactivarTodosBotones();

        // Restaurar estado visual del jugador
        if (jugadorRect != null)
        {
            jugadorRect.gameObject.SetActive(true);
            jugadorRect.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (imagenCaida != null)
        {
            imagenCaida.SetActive(false);
        }
    }

    // Activa un botón aleatorio de la lista (y desactiva el anterior)
    void ActivarBotonAleatorio()
    {
        if (botonActivo != null)
            botonActivo.SetActive(false);

        if (botones.Count == 0) return;

        int indice = Random.Range(0, botones.Count);
        botonActivo = botones[indice];
        botonActivo.SetActive(true);
        timerBoton = tiempoLimiteBoton;
    }

    // Llamado desde el evento OnClick de cada botón
    public void PresionarBoton(GameObject botonPresionado)
    {
        if (juegoTerminado) return;

        // Solo es válido si es el botón activo
        if (botonPresionado == botonActivo)
        {
            // Acierto: activar otro botón inmediatamente
            ActivarBotonAleatorio();
        }
        else
        {
            // Error: pulsó un botón inactivo
            PerderVida();
        }
    }

    void PerderVida()
    {
        if (juegoTerminado) return;

        vidas--;
        ActualizarUI();

        if (vidas <= 0)
        {
            StartCoroutine(CaidaYDerrota());
        }
    }

    IEnumerator CaidaYDerrota()
    {
        juegoTerminado = true;
        // Ocultar jugador normal y mostrar imagen caída
        jugadorRect.gameObject.SetActive(false);
        if (imagenCaida != null) imagenCaida.SetActive(true);

        // Desactivar cualquier botón activo
        DesactivarTodosBotones();

        yield return new WaitForSeconds(2f);
        TerminarMinijuego(false);
    }

    void DesactivarTodosBotones()
    {
        foreach (GameObject btn in botones)
            btn.SetActive(false);
    }

    // ---------- UI ----------
    void ActualizarUI()
    {
        for (int i = 0; i < imagenesVida.Count; i++)
            imagenesVida[i].enabled = i < vidas;
    }

    void ActualizarTextoTiempo()
    {
        if (textoTiempo != null)
            textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();
    }

    // ---------- FINAL DEL MINIJUEGO ----------
    void TerminarMinijuego(bool victoria)
    {
        juegoTerminado = true;
        StopAllCoroutines();

        DesactivarTodosBotones();

        panelEsteMinijuego.SetActive(false);

        // << CAMBIO AQUÍ >>: Reseteamos todo de inmediato al apagar el panel
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
}