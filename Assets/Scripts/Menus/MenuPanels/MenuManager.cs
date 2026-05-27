using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject menuPanel;
    public GameObject guiPanel; 
    public GameObject opcionesPanel;
    public GameObject salirPanel;

    [Header("Botones del Menú Principal")]
    public Button botonJugar;
    public Button botonOpciones;
    public Button botonSalir;

    [Header("Botones de Opciones")]
    public Button botonAplicar;
    public Button botonRegresarMenu;

    [Header("Botones de Confirmación")]
    public Button botonConfirmarSalir;
    public Button botonCancelarSalir;

    void Start()
    {
        ConfigurarBotones();

        MostrarSoloMenuPrincipal();
    }

    void ConfigurarBotones()
    {
        if (botonJugar != null)
            botonJugar.onClick.AddListener(Jugar);

        if (botonOpciones != null)
            botonOpciones.onClick.AddListener(AbrirOpciones);

        if (botonSalir != null)
            botonSalir.onClick.AddListener(AbrirConfirmacionSalir);

        if (botonAplicar != null)
            botonAplicar.onClick.AddListener(AplicarOpciones);

        if (botonRegresarMenu != null)
            botonRegresarMenu.onClick.AddListener(RegresarAlMenu);

        if (botonConfirmarSalir != null)
            botonConfirmarSalir.onClick.AddListener(SalirDelJuego);

        if (botonCancelarSalir != null)
            botonCancelarSalir.onClick.AddListener(CancelarSalida);
    }

    void MostrarSoloMenuPrincipal()
    {
        menuPanel.SetActive(true);
        guiPanel.SetActive(false);
        opcionesPanel.SetActive(false);
        salirPanel.SetActive(false);
    }

    void Jugar()
    {
        menuPanel.SetActive(false);
        guiPanel.SetActive(true);
        Debug.Log("Panel del juego activado");

    }

    void AbrirOpciones()
    {
        opcionesPanel.SetActive(true);
        Debug.Log("Panel de opciones abierto");
    }

    void AplicarOpciones()
    {
        Debug.Log("Aplicando opciones (funcionalidad pendiente)");
    }

    void RegresarAlMenu()
    {
        opcionesPanel.SetActive(false);
        Debug.Log("Regresando al menú principal");
    }

    void AbrirConfirmacionSalir()
    {
        salirPanel.SetActive(true);
        Debug.Log("Panel de confirmación de salida abierto");
    }

    void CancelarSalida()
    {
        salirPanel.SetActive(false);
        Debug.Log("Salida cancelada");
    }

    void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        //salir del juego
        Application.Quit();

    }
}