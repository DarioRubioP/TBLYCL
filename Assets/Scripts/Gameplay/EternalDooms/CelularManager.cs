using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CelularManager : MonoBehaviour
{
    public static CelularManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA;
    public GameObject panelMinijuegoB;

    [Header("Secuencia Narrativa")]
    public GameObject panelNarrativo;
    public GameObject imagenNarrativa;
    public GameObject panelJuego;

    [Header("Juego – Celular")]
    public ScrollInfinite scrollScript;
    public float suenioMaximo = 100f;
    public float incrementoSuenio = 10f;
    public float retardoReanudarSuenio = 1f;

    [Header("Tiempo")]
    public float tiempoLimite = 30f;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoSuenio;

    // Estado interno
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    private float tiempoActual;
    private float suenioActual = 0f;
    private float timerReanudarSuenio = 0f;
    private bool scrolleando = false;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Inicializamos valores al empezar
        ReiniciarValores();
    }

    void Update()
    {
        // Activar secuencia narrativa al activarse el panel principal
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado && !juegoTerminado)
        {
            StartCoroutine(SecuenciaNarrativa());
            juegoIniciado = true;
        }

        if (!panelJuego.activeInHierarchy || juegoTerminado) return;

        // Temporizador de tiempo
        tiempoActual -= Time.deltaTime;
        ActualizarTextos();

        if (tiempoActual <= 0f)
        {
            EvaluarFinalPorTiempo();
            return;
        }

        // Lógica del sueño
        if (scrolleando)
        {
            timerReanudarSuenio = retardoReanudarSuenio;
        }
        else
        {
            if (timerReanudarSuenio > 0f)
            {
                timerReanudarSuenio -= Time.deltaTime;
            }
            else
            {
                suenioActual += incrementoSuenio * Time.deltaTime;
                if (suenioActual >= suenioMaximo)
                {
                    suenioActual = suenioMaximo;
                    TerminarMinijuego(true);
                }
            }
        }

        ActualizarTextos();
    }

    public void EmpezarScroll() { scrolleando = true; }
    public void TerminarScroll() { scrolleando = false; }

    IEnumerator SecuenciaNarrativa()
    {
        // Aseguramos estado inicial al empezar narrativa
        ReiniciarValores();

        panelNarrativo.SetActive(true);
        imagenNarrativa.SetActive(false);
        panelJuego.SetActive(false);

        yield return new WaitForSeconds(3f);
        imagenNarrativa.SetActive(true);

        yield return new WaitForSeconds(2f);
        panelJuego.SetActive(true);
        imagenNarrativa.SetActive(false);
    }

    void EvaluarFinalPorTiempo()
    {
        TerminarMinijuego(suenioActual >= 70f);
    }

    void TerminarMinijuego(bool exito)
    {
        juegoTerminado = true;
        StopAllCoroutines();

        panelJuego.SetActive(false);
        panelNarrativo.SetActive(false);
        panelEsteMinijuego.SetActive(false);
        panelDivergencia.SetActive(true);

        StartCoroutine(Transicion(exito));
    }

    IEnumerator Transicion(bool exito)
    {
        yield return new WaitForSeconds(2f);
        panelDivergencia.SetActive(false);

        if (exito) panelMinijuegoA.SetActive(true);
        else panelMinijuegoB.SetActive(true);

        // Al terminar el proceso completo, reseteamos para una futura ejecución
        ReiniciarValores();
    }

    void ActualizarTextos()
    {
        if (textoTiempo != null) textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();
        if (textoSuenio != null) textoSuenio.text = Mathf.FloorToInt(suenioActual) + "%";
    }

    // Nuevo método de reseteo
    public void ReiniciarValores()
    {
        tiempoActual = tiempoLimite;
        suenioActual = 0f;
        timerReanudarSuenio = 0f;
        juegoTerminado = false;
        juegoIniciado = false;
        scrolleando = false;

        ActualizarTextos();
    }
}