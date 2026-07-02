using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VirusDeleterManager : MonoBehaviour
{
    public static VirusDeleterManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA;           // Victoria
    public GameObject panelMinijuegoB;           // Derrota

    [Header("Paneles de Secuencia Narrativa")]
    public GameObject panelEscritorio;
    public GameObject panelComputadora;
    public GameObject panelJuego;
    public float retrasoEscritorioAComputadora = 3f;
    public float retrasoComputadoraAJuego = 2f;

    [Header("Antivirus y Paneles de Confirmación")]
    public GameObject panelAntivirus;
    public GameObject panelConfirmacion1;
    public GameObject panelConfirmacion2;
    public GameObject panelFelicitacion;

    [Header("Pop‑ups")]
    public RectTransform popupPrefab;
    public Transform[] puntosSpawnPopups;
    public float intervaloMinSpawn = 1.0f;
    public float intervaloMaxSpawn = 3.0f;
    public int tamanoPoolPopups = 8;

    [Header("Tiempo")]
    public float tiempoLimite = 45f;
    public TextMeshProUGUI textoTiempo;

    // Estado interno
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    private float tiempoActual;
    private bool antivirusAbierto = false;

    // Pool de pop‑ups
    private Queue<RectTransform> poolPopups = new Queue<RectTransform>();

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Crear el pool inicial de popups (solo una vez)
        for (int i = 0; i < tamanoPoolPopups; i++)
        {
            RectTransform popup = Instantiate(popupPrefab, panelJuego.transform);
            popup.gameObject.SetActive(false);
            poolPopups.Enqueue(popup);
        }
    }

    void Update()
    {
        // Si el panel principal se activa y aún no se ha iniciado el juego → reiniciar y empezar
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado)
        {
            ReiniciarMinijuego();
            StartCoroutine(SecuenciaNarrativa());
            return;
        }

        // Si el panel de juego no está activo o el juego terminó, no hacemos nada
        if (!panelJuego.activeInHierarchy || juegoTerminado) return;

        // Temporizador
        tiempoActual -= Time.deltaTime;
        ActualizarTextoTiempo();

        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(false);
        }
    }

    // ----- NUEVO MÉTODO DE REINICIO -----
    void ReiniciarMinijuego()
    {
        // Restablecer variables de estado
        juegoTerminado = false;
        juegoIniciado = false;              // Se pondrá true al final de la secuencia narrativa
        antivirusAbierto = false;
        tiempoActual = tiempoLimite;
        ActualizarTextoTiempo();

        // Desactivar todos los paneles internos
        panelEscritorio.SetActive(false);
        panelComputadora.SetActive(false);
        panelJuego.SetActive(false);
        panelAntivirus.SetActive(false);
        panelConfirmacion1.SetActive(false);
        panelConfirmacion2.SetActive(false);
        panelFelicitacion.SetActive(false);

        // Reciclar todos los popups activos y rehacer el pool
        Popup[] popups = panelJuego.GetComponentsInChildren<Popup>(true);
        foreach (Popup p in popups)
        {
            p.gameObject.SetActive(false);
        }
        poolPopups.Clear();
        foreach (Popup p in popups)
        {
            RectTransform rt = p.GetComponent<RectTransform>();
            if (rt != null) poolPopups.Enqueue(rt);
        }
        // Si faltan popups para completar el tamaño del pool, instanciar más
        while (poolPopups.Count < tamanoPoolPopups)
        {
            RectTransform nuevo = Instantiate(popupPrefab, panelJuego.transform);
            nuevo.gameObject.SetActive(false);
            poolPopups.Enqueue(nuevo);
        }
    }

    // ---------- SECUENCIA NARRATIVA ----------
    IEnumerator SecuenciaNarrativa()
    {
        panelEscritorio.SetActive(true);
        yield return new WaitForSeconds(retrasoEscritorioAComputadora);

        panelComputadora.SetActive(true);
        yield return new WaitForSeconds(retrasoComputadoraAJuego);

        panelJuego.SetActive(true);
        juegoIniciado = true;
        StartCoroutine(SpawnearPopups());
    }

    // ---------- ANTIVIRUS Y CONFIRMACIONES ----------
    public void AbrirAntivirus()
    {
        if (juegoTerminado || !panelJuego.activeInHierarchy) return;
        antivirusAbierto = true;
        panelAntivirus.SetActive(true);
    }

    public void ConfirmarAntivirus()
    {
        panelConfirmacion1.SetActive(true);
    }

    public void CancelarAntivirus()
    {
        panelAntivirus.SetActive(false);
    }

    public void ConfirmarSegundo()
    {
        panelConfirmacion2.SetActive(true);
    }

    public void CancelarSegundo()
    {
        panelConfirmacion1.SetActive(false);
    }

    public void ConfirmarTercero()
    {
        MostrarFelicitacion();
    }

    public void CancelarTercero()
    {
        panelConfirmacion2.SetActive(false);
        panelConfirmacion1.SetActive(false);
    }

    void MostrarFelicitacion()
    {
        juegoIniciado = false;
        panelFelicitacion.SetActive(true);
        StartCoroutine(FinalizarConExito());
    }

    IEnumerator FinalizarConExito()
    {
        yield return new WaitForSeconds(2f);
        TerminarMinijuego(true);
    }

    // ---------- POP‑UPS ----------
    IEnumerator SpawnearPopups()
    {
        while (!juegoTerminado && panelJuego.activeInHierarchy)
        {
            while (!panelJuego.activeInHierarchy && !juegoTerminado)
                yield return null;

            float espera = Random.Range(intervaloMinSpawn, intervaloMaxSpawn);
            yield return new WaitForSeconds(espera);

            if (juegoTerminado || !panelJuego.activeInHierarchy) continue;

            RectTransform popup = ObtenerPopupPool();
            if (popup != null)
            {
                int indice = Random.Range(0, puntosSpawnPopups.Length);
                popup.anchoredPosition = puntosSpawnPopups[indice].GetComponent<RectTransform>().anchoredPosition;
                popup.localRotation = Quaternion.identity;
                popup.gameObject.SetActive(true);

                Popup scriptPopup = popup.GetComponent<Popup>();
                if (scriptPopup) scriptPopup.Inicializar(this);
            }
        }
    }

    RectTransform ObtenerPopupPool()
    {
        if (poolPopups.Count > 0)
            return poolPopups.Dequeue();
        return null;
    }

    public void DevolverPopup(RectTransform popup)
    {
        popup.gameObject.SetActive(false);
        poolPopups.Enqueue(popup);
    }

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

        // Desactivar paneles internos (el panel principal se apagará luego)
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

    public void CerrarPopup(GameObject popup)
    {
        RectTransform rt = popup.GetComponent<RectTransform>();
        if (rt != null)
            DevolverPopup(rt);
    }
}