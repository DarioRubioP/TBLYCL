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

    [Header("Tutorial – Imagen con latido y desvanecimiento")]
    public Image imageTutorial;                  // Asigná acá el componente Image de "ImageTutorial"
    public float duracionAntesDeDesvanecer = 5f;  // Segundos que late antes de empezar a desaparecer
    public float duracionDesvanecimiento = 1.5f;  // Duración del fade out
    public float velocidadLatido = 2f;            // Velocidad del pulso (mayor = más rápido)
    public float escalaMinima = 0.95f;
    public float escalaMaxima = 1.05f;

    // Estado interno
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    private float tiempoActual;
    private float suenioActual = 0f;
    private float timerReanudarSuenio = 0f;
    private bool scrolleando = false;
    private Coroutine corrutinaTutorial;

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

        // Muestra el tutorial con latido y luego lo desvanece
        MostrarTutorial();
    }

    // ---------- TUTORIAL: LATIDO Y DESVANECIMIENTO ----------
    public void MostrarTutorial()
    {
        if (imageTutorial == null) return;

        if (corrutinaTutorial != null)
            StopCoroutine(corrutinaTutorial);

        imageTutorial.gameObject.SetActive(true);
        Color c = imageTutorial.color;
        c.a = 1f;
        imageTutorial.color = c;
        imageTutorial.rectTransform.localScale = Vector3.one;

        corrutinaTutorial = StartCoroutine(AnimarTutorial());
    }

    IEnumerator AnimarTutorial()
    {
        float tiempoTranscurrido = 0f;
        Color colorOriginal = imageTutorial.color;
        colorOriginal.a = 1f;

        // Fase 1: latido suave durante "duracionAntesDeDesvanecer" segundos
        while (tiempoTranscurrido < duracionAntesDeDesvanecer)
        {
            float t = (Mathf.Sin(tiempoTranscurrido * velocidadLatido) + 1f) / 2f; // 0..1
            float escala = Mathf.Lerp(escalaMinima, escalaMaxima, t);
            imageTutorial.rectTransform.localScale = Vector3.one * escala;

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Fase 2: desvanecimiento suave, manteniendo el latido mientras se apaga
        float tiempoFade = 0f;
        while (tiempoFade < duracionDesvanecimiento)
        {
            float t = (Mathf.Sin(tiempoTranscurrido * velocidadLatido) + 1f) / 2f;
            float escala = Mathf.Lerp(escalaMinima, escalaMaxima, t);
            imageTutorial.rectTransform.localScale = Vector3.one * escala;

            Color c = colorOriginal;
            c.a = Mathf.Lerp(1f, 0f, tiempoFade / duracionDesvanecimiento);
            imageTutorial.color = c;

            tiempoFade += Time.deltaTime;
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        Color cFinal = colorOriginal;
        cFinal.a = 0f;
        imageTutorial.color = cFinal;
        imageTutorial.gameObject.SetActive(false);
        corrutinaTutorial = null;
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
