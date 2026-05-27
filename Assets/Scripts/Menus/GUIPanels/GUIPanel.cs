using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    [Header("Paneles del GUI")]
    public GameObject guiPanel;           // Panel principal del GUI
    public GameObject pausaPanel;         // Panel de pausa
    public GameObject opcionesPanel;      // Panel de opciones (reutilizado del menú)

    [Header("Paneles del Menú Principal")]
    public GameObject menuPanel;          // Referencia al panel del menú principal

    [Header("Botones")]
    public Button botonPausa;             // Botón de pausa en el GUI Panel
    public Button botonContinue;          // Botón Continue en el Pausa Panel
    public Button botonOpcionesPausa;     // Botón Opciones en el Pausa Panel
    public Button botonSalirPausa;        // Botón Salir en el Pausa Panel

    [Header("Botones de Opciones (reutilizados)")]
    public Button botonAplicarOpciones;   // Botón Aplicar en Opciones Panel
    public Button botonRegresarOpciones;  // Botón Regresar en Opciones Panel

    private bool isGamePaused = false;
    private MenuManager menuManager;      // Referencia al MenuManager para reutilizar métodos

    void Start()
    {
        // Buscar el MenuManager en la escena
        menuManager = FindFirstObjectByType<MenuManager>();

        // Si no encuentra MenuManager, intentar encontrar por nombre
        if (menuManager == null)
        {
            GameObject menuManagerObj = GameObject.Find("MenuManager");
            if (menuManagerObj != null)
                menuManager = menuManagerObj.GetComponent<MenuManager>();
        }

        ConfigurarBotones();
        ReanudarJuego(); // Asegurar que el juego comienza sin pausa
    }

    void Update()
    {
        
    }

    void ConfigurarBotones()
    {
        // Botón de pausa en GUI Panel
        if (botonPausa != null)
            botonPausa.onClick.AddListener(PausarJuego);

        // Botones del Pausa Panel
        if (botonContinue != null)
            botonContinue.onClick.AddListener(ReanudarJuego);

        if (botonOpcionesPausa != null)
            botonOpcionesPausa.onClick.AddListener(AbrirOpcionesDesdePausa);

        if (botonSalirPausa != null)
            botonSalirPausa.onClick.AddListener(SalirAlMenu);

        // Botones del panel de opciones (reutilizados)
        if (botonAplicarOpciones != null)
            botonAplicarOpciones.onClick.AddListener(AplicarOpciones);

        if (botonRegresarOpciones != null)
            botonRegresarOpciones.onClick.AddListener(CerrarOpciones);
    }

    // Método para pausar el juego
    void PausarJuego()
    {
        isGamePaused = true;
        Time.timeScale = 0f; // Congelar el tiempo del juego

        // Abrir panel de pausa
        pausaPanel.SetActive(true);

        Debug.Log("Juego pausado");
    }

    // Método para reanudar el juego
    void ReanudarJuego()
    {
        isGamePaused = false;
        Time.timeScale = 1f; // Restaurar el tiempo del juego

        // Cerrar todos los paneles de pausa y opciones
        pausaPanel.SetActive(false);

        // Si el panel de opciones estaba abierto, cerrarlo también
        if (opcionesPanel != null)
            opcionesPanel.SetActive(false);

        Debug.Log("Juego reanudado");
    }

    // Método para abrir opciones desde el panel de pausa
    void AbrirOpcionesDesdePausa()
    {
        // Reutilizamos el mismo panel de opciones del menú principal
        if (opcionesPanel != null)
        {
            opcionesPanel.SetActive(true);
            Debug.Log("Opciones abiertas desde el menú de pausa");
        }
        else
        {
            Debug.LogWarning("No se ha asignado el panel de opciones en GUIManager");

            // Intentar usar el panel de opciones del MenuManager si existe
            if (menuManager != null && menuManager.opcionesPanel != null)
            {
                menuManager.opcionesPanel.SetActive(true);
            }
        }
    }

    // Método para aplicar opciones (sin funcionalidad por ahora)
    void AplicarOpciones()
    {
        Debug.Log("Aplicando opciones desde el menú de pausa");
        // Aquí puedes implementar la lógica para aplicar configuraciones
    }

    // Método para cerrar el panel de opciones
    void CerrarOpciones()
    {
        if (opcionesPanel != null)
            opcionesPanel.SetActive(false);
        else if (menuManager != null && menuManager.opcionesPanel != null)
            menuManager.opcionesPanel.SetActive(false);

        Debug.Log("Panel de opciones cerrado");
    }

    // Método para salir al menú principal
    void SalirAlMenu()
    {
        // Primero restauramos el tiempo
        Time.timeScale = 1f;
        isGamePaused = false;

        // Desactivar GUI Panel y sus paneles hijos
        guiPanel.SetActive(false);
        pausaPanel.SetActive(false);

        // Cerrar panel de opciones si está abierto
        if (opcionesPanel != null)
            opcionesPanel.SetActive(false);
        else if (menuManager != null && menuManager.opcionesPanel != null)
            menuManager.opcionesPanel.SetActive(false);

        // Activar el menú principal
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
        else if (menuManager != null && menuManager.menuPanel != null)
        {
            menuManager.menuPanel.SetActive(true);
        }

        Debug.Log("Saliendo al menú principal");
    }

    // Método público para verificar si el juego está en pausa
    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    // Método para limpiar el estado al destruir el objeto
    void OnDestroy()
    {
        // Asegurar que el tiempo se restaura si el objeto se destruye
        Time.timeScale = 1f;
    }
}