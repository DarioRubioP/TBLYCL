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
    public int tamanoPool = 10;

    // Estado
    private int vidas;
    private bool juegoTerminado = false;
    private float tiempoActual;
    private Vector2 posicionInicialAuto;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Guardamos posición inicial del coche
        posicionInicialAuto = autoRect.anchoredPosition;

        // Inicializar vidas
        vidas = imagenesVida.Count;
        ActualizarUI();

        // Crear pool de obstáculos
        for (int i = 0; i < tamanoPool; i++)
        {
            RectTransform obs = Instantiate(obstaculoPrefab, panelEsteMinijuego.transform);
            obs.gameObject.SetActive(false);
            poolObstaculos.Enqueue(obs);
        }

        // Comenzar a spawnear obstáculos
        StartCoroutine(SpawnearObstaculos());

        if (usarConduccionBorracha)
            StartCoroutine(RutinaDerrape());

        tiempoActual = tiempoLimite;

    }

    void Update()
    {
        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado) return;

        // Movimiento combinado: jugador + derrape
        float horizontal = Input.GetAxis("Horizontal");

        // Suavizar el derrape actual hacia el objetivo
        driftVelocity.x = Mathf.Lerp(driftVelocity.x, targetDriftSpeedX, Time.deltaTime * suavizadoDerrape);

        // Velocidad neta horizontal
        float netSpeedX = horizontal * velocidadMovimiento + driftVelocity.x;

        // Aplicar movimiento
        Vector2 mov = new Vector2(netSpeedX * Time.deltaTime, 0f);
        autoRect.anchoredPosition += mov;

        // Rotación del coche según el movimiento neto
        float maxSpeed = velocidadMovimiento + intensidadDerrape;
        float factor = Mathf.Clamp(netSpeedX / maxSpeed, -1f, 1f);
        float targetRot = -factor * maxAnguloRotacion; // negativo para que gire visualmente hacia la dirección

        Quaternion rotActual = autoRect.localRotation;
        Quaternion rotObjetivo = Quaternion.Euler(0f, 0f, targetRot);
        autoRect.localRotation = Quaternion.Slerp(rotActual, rotObjetivo, Time.deltaTime * velocidadRotacion);

        tiempoActual -= Time.deltaTime;
        textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();

        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(true); // true = sobrevivió
        }

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
            return null; // Podrías instanciar más si quieres
    }

    public void DevolverObstaculo(RectTransform obs)
    {
        obs.gameObject.SetActive(false);
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
        // Activar/desactivar imágenes de vida
        for (int i = 0; i < imagenesVida.Count; i++)
        {
            imagenesVida[i].enabled = i < vidas;
        }
    }

    // Método para comprobar superposición de dos RectTransform (AABB básico)
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

        // Desactivar minijuego
        panelEsteMinijuego.SetActive(false);
        // Mostrar divergencia
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
}