using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class SacudirMaquinaManager : MonoBehaviour
{
    [Header("Panel Principal")]
    public GameObject panelEsteMinijuego;

    [Header("Player")]
    public GameObject playerNormal;
    public GameObject playerSacudiendo;

    [Header("Máquina")]
    public RectTransform maquina;

    [Header("Botón Sacudir")]
    public HoldButton botonSacudir;

    [Header("Jefe")]
    public GameObject jefeMirando;
    public GameObject jefeNoMirando;

    [Header("UI")]
    public TextMeshProUGUI textoProgreso;
    public TextMeshProUGUI textoResultado;
    public TextMeshProUGUI textoTiempo;

    [Header("Paneles Resultado")]
    public GameObject panelTransicion;
    public GameObject panelVictoria;
    public GameObject panelDerrota;

    [Header("Configuración")]
    public float progreso = 0f;

    [Header("Tiempo")]
    public float tiempoRestante = 15f;
    private float tiempoInicial; // <-- NUEVO: Para guardar el tiempo original

    public float velocidadSubida = 35f;
    public float velocidadBajada = 10f;

    [Header("Jefe")]
    public float tiempoMinimoMirando = 2f;
    public float tiempoMaximoMirando = 4f;

    public float tiempoMinimoNoMirando = 2f;
    public float tiempoMaximoNoMirando = 5f;

    public float coyoteTime = 1f;

    [Header("Sacudida")]
    public float intensidadSacudida = 10f;
    public float velocidadSacudida = 40f;

    private bool juegoTerminado = false;

    private bool jefeEstaMirando = false;

    private float timerEstadoJefe = 0f;
    private float timerCoyote = 0f;

    private Vector2 posicionOriginalMaquina;

    void Start()
    {
        posicionOriginalMaquina = maquina.anchoredPosition;
        tiempoInicial = tiempoRestante; // <-- NUEVO: Guardamos el tiempo configurado

        CambiarEstadoJefe(false);
    }

    void Update()
    {
        if (!panelEsteMinijuego.activeInHierarchy)
            return;

        if (juegoTerminado) return;

        // TIMER
        tiempoRestante -= Time.deltaTime;

        textoTiempo.text =
            Mathf.CeilToInt(tiempoRestante).ToString();

        // Pierde si se acaba el tiempo
        if (tiempoRestante <= 0f)
        {
            PerderTiempo();
        }

        bool jugadorSacudiendo = botonSacudir.estaPresionado;

        // PLAYER VISUAL
        playerNormal.SetActive(!jugadorSacudiendo);
        playerSacudiendo.SetActive(jugadorSacudiendo);

        // PROGRESO
        if (jugadorSacudiendo)
        {
            progreso += velocidadSubida * Time.deltaTime;

            SacudirMaquina();
        }
        else
        {
            progreso -= velocidadBajada * Time.deltaTime;

            maquina.anchoredPosition = posicionOriginalMaquina;
        }

        progreso = Mathf.Clamp(progreso, 0f, 100f);

        textoProgreso.text = "Progreso: " + Mathf.RoundToInt(progreso) + "%";

        // GANAR
        if (progreso >= 100f)
        {
            Ganar();
        }

        // JEFE
        ActualizarJefe(jugadorSacudiendo);
    }

    void PerderTiempo()
    {
        juegoTerminado = true;

        textoResultado.text = "¡No conseguiste la bebida!";

        StartCoroutine(SecuenciaFinal(panelDerrota));
    }

    void SacudirMaquina()
    {
        float offsetX = Mathf.Sin(Time.time * velocidadSacudida) * intensidadSacudida;

        maquina.anchoredPosition =
            posicionOriginalMaquina + new Vector2(offsetX, 0f);
    }

    void ActualizarJefe(bool jugadorSacudiendo)
    {
        timerEstadoJefe -= Time.deltaTime;

        if (timerEstadoJefe <= 0f)
        {
            CambiarEstadoJefe(!jefeEstaMirando);
        }

        if (jefeEstaMirando)
        {
            timerCoyote -= Time.deltaTime;

            if (timerCoyote <= 0f && jugadorSacudiendo)
            {
                Perder();
            }
        }
    }

    void CambiarEstadoJefe(bool mirando)
    {
        jefeEstaMirando = mirando;

        jefeMirando.SetActive(mirando);
        jefeNoMirando.SetActive(!mirando);

        if (mirando)
        {
            timerEstadoJefe =
                Random.Range(tiempoMinimoMirando, tiempoMaximoMirando);

            timerCoyote = coyoteTime;
        }
        else
        {
            timerEstadoJefe =
                Random.Range(tiempoMinimoNoMirando, tiempoMaximoNoMirando);
        }
    }

    void Ganar()
    {
        juegoTerminado = true;

        textoResultado.text = "¡Conseguiste la bebida!";

        StartCoroutine(SecuenciaFinal(panelVictoria));
    }

    void Perder()
    {
        juegoTerminado = true;

        textoResultado.text = "¡El jefe te atrapó!";

        StartCoroutine(SecuenciaFinal(panelDerrota));
    }

    IEnumerator SecuenciaFinal(GameObject panelResultado)
    {
        yield return new WaitForSeconds(1.5f);

        panelEsteMinijuego.SetActive(false);

        ReiniciarValores(); // <-- NUEVO: Llamamos a la función para limpiar todo

        panelTransicion.SetActive(true);

        yield return new WaitForSeconds(3f);

        panelTransicion.SetActive(false);

        panelResultado.SetActive(true);
    }

    // <-- NUEVO: Método que reinicia todas las variables a su estado de fábrica
    void ReiniciarValores()
    {
        progreso = 0f;
        tiempoRestante = tiempoInicial;
        juegoTerminado = false;

        maquina.anchoredPosition = posicionOriginalMaquina;

        // Reiniciamos estado visual del jugador
        playerNormal.SetActive(true);
        playerSacudiendo.SetActive(false);

        // Reiniciamos estado del jefe
        CambiarEstadoJefe(false);

        // Actualizamos los textos inmediatamente para que no haya parpadeos raros
        textoProgreso.text = "Progreso: 0%";
        textoTiempo.text = Mathf.CeilToInt(tiempoRestante).ToString();
        textoResultado.text = "";
    }
}