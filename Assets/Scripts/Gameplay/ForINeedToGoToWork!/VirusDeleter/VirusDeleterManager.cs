using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VirusDeleterManager : MonoBehaviour
{
    public static VirusDeleterManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;        // Panel raíz de TODO el minijuego
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA;           // Victoria
    public GameObject panelMinijuegoB;           // Derrota

    [Header("Paneles de Secuencia Narrativa")]
    public GameObject panelEscritorio;           // Jugador en el escritorio, jefe detrás
    public GameObject panelComputadora;          // Vista frontal de la computadora
    public GameObject panelJuego;                // Pantalla con icono antivirus y popups
    public float retrasoEscritorioAComputadora = 3f;
    public float retrasoComputadoraAJuego = 2f;

    [Header("Antivirus y Paneles de Confirmación")]
    public GameObject panelAntivirus;            // "¿Eliminar virus? Sí / No"
    public GameObject panelConfirmacion1;        // Segundo panel: "¿Seguro? Sí / No"
    public GameObject panelConfirmacion2;        // Tercer panel: "¿Última oportunidad? Sí / No"
    public GameObject panelFelicitacion;         // Panel final de éxito

    [Header("Pop‑ups")]
    public RectTransform popupPrefab;            // Prefab de pop‑up (debe tener botón para cerrar)
    public Transform[] puntosSpawnPopups;        // Posiciones posibles (RectTransforms vacíos)
    public float intervaloMinSpawn = 1.0f;
    public float intervaloMaxSpawn = 3.0f;
    public int tamanoPoolPopups = 8;

    [Header("Tiempo")]
    public float tiempoLimite = 45f;             // Tiempo total para completar la tarea
    public TextMeshProUGUI textoTiempo;

    // Estado interno
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;          // ¿Está en curso el panel de juego?
    private float tiempoActual;
    private bool antivirusAbierto = false;       // Para saber si ya se abrió el antivirus

    // Pool de pop‑ups
    private Queue<RectTransform> poolPopups = new Queue<RectTransform>();

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Comenzar la secuencia narrativa
        //StartCoroutine(SecuenciaNarrativa());
        // Crear pool de popups
        for (int i = 0; i < tamanoPoolPopups; i++)
        {
            RectTransform popup = Instantiate(popupPrefab, panelJuego.transform);
            popup.gameObject.SetActive(false);
            poolPopups.Enqueue(popup);
        }
        tiempoActual = tiempoLimite;
        ActualizarTextoTiempo();
    }

    void Update()
    {
        //Si panelEsteMinijuego esta activo, comenzar secuencia narrativa
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado)
        {
            StartCoroutine(SecuenciaNarrativa());
        }

        // Solo ejecutamos lógica del juego si el panel de juego está activo y no ha terminado
        if (!panelJuego.activeInHierarchy || juegoTerminado) return;

        // Si no se ha iniciado aún (puede que se active el panel pero aún estemos en transición)
        if (!juegoIniciado) return;

        // Temporizador
        tiempoActual -= Time.deltaTime;
        ActualizarTextoTiempo();

        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(false); // Derrota por tiempo
        }
    }

    // ---------- SECUENCIA NARRATIVA ----------
    IEnumerator SecuenciaNarrativa()
    {
        // 1. Mostrar escritorio
        panelEscritorio.SetActive(true);
        yield return new WaitForSeconds(retrasoEscritorioAComputadora);
        //panelEscritorio.SetActive(false);

        // 2. Mostrar computadora
        panelComputadora.SetActive(true);
        yield return new WaitForSeconds(retrasoComputadoraAJuego);
        //panelComputadora.SetActive(false);

        // 3. Mostrar panel de juego e iniciar
        panelJuego.SetActive(true);
        juegoIniciado = true;
        // Comenzar a spawnear pop‑ups
        StartCoroutine(SpawnearPopups());
    }

    // ---------- ANTIVIRUS Y CONFIRMACIONES ----------
    // Se llama desde el botón del icono del antivirus
    public void AbrirAntivirus()
    {
        if (juegoTerminado || !panelJuego.activeInHierarchy) return;
        antivirusAbierto = true;
        panelAntivirus.SetActive(true);
    }

    // Botón "Sí" en el primer panel
    public void ConfirmarAntivirus()
    {
        //panelAntivirus.SetActive(false);
        panelConfirmacion1.SetActive(true);
    }

    // Botón "No" en el primer panel
    public void CancelarAntivirus()
    {
        panelAntivirus.SetActive(false);
    }

    // Botón "Sí" en el segundo panel
    public void ConfirmarSegundo()
    {
        //panelConfirmacion1.SetActive(false);
        panelConfirmacion2.SetActive(true);
    }

    // Botón "No" en el segundo panel
    public void CancelarSegundo()
    {
        panelConfirmacion1.SetActive(false);
    }

    // Botón "Sí" en el tercer panel
    public void ConfirmarTercero()
    {
        //panelConfirmacion2.SetActive(false);
        // ¡Éxito!
        MostrarFelicitacion();
    }

    // Botón "No" en el tercer panel
    public void CancelarTercero()
    {
        panelConfirmacion2.SetActive(false);
        panelConfirmacion1.SetActive(false);
    }

    void MostrarFelicitacion()
    {
        juegoIniciado = false;
        // Desactivar pop‑ups (la corrutina se detendrá con juegoTerminado)
        panelFelicitacion.SetActive(true);
        // Pequeña pausa para disfrutar la felicitación y luego terminar
        StartCoroutine(FinalizarConExito());
    }

    IEnumerator FinalizarConExito()
    {
        yield return new WaitForSeconds(2f);
        TerminarMinijuego(true); // Victoria
    }

    // ---------- POP‑UPS ----------
    IEnumerator SpawnearPopups()
    {
        while (!juegoTerminado && panelJuego.activeInHierarchy)
        {
            // Esperar mientras no esté activo el panel de juego
            while (!panelJuego.activeInHierarchy && !juegoTerminado)
                yield return null;

            float espera = Random.Range(intervaloMinSpawn, intervaloMaxSpawn);
            yield return new WaitForSeconds(espera);

            if (juegoTerminado || !panelJuego.activeInHierarchy) continue;

            // Obtener pop‑up del pool
            RectTransform popup = ObtenerPopupPool();
            if (popup != null)
            {
                // Posición aleatoria
                int indice = Random.Range(0, puntosSpawnPopups.Length);
                popup.anchoredPosition = puntosSpawnPopups[indice].GetComponent<RectTransform>().anchoredPosition;
                popup.localRotation = Quaternion.identity;
                popup.gameObject.SetActive(true);

                // Asignar el manager al script del pop‑up (si tiene)
                Popup scriptPopup = popup.GetComponent<Popup>();
                if (scriptPopup) scriptPopup.Inicializar(this);
            }
        }
    }

    RectTransform ObtenerPopupPool()
    {
        if (poolPopups.Count > 0)
            return poolPopups.Dequeue();
        else
            return null; // Podrías instanciar uno extra si quieres
    }

    public void DevolverPopup(RectTransform popup)
    {
        popup.gameObject.SetActive(false);
        poolPopups.Enqueue(popup);
    }

    // ---------- UI AUXILIAR ----------
    void ActualizarTextoTiempo()
    {
        if (textoTiempo != null)
            textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();
    }

    // ---------- FINAL DEL MINIJUEGO ----------
    void TerminarMinijuego(bool sobrevivio)
    {
        juegoTerminado = true;
        juegoIniciado = false;
        StopAllCoroutines();

        // Ocultar todos los paneles internos
        panelJuego.SetActive(false);
        panelAntivirus.SetActive(false);
        panelConfirmacion1.SetActive(false);
        panelConfirmacion2.SetActive(false);
        panelFelicitacion.SetActive(false);
        panelEscritorio.SetActive(false);
        panelComputadora.SetActive(false);

        // Mostrar divergencia
        panelEsteMinijuego.SetActive(false);
        panelDivergencia.SetActive(true);

        StartCoroutine(Transicion(sobrevivio));
    }

    IEnumerator Transicion(bool sobrevivio)
    {
        yield return new WaitForSeconds(2f);
        panelDivergencia.SetActive(false);

        if (sobrevivio)
            panelMinijuegoA.SetActive(true);
        else
            panelMinijuegoB.SetActive(true);
    }

    // Método público para que los pop‑ups puedan cerrarse a sí mismos
    public void CerrarPopup(GameObject popup)
    {
        RectTransform rt = popup.GetComponent<RectTransform>();
        if (rt != null)
            DevolverPopup(rt);
    }
}