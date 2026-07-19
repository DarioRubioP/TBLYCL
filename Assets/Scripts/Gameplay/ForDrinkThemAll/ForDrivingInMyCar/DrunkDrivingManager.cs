using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DrunkDrivingManager : MonoBehaviour
{
    public static DrunkDrivingManager Instancia;

    [Header("UI Principal")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA; // Victoria (si sobrevive el tiempo)
    public GameObject panelMinijuegoB; // Derrota (si pierde todas las vidas)

    [Header("Elementos del juego")]
    public RectTransform autoRect;          // La imagen del coche
    public float velocidadMovimiento = 200f;
    public float velocidadRotacion = 5f;    // Suavizado rotación
    public float maxAnguloRotacion = 15f;   // Máximo ángulo al girar

    public RectTransform limiteIzquierdo;   // Imagen del borde izquierdo (para choque)
    public RectTransform limiteDerecho;     // Imagen del borde derecho
    public List<Image> imagenesVida;        // 3 imágenes de vida

    [Header("Obstáculos")]
    public RectTransform obstaculoPrefab;   // Prefab del obstáculo (imagen)
    public Transform[] puntosSpawn;         // Posiciones superiores (pueden ser RectTransforms vacíos)
    public float velocidadObstaculo = 150f;
    public float intervaloMinSpawn = 0.8f;
    public float intervaloMaxSpawn = 2.0f;
    public RectTransform zonaEliminacion;   // Imagen inferior que borra obstáculos

    [Header("Tiempo")]
    public float tiempoLimite = 30f;
    public TextMeshProUGUI textoTiempo;

    [Header("Conducción Borracha")]
    public bool usarConduccionBorracha = true;   // Activar/desactivar el efecto
    public float intensidadDerrape = 80f;        // Velocidad extra máxima (unidades/segundo)
    public float tiempoCambioMin = 0.8f;         // Mínimo tiempo entre cambios de dirección
    public float tiempoCambioMax = 2.5f;         // Máximo tiempo entre cambios
    public float suavizadoDerrape = 2.5f;        // Qué tan rápido gira hacia el nuevo objetivo

    private Vector2 driftVelocity;               // Velocidad de derrape actual (solo se usa X)
    private float targetDriftSpeedX;             // Velocidad objetivo a la que tiende

    // Pooling
    private Queue<RectTransform> poolObstaculos = new Queue<RectTransform>();
    private List<RectTransform> todosLosObstaculos = new List<RectTransform>(); // Lista fija para rastrear y resetear el pool
    public int tamanoPool = 10;

    [Header("Tutorial – Imagen con latido y desvanecimiento")]
    public Image imageTutorial;                  // Asigná acá el componente Image de "ImageTutorial"
    public float duracionAntesDeDesvanecer = 5f;  // Segundos que late antes de empezar a desaparecer
    public float duracionDesvanecimiento = 1.5f;  // Duración del fade out
    public float velocidadLatido = 2f;            // Velocidad del pulso (mayor = más rápido)
    public float escalaMinima = 0.95f;
    public float escalaMaxima = 1.05f;

    // Estado
    private int vidas;
    private bool juegoTerminado = false;
    private float tiempoActual;
    private Vector2 posicionInicialAuto;
    private Coroutine corrutinaTutorial;

    // Input de botones UI
    private bool botonIzquierdaPresionado = false;
    private bool botonDerechaPresionado = false;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Guardamos posición inicial del coche
        posicionInicialAuto = autoRect.anchoredPosition;

        // Crear pool de obstáculos y guardar sus referencias estables
        for (int i = 0; i < tamanoPool; i++)
        {
            RectTransform obs = Instantiate(obstaculoPrefab, panelEsteMinijuego.transform);
            obs.gameObject.SetActive(false);
            poolObstaculos.Enqueue(obs);
            todosLosObstaculos.Add(obs);
        }

        // Inicializamos los valores por primera vez
        ReiniciarMinijuego();
    }

    void Update()
    {
        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado) return;

        // Input combinado: teclado + botones UI
        float inputTeclado = Input.GetAxis("Horizontal");
        float inputBotones = 0f;
        if (botonDerechaPresionado) inputBotones += 1f;
        if (botonIzquierdaPresionado) inputBotones -= 1f;
        float horizontal = Mathf.Clamp(inputTeclado + inputBotones, -1f, 1f);

        // Suavizar derrape
        driftVelocity.x = Mathf.Lerp(driftVelocity.x, targetDriftSpeedX, Time.deltaTime * suavizadoDerrape);

        // Velocidad neta
        float netSpeedX = horizontal * velocidadMovimiento + driftVelocity.x;

        // Aplicar movimiento
        Vector2 mov = new Vector2(netSpeedX * Time.deltaTime, 0f);
        autoRect.anchoredPosition += mov;

        // Limitar dentro del panel
        RectTransform panelRect = (RectTransform)autoRect.parent; // El panel contenedor
        float panelHalfWidth = panelRect.rect.width / 2f;
        float carHalfWidth = autoRect.rect.width / 2f;
        float minX = -panelHalfWidth + carHalfWidth;
        float maxX = panelHalfWidth - carHalfWidth;

        Vector2 pos = autoRect.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        autoRect.anchoredPosition = pos;

        // Rotación
        float maxSpeed = velocidadMovimiento + intensidadDerrape;
        float factor = Mathf.Clamp(netSpeedX / maxSpeed, -1f, 1f);
        float targetRot = -factor * maxAnguloRotacion;
        Quaternion rotActual = autoRect.localRotation;
        Quaternion rotObjetivo = Quaternion.Euler(0f, 0f, targetRot);
        autoRect.localRotation = Quaternion.Slerp(rotActual, rotObjetivo, Time.deltaTime * velocidadRotacion);

        // Temporizador
        tiempoActual -= Time.deltaTime;
        textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();

        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(true);
        }
    }

    // ----- MÉTODO DE REINICIO -----
    public void ReiniciarMinijuego()
    {
        juegoTerminado = false;
        vidas = imagenesVida.Count;
        tiempoActual = tiempoLimite;

        // UI y Textos
        ActualizarUI();
        if (textoTiempo != null) textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();

        // Reset de posición y rotación del coche
        if (autoRect != null)
        {
            autoRect.anchoredPosition = posicionInicialAuto;
            autoRect.localRotation = Quaternion.identity;
        }

        // Reset de físicas/controles de borrachera
        driftVelocity = Vector2.zero;
        targetDriftSpeedX = 0f;
        botonIzquierdaPresionado = false;
        botonDerechaPresionado = false;

        // Limpiar pantalla y reconstruir la cola del Pool de obstáculos
        poolObstaculos.Clear();
        foreach (RectTransform obs in todosLosObstaculos)
        {
            if (obs != null)
            {
                obs.gameObject.SetActive(false);
                poolObstaculos.Enqueue(obs);
            }
        }

        // Volver a encender las corrutinas base (se pausarán solas si el panel está desactivado)
        StopAllCoroutines();
        corrutinaTutorial = null; // StopAllCoroutines ya detuvo la corrutina del tutorial, si había una
        StartCoroutine(SpawnearObstaculos());
        if (usarConduccionBorracha)
            StartCoroutine(RutinaDerrape());

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

    IEnumerator RutinaDerrape()
    {
        while (!juegoTerminado)
        {
            // Esperar mientras el panel no esté activo
            while (!panelEsteMinijuego.activeInHierarchy && !juegoTerminado)
                yield return null;

            float espera = Random.Range(tiempoCambioMin, tiempoCambioMax);
            yield return new WaitForSeconds(espera);

            if (juegoTerminado || !panelEsteMinijuego.activeInHierarchy) continue;

            targetDriftSpeedX = Random.Range(-intensidadDerrape, intensidadDerrape);
        }
    }

    IEnumerator SpawnearObstaculos()
    {
        while (!juegoTerminado)
        {
            // Esperar mientras el panel no esté activo
            while (!panelEsteMinijuego.activeInHierarchy && !juegoTerminado)
                yield return null;

            float espera = Random.Range(intervaloMinSpawn, intervaloMaxSpawn);
            yield return new WaitForSeconds(espera);

            if (juegoTerminado || !panelEsteMinijuego.activeInHierarchy) continue;

            RectTransform obs = ObtenerObstaculoPool();
            if (obs != null)
            {
                int indice = Random.Range(0, puntosSpawn.Length);
                obs.anchoredPosition = puntosSpawn[indice].GetComponent<RectTransform>().anchoredPosition;
                obs.localRotation = Quaternion.identity;
                obs.gameObject.SetActive(true);

                Obstaculo obsScript = obs.GetComponent<Obstaculo>();
                if (obsScript) obsScript.Inicializar(this);
            }
        }
    }

    RectTransform ObtenerObstaculoPool()
    {
        if (poolObstaculos.Count > 0)
            return poolObstaculos.Dequeue();
        else
            return null;
    }

    public void DevolverObstaculo(RectTransform obs)
    {
        obs.gameObject.SetActive(false);
        if (!poolObstaculos.Contains(obs)) // Evitar duplicados en colas por choques raros
            poolObstaculos.Enqueue(obs);
    }

    public void PerderVida()
    {
        if (juegoTerminado) return;

        vidas--;
        ActualizarUI();

        // Resetear estado borracho al reaparecer
        driftVelocity = Vector2.zero;
        targetDriftSpeedX = 0f;

        autoRect.anchoredPosition = posicionInicialAuto;
        autoRect.localRotation = Quaternion.identity;

        if (vidas <= 0)
            TerminarMinijuego(false);
    }

    void ActualizarUI()
    {
        for (int i = 0; i < imagenesVida.Count; i++)
        {
            imagenesVida[i].enabled = i < vidas;
        }
    }

    // ----- BOTONES UI -----
    public void PresionarIzquierda() { botonIzquierdaPresionado = true; }
    public void SoltarIzquierda() { botonIzquierdaPresionado = false; }
    public void PresionarDerecha() { botonDerechaPresionado = true; }
    public void SoltarDerecha() { botonDerechaPresionado = false; }

    public bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Vector3[] cornersA = new Vector3[4];
        Vector3[] cornersB = new Vector3[4];
        a.GetWorldCorners(cornersA);
        b.GetWorldCorners(cornersB);

        Rect rectA = new Rect(cornersA[0].x, cornersA[0].y, cornersA[2].x - cornersA[0].x, cornersA[2].y - cornersA[0].y);
        Rect rectB = new Rect(cornersB[0].x, cornersB[0].y, cornersB[2].x - cornersB[0].x, cornersB[2].y - cornersB[0].y);

        return rectA.Overlaps(rectB);
    }

    void TerminarMinijuego(bool sobrevivio)
    {
        juegoTerminado = true;
        StopAllCoroutines();
        corrutinaTutorial = null; // StopAllCoroutines ya detuvo la corrutina del tutorial

        // Desactivar minijuego
        panelEsteMinijuego.SetActive(false);

        // << CAMBIO AQUÍ >>: Reseteamos todo inmediatamente al apagar el panel
        ReiniciarMinijuego();

        // Mostrar divergencia
        panelDivergencia.SetActive(true);

        // Iniciamos la transición (esta corrutina no se cancelará porque se llama DESPUÉS del ReiniciarMinijuego)
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
}
