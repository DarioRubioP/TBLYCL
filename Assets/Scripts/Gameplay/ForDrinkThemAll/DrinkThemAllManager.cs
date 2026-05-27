using UnityEngine;
using TMPro;
using System.Collections;

public class DrinkThemAllManager : MonoBehaviour
{
    public static DrinkThemAllManager Instancia;

    [Header("UI Elementos de este Minijuego")]
    public GameObject panelEsteMinijuego; // El panel contenedor de TODO este minijuego actual
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoCervezas;
    public GameObject manoTutorial;

    [Header("Paneles de Divergencia")]
    public GameObject panelDivergencia; // El panel general que dice "Cargando Divergencia"
    public GameObject panelMinijuegoA;  // Siguiente minijuego si tomó 3 o menos
    public GameObject panelMinijuegoB;  // Siguiente minijuego si tomó más de 3

    [Header("Configuración")]
    public float tiempoRestante = 15f;

    private int cervezasTomadas = 0;
    private bool juegoTerminado = false;

    void Awake()
    {
        Instancia = this;
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
        if (manoTutorial.activeSelf && (Input.GetMouseButtonDown(0) || tiempoRestante < 12f))
        {
            manoTutorial.SetActive(false);
        }

        // Al agotarse el tiempo
        if (tiempoRestante <= 0)
        {
            TerminarMinijuego();
        }
    }

    public void TomarCerveza()
    {
        if (juegoTerminado) return;

        cervezasTomadas++;
        textoCervezas.text = "Cervezas: " + cervezasTomadas;
        manoTutorial.SetActive(false); // Ocultar tutorial en cuanto interactúe
    }

    void TerminarMinijuego()
    {
        juegoTerminado = true;

        // Desactivamos el minijuego de la cerveza para limpiar la pantalla
        panelEsteMinijuego.SetActive(false);

        // Activamos el letrero de "Cargando Divergencia"
        panelDivergencia.SetActive(true);

        StartCoroutine(TransicionAMinijuego());
    }

    IEnumerator TransicionAMinijuego()
    {
        // Esperamos 2 segundos mostrando el aviso "Cargando Divergencia..."
        yield return new WaitForSeconds(2f);

        // Apagamos el letrero de carga
        panelDivergencia.SetActive(false);

        // Evaluamos las cervezas y encendemos el panel del Siguiente Minijuego
        if (cervezasTomadas <= 3)
        {
            panelMinijuegoA.SetActive(true);
        }
        else
        {
            panelMinijuegoB.SetActive(true);
        }
    }
}