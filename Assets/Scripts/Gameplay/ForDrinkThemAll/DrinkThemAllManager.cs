using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DrinkThemAllManager : MonoBehaviour
{
    public static DrinkThemAllManager Instancia;
    [Header("UI Elementos de este Minijuego")]
    public GameObject panelEsteMinijuego; // El panel contenedor de TODO este minijuego actual
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoCervezas;
    //public GameObject manoTutorial;
    [Header("Paneles de Divergencia")]
    public GameObject panelDivergencia; // El panel general que dice "Cargando Divergencia"
    public GameObject panelMinijuegoA;  // Siguiente minijuego si tomó 3 o menos
    public GameObject panelMinijuegoB;  // Siguiente minijuego si tomó más de 3
    [Header("Configuración")]
    public float tiempoRestante = 15f;
    private int cervezasTomadas = 0;
    private bool juegoTerminado = false;
    private float tiempoInicial; // Para respaldar el tiempo configurado en el Inspector

    [Header("Tutorial – Imagen con latido y desvanecimiento")]
    public Image imageTutorial;                  // Asigná acá el componente Image de "ImageTutorial"
    public float duracionAntesDeDesvanecer = 5f;  // Segundos que late antes de empezar a desaparecer
    public float duracionDesvanecimiento = 1.5f;  // Duración del fade out
    public float velocidadLatido = 2f;            // Velocidad del pulso (mayor = más rápido)
    public float escalaMinima = 0.95f;
    public float escalaMaxima = 1.05f;
    private Coroutine corrutinaTutorial;

    void Awake()
    {
        Instancia = this;
    }
    void Start()
    {
        // Guardamos el tiempo original del inspector para poder reutilizarlo en los reinicios
        tiempoInicial = tiempoRestante;
        // Inicializamos los valores por primera vez
        ReiniciarMinijuego();
    }
    void Update()
    {
        if (!panelEsteMinijuego.activeInHierarchy)
            return;
        if (juegoTerminado) return;
        // Cuenta regresiva del tiempo
        tiempoRestante -= Time.deltaTime;
        textoTiempo.text = Mathf.CeilToInt(tiempoRestante).ToString();
        // Ocultar la mano tutorial tras 3 segundos o si el jugador hace clic
        //if (manoTutorial.activeSelf && (Input.GetMouseButtonDown(0) || tiempoRestante < (tiempoInicial - 3f)))
        //{
        //  manoTutorial.SetActive(false);
        //}
        // Al agotarse el tiempo
        if (tiempoRestante <= 0)
        {
            TerminarMinijuego();
        }
    }
    // ----- MÉTODO DE REINICIO -----
    public void ReiniciarMinijuego()
    {
        juegoTerminado = false;
        cervezasTomadas = 0;
        tiempoRestante = tiempoInicial;
        // Restablecer textos e interfaz de conteo
        if (textoTiempo != null) textoTiempo.text = Mathf.CeilToInt(tiempoRestante).ToString();
        if (textoCervezas != null) textoCervezas.text = "Cervezas: 0";
        // Reactivar la mano del tutorial para la próxima partida
        //if (manoTutorial != null) manoTutorial.SetActive(true);

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

    void OcultarTutorialInmediato()
    {
        if (imageTutorial == null) return;

        if (corrutinaTutorial != null)
        {
            StopCoroutine(corrutinaTutorial);
            corrutinaTutorial = null;
        }
        imageTutorial.gameObject.SetActive(false);
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

    public void TomarCerveza()
    {
        if (juegoTerminado) return;
        cervezasTomadas++;
        textoCervezas.text = "Cervezas: " + cervezasTomadas;
        // manoTutorial.SetActive(false); // Ocultar tutorial en cuanto interactúe
        OcultarTutorialInmediato(); // Ocultamos el tutorial en cuanto el jugador interactúe
    }
    void TerminarMinijuego()
    {
        juegoTerminado = true;
        panelEsteMinijuego.SetActive(false);
        // Guardamos el resultado final antes de reiniciar
        int cervezasFinal = cervezasTomadas;
        // Ahora sí, reiniciamos para la próxima partida
        ReiniciarMinijuego();
        panelDivergencia.SetActive(true);
        StartCoroutine(TransicionAMinijuego(cervezasFinal));
    }
    //tartCoroutine(TransicionAMinijuego());

    IEnumerator TransicionAMinijuego(int cervezas)
    {
        yield return new WaitForSeconds(2f);
        panelDivergencia.SetActive(false);
        if (cervezas <= 3)
            panelMinijuegoA.SetActive(true);
        else
            panelMinijuegoB.SetActive(true);
    }
}
