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
    private bool botonBloqueado = false;

    void Update()
    {

        if (!panelEsteMinijuego.activeInHierarchy)
            return;

        if (juegoTerminado) return;

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

        sueño = 0f;

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

        panelTransicion.SetActive(true);

        yield return new WaitForSeconds(3f);

        panelTransicion.SetActive(false);

        siguientePanel.SetActive(true);
    }
}