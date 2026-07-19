using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CorreManager : MonoBehaviour
{
    public static CorreManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA; // Victoria (sobrevive)
    public GameObject panelMinijuegoB; // Derrota (chocó o tiempo)

    [Header("Jugador")]
    public RectTransform jugadorRect;
    public float alturaSalto = 120f;
    public float duracionSalto = 0.6f;
    public AnimationCurve curvaSalto;
    public float alturaEvitarObstaculo = 60f;   // Umbral para esquivar

    [Header("Rotación (caminata)")]
    public float anguloBalanceo = 15f;
    public float velocidadBalanceo = 0.2f;

    [Header("Vidas")]
    public List<Image> imagenesVida;            // 3 imágenes de corazón

    [Header("Obstáculos")]
    public RectTransform obstaculoPrefab;
    public Transform[] puntosSpawn;
    public float velocidadObstaculo = 200f;
    public float intervaloMinSpawn = 1.0f;
    public float intervaloMaxSpawn = 2.5f;
    public RectTransform zonaEliminacion;
    public int tamanoPool = 8;

    [Header("Tiempo")]
    public float tiempoLimite = 30f;
    public TextMeshProUGUI textoTiempo;

    [Header("Tutorial – Imagen con latido y desvanecimiento")]
    public Image imageTutorial;                  // Asigná acá el componente Image de "ImageTutorial"
    public float duracionAntesDeDesvanecer = 5f;  // Segundos que late antes de empezar a desaparecer
    public float duracionDesvanecimiento = 1.5f;  // Duración del fade out
    public float velocidadLatido = 2f;            // Velocidad del pulso (mayor = más rápido)
    public float escalaMinima = 0.95f;
    public float escalaMaxima = 1.05f;

    // Estado interno
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    private float tiempoActual;
    private bool saltando = false;
    private float baseY;
    private int vidas;
    public bool EsInvulnerable => saltando;
    private Coroutine corrutinaTutorial;

    // Balanceo
    private float timerBalanceo;
    private int direccionBalanceo = 1;

    // Pool obstáculos
    private Queue<RectTransform> poolObstaculos = new Queue<RectTransform>();

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Solo creamos el pool inicial, pero no iniciamos el juego aún.
        for (int i = 0; i < tamanoPool; i++)
        {
            RectTransform obs = Instantiate(obstaculoPrefab, panelEsteMinijuego.transform);
            obs.gameObject.SetActive(false);
            poolObstaculos.Enqueue(obs);
        }
    }

    void Update()
    {
        // Si el panel principal se activa y aún no se ha iniciado el juego → reiniciar y empezar
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado)
        {
            ReiniciarMinijuego();
            juegoIniciado = true;
            StartCoroutine(SpawnearObstaculos());
            MostrarTutorial();
            return;
        }

        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado || !juegoIniciado) return;

        // Temporizador
        tiempoActual -= Time.deltaTime;
        ActualizarTextoTiempo();
        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(true);
            return;
        }

        // Rotación de caminata
        timerBalanceo += Time.deltaTime;
        if (timerBalanceo >= velocidadBalanceo)
        {
            timerBalanceo = 0f;
            direccionBalanceo *= -1;
        }
        float targetZ = direccionBalanceo * anguloBalanceo;
        jugadorRect.localRotation = Quaternion.Euler(0f, 0f, targetZ);
    }

    // ----- NUEVO MÉTODO DE REINICIO -----
    void ReiniciarMinijuego()
    {
        // Restablecer estado general
        juegoTerminado = false;
        juegoIniciado = false;          // Se pondrá true en Update después de reiniciar
        saltando = false;
        tiempoActual = tiempoLimite;
        vidas = imagenesVida.Count;
        baseY = jugadorRect.anchoredPosition.y;

        // Restablecer posición y rotación del jugador
        jugadorRect.anchoredPosition = new Vector2(jugadorRect.anchoredPosition.x, baseY);
        jugadorRect.localRotation = Quaternion.identity;

        // Resetear balanceo
        timerBalanceo = 0f;
        direccionBalanceo = 1;

        // Reciclar todos los obstáculos activos y rehacer el pool
        ObstaculoCorre[] obstaculos = panelEsteMinijuego.GetComponentsInChildren<ObstaculoCorre>(true);
        foreach (ObstaculoCorre obs in obstaculos)
        {
            obs.gameObject.SetActive(false);
        }
        poolObstaculos.Clear();
        foreach (ObstaculoCorre obs in obstaculos)
        {
            RectTransform rt = obs.GetComponent<RectTransform>();
            if (rt != null) poolObstaculos.Enqueue(rt);
        }
        // Si faltan para completar el pool, instanciar más
        while (poolObstaculos.Count < tamanoPool)
        {
            RectTransform nuevo = Instantiate(obstaculoPrefab, panelEsteMinijuego.transform);
            nuevo.gameObject.SetActive(false);
            poolObstaculos.Enqueue(nuevo);
        }

        ActualizarUI();
        ActualizarTextoTiempo();
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

    // ---------- SALTO ----------
    public void Saltar()
    {
        if (!panelEsteMinijuego.activeInHierarchy || juegoTerminado || saltando) return;
        saltando = true;
        StartCoroutine(CoroutineSalto());
    }

    IEnumerator CoroutineSalto()
    {
        float tiempo = 0f;
        while (tiempo < duracionSalto)
        {
            float deltaY = curvaSalto.Evaluate(tiempo / duracionSalto) * alturaSalto;
            jugadorRect.anchoredPosition = new Vector2(jugadorRect.anchoredPosition.x, baseY + deltaY);
            tiempo += Time.deltaTime;
            yield return null;
        }
        jugadorRect.anchoredPosition = new Vector2(jugadorRect.anchoredPosition.x, baseY);
        saltando = false;
    }

    // ---------- VIDAS ----------
    public void Perder()
    {
        if (juegoTerminado || saltando) return;

        vidas--;
        ActualizarUI();

        if (vidas <= 0)
        {
            TerminarMinijuego(false);
        }
    }

    void ActualizarUI()
    {
        for (int i = 0; i < imagenesVida.Count; i++)
        {
            imagenesVida[i].enabled = i < vidas;
        }
    }

    public bool JugadorEstaElevado()
    {
        return jugadorRect.anchoredPosition.y >= baseY + alturaEvitarObstaculo;
    }

    // ---------- POOLING ----------
    IEnumerator SpawnearObstaculos()
    {
        while (!juegoTerminado && juegoIniciado)
        {
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

                ObstaculoCorre script = obs.GetComponent<ObstaculoCorre>();
                if (script) script.Inicializar(this, velocidadObstaculo);
            }
        }
    }

    RectTransform ObtenerObstaculoPool()
    {
        if (poolObstaculos.Count > 0)
            return poolObstaculos.Dequeue();
        return null;
    }

    public void DevolverObstaculo(RectTransform obs)
    {
        obs.gameObject.SetActive(false);
        poolObstaculos.Enqueue(obs);
    }

    void ActualizarTextoTiempo()
    {
        if (textoTiempo != null)
            textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();
    }

    // ---------- FINAL ----------
    void TerminarMinijuego(bool sobrevivio)
    {
        juegoTerminado = true;
        juegoIniciado = false;
        StopAllCoroutines();

        // Desactivar obstáculos restantes (el reinicio los reciclará después)
        foreach (var obs in poolObstaculos)
        {
            if (obs.gameObject.activeSelf)
                obs.gameObject.SetActive(false);
        }

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
}
