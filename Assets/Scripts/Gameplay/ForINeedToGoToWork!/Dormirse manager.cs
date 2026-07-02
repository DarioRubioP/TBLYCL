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

    private bool juegoTerminado = false;
    private bool juegoIniciado = false;        // <-- NUEVA BANDERA
    private bool botonBloqueado = false;

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

        panelTransicion.SetActive(true);
        yield return new WaitForSeconds(3f);
        panelTransicion.SetActive(false);
        siguientePanel.SetActive(true);
    }
}