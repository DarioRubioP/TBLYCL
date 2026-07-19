using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject menuPanel;
    public GameObject guiPanel; 
    //public GameObject opcionesPanel;
    public GameObject ADsPanel;
    public GameObject salirPanel;

    [Header("Botones del Menú Principal")]
    public Button botonJugar;
    //public Button botonOpciones;
    public Button botonADs;
    public Button botonSalir;

    [Header("Botones de Opciones")]
    //public Button botonAplicar;
    //public Button botonRegresarMenu;

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

        if (botonADs != null)
            botonADs.onClick.AddListener(AbrirADs);
        

        if (botonSalir != null)
            botonSalir.onClick.AddListener(AbrirConfirmacionSalir);

        if (botonConfirmarSalir != null)
            botonConfirmarSalir.onClick.AddListener(SalirDelJuego);

        if (botonCancelarSalir != null)
            botonCancelarSalir.onClick.AddListener(CancelarSalida);
    }

    void MostrarSoloMenuPrincipal()
    {
        menuPanel.SetActive(true);
        guiPanel.SetActive(false);
        salirPanel.SetActive(false);
    }

    void Jugar()
    {
        menuPanel.SetActive(false);
        guiPanel.SetActive(true);
        Debug.Log("Panel del juego activado");

    }

    void AbrirADs()
    {
        ADsPanel.SetActive(true);
        Debug.Log("Funcionalidad de anuncios (pendiente)");
    }

    void AplicarOpciones()
    {
        Debug.Log("Aplicando opciones (funcionalidad pendiente)");
    }

    void RegresarAlMenu()
    {
        //opcionesPanel.SetActive(false);
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

    public void ResetScener()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Reiniciando escena...");
    }

    public void ChangeSceneToNoADs()
    {
        SceneManager.LoadScene("NoADs");
        Debug.Log("Cambiando a la escena NoADs...");
    }
}