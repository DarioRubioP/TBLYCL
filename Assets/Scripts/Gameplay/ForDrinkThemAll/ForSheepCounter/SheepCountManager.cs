using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SheepCountManager : MonoBehaviour
{
    public static SheepCountManager Instancia;

    [Header("UI Principal")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA; // Victoria (sobrevive el tiempo)
    public GameObject panelMinijuegoB; // Derrota (pierde todas las vidas)

    [Header("Elementos del juego")]
    public RectTransform imagenJugador;       // Imagen del jugador acostado (para temblar)
    public RectTransform puntoInicio;         // Punto de spawn de la oveja (derecha)
    public RectTransform puntoFin;            // Punto de meta (izquierda)
    public RectTransform obstaculoRect;       // Imagen del obstáculo (valla)
    public float alturaEvitarObstaculo = 40f; // Umbral de altura Y para esquivar (debe ser mayor que la parte alta del obstáculo)

    [Header("Velocidad Oveja")]
    public float velocidadInicial = 100f;
    public float velocidadFinal = 300f;

    [Header("Salto")]
    public float alturaSalto = 80f;
    public float duracionSaltoInicial = 3f;
    public float duracionSaltoFinal = 1f;
    public AnimationCurve curvaSalto;


    [Header("Vidas")]
    public List<Image> imagenesVida;          // 3 imágenes de vida

    [Header("Pool Ovejas")]
    public RectTransform ovejaPrefab;         // Prefab de la imagen de oveja
    public int poolSize = 3;                  // Tamaño del pool (solo una activa)

    [Header("Tiempo")]
    public float tiempoLimite = 30f;
    public TextMeshProUGUI textoTiempo;

    [Header("Temblor")]
    public float duracionTemblor = 0.3f;
    public float intensidadTemblor = 10f;

    // Pool interno
    private Queue<Oveja> poolOvejas = new Queue<Oveja>();
    private Oveja ovejaActual;

    // Estado
    public bool juegoTerminado { get; private set; }
    private int vidas;
    private float tiempoActual;
    private Coroutine rutinaTemblor;


    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        tiempoActual = tiempoLimite;
        juegoTerminado = false;
        vidas = imagenesVida.Count;
        ActualizarUI();

        // Crear pool de ovejas
        for (int i = 0; i < poolSize; i++)
        {
            RectTransform obj = Instantiate(ovejaPrefab, panelEsteMinijuego.transform);
            Oveja oveja = obj.GetComponent<Oveja>();
            obj.gameObject.SetActive(false);
            poolOvejas.Enqueue(oveja);
        }

        // Spawnear la primera oveja
        SpawnOveja();
    }

    void Update()
    {
        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado) return;

        // Input: clic para saltar
        if (Input.GetMouseButtonDown(0))
        {
            if (ovejaActual != null && ovejaActual.activa)
                ovejaActual.Saltar();
        }

        // Cuenta regresiva
        tiempoActual -= Time.deltaTime;
        textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();

        if (tiempoActual <= 0f)
            TerminarMinijuego(true);
    }

    public void SpawnOveja()
    {
        if (juegoTerminado) return;

        if (poolOvejas.Count == 0)
        {
            RectTransform obj = Instantiate(ovejaPrefab, panelEsteMinijuego.transform);
            Oveja oveja = obj.GetComponent<Oveja>();
            obj.gameObject.SetActive(false);
            poolOvejas.Enqueue(oveja);
        }

        Oveja nueva = poolOvejas.Dequeue();
        nueva.gameObject.SetActive(true);
        nueva.Inicializar(this, puntoInicio.anchoredPosition, GetVelocidadActual(), GetDuracionSaltoActual());
        ovejaActual = nueva;
    }


    float GetVelocidadActual()
    {
        // factor: 0 al inicio (tiempoLimite seg), 1 cuando quedan 3 seg
        float factor = (tiempoLimite - tiempoActual) / (tiempoLimite - 3f);
        factor = Mathf.Clamp01(factor);
        return Mathf.Lerp(velocidadInicial, velocidadFinal, factor);
    }

    float GetDuracionSaltoActual()
    {
        float factor = (tiempoLimite - tiempoActual) / (tiempoLimite - 3f);
        factor = Mathf.Clamp01(factor);
        return Mathf.Lerp(duracionSaltoInicial, duracionSaltoFinal, factor);
    }

    public void DevolverOveja(Oveja oveja)
    {
        oveja.gameObject.SetActive(false);
        poolOvejas.Enqueue(oveja);
        if (ovejaActual == oveja)
            ovejaActual = null;
    }

    public void PerderVida()
    {
        if (juegoTerminado) return;

        vidas--;
        ActualizarUI();

        // Activar temblor del jugador
        if (rutinaTemblor != null) StopCoroutine(rutinaTemblor);
        rutinaTemblor = StartCoroutine(TemblorJugador());

        if (vidas <= 0)
            TerminarMinijuego(false);
    }

    IEnumerator TemblorJugador()
    {
        Vector2 posOriginal = imagenJugador.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duracionTemblor)
        {
            float offsetX = Random.Range(-intensidadTemblor, intensidadTemblor);
            float offsetY = Random.Range(-intensidadTemblor, intensidadTemblor);
            imagenJugador.anchoredPosition = posOriginal + new Vector2(offsetX, offsetY);
            elapsed += Time.deltaTime;
            yield return null;
        }
        imagenJugador.anchoredPosition = posOriginal;
    }

    void ActualizarUI()
    {
        for (int i = 0; i < imagenesVida.Count; i++)
        {
            imagenesVida[i].enabled = i < vidas;
        }
    }

    // Detección de colisiones por rectángulos (AABB en coordenadas de mundo)
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
}