using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DormirseManager : MonoBehaviour
{
    [Header("UI Principal")]
    public GameObject panelEsteMinijuego;

    [Header("Player UI")]
    public RectTransform playerImagen;

    [Header("Botón Despertar")]
    public Button botonDespertar;

    [Header("Texto")]
    public TextMeshProUGUI textoPorcentaje;
    public TextMeshProUGUI textoResultado;

    [Header("Overlay Sueño")]
    public Image imagenSueño;

    [Header("Paneles de transición")]
    public GameObject panelTransicion;
    public GameObject panelExito;
    public GameObject panelFracaso;

    [Header("Configuración Sueño")]
    [Range(0, 100)]
    public float sueño = 0f;

    public float velocidadSueño = 10f;
    public float reducciónPorClick = 8f;

    [Header("Rotación")]
    public float rotacionMaxima = 60f;

    [Header("Duración")]
    public float duracionMinijuego = 15f;

    [Header("Tutorial – Imagen con latido y desvanecimiento")]
    public Image imageTutorial;                  // Asigná acá el componente Image de "ImageTutorial"
    public float duracionAntesDeDesvanecer = 5f;  // Segundos que late antes de empezar a desaparecer
    public float duracionDesvanecimiento = 1.5f;  // Duración del fade out
    public float velocidadLatido = 2f;            // Velocidad del pulso (mayor = más rápido)
    public float escalaMinima = 0.95f;
    public float escalaMaxima = 1.05f;

    private bool juegoTerminado = false;
    private bool juegoIniciado = false;        // <-- NUEVA BANDERA
    private bool botonBloqueado = false;
    private Coroutine corrutinaTutorial;

    void Start()
    {
        // No llamamos a ReiniciarMinijuego aquí; lo haremos cuando el panel se active (ver Update)
    }

    void Update()
    {
        // Si el panel está activo y aún no se ha iniciado el juego → reiniciar e iniciar
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado)
        {
            ReiniciarMinijuego();
            juegoIniciado = true;
            MostrarTutorial();
            return;
        }

        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado) return;

        // Tiempo del minijuego
        duracionMinijuego -= Time.deltaTime;

        // El sueño aumenta constantemente
        sueño += velocidadSueño * Time.deltaTime;
        sueño = Mathf.Clamp(sueño, 0f, 100f);

        // Actualizar texto
        textoPorcentaje.text = "Sueño: " + Mathf.RoundToInt(sueño) + "%";

        // Rotación del personaje
        float rotZ = Mathf.Lerp(0f, -rotacionMaxima, sueño / 100f);
        playerImagen.rotation = Quaternion.Euler(0f, 0f, rotZ);

        // Overlay oscuro/transparente aparece después del 30%
        if (sueño > 30f)
        {
            float alpha = Mathf.InverseLerp(30f, 100f, sueño);
            Color color = imagenSueño.color;
            color.a = alpha * 0.75f;
            imagenSueño.color = color;
        }

        // Si llega a 80% el botón deja de funcionar
        if (sueño >= 80f)
        {
            botonBloqueado = true;
            botonDespertar.interactable = false;
        }

        // Pierde si llega a 100%
        if (sueño >= 100f)
        {
            Perder();
        }

        // Gana si sobrevive el tiempo
        if (duracionMinijuego <= 0)
        {
            Ganar();
        }
    }

    // ----- NUEVO MÉTODO DE REINICIO -----
    public void ReiniciarMinijuego()
    {
        // Restablecer todos los valores a su estado inicial
        sueño = 0f;
        duracionMinijuego = 15f;
        juegoTerminado = false;
        botonBloqueado = false;

        // Reactivar el botón
        if (botonDespertar != null)
            botonDespertar.interactable = true;

        // Restablecer overlay (transparente)
        if (imagenSueño != null)
        {
            Color color = imagenSueño.color;
            color.a = 0f;
            imagenSueño.color = color;
        }

        // Restablecer rotación del jugador
        if (playerImagen != null)
            playerImagen.rotation = Quaternion.identity;

        // Limpiar textos
        if (textoPorcentaje != null)
            textoPorcentaje.text = "Sueño: 0%";
        if (textoResultado != null)
            textoResultado.text = "";
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

    public void PresionarDespertar()
    {
        if (juegoTerminado) return;
        if (botonBloqueado) return;

        sueño -= reducciónPorClick;
        if (sueño < 0f)
            sueño = 0f;
    }

    void Ganar()
    {
        juegoTerminado = true;
        textoResultado.text = "¡No te dormiste!";
        botonBloqueado = true;
        botonDespertar.interactable = false;

        StartCoroutine(SecuenciaFinal(panelExito));
    }

    void Perder()
    {
        juegoTerminado = true;
        textoResultado.text = "¡Te dormiste!";
        botonBloqueado = true;
        botonDespertar.interactable = false;

        StartCoroutine(SecuenciaFinal(panelFracaso));
    }

    IEnumerator SecuenciaFinal(GameObject siguientePanel)
    {
        yield return new WaitForSeconds(1.5f);

        panelEsteMinijuego.SetActive(false);
        juegoIniciado = false;                // <-- PERMITE REINICIAR LA PRÓXIMA VEZ

        // Ya no reseteamos valores aquí, se hará en ReiniciarMinijuego()

        // Apagamos y reseteamos la imagen de tutorial por si quedó a mitad de la animación
        if (corrutinaTutorial != null)
        {
            StopCoroutine(corrutinaTutorial);
            corrutinaTutorial = null;
        }
        if (imageTutorial != null) imageTutorial.gameObject.SetActive(false);

        panelTransicion.SetActive(true);
        yield return new WaitForSeconds(3f);
        panelTransicion.SetActive(false);
        siguientePanel.SetActive(true);
    }
}
