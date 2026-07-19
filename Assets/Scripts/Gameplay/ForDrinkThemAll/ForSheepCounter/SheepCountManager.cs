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
    public float alturaEvitarObstaculo = 40f; // Umbral de altura Y para esquivar

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
    public int poolSize = 3;                  // Tamaño del pool

    [Header("Tiempo")]
    public float tiempoLimite = 30f;
    public TextMeshProUGUI textoTiempo;

    [Header("Temblor")]
    public float duracionTemblor = 0.3f;
    public float intensidadTemblor = 10f;

    [Header("Tutorial – Imagen con latido y desvanecimiento")]
    public Image imageTutorial;                  // Asigná acá el componente Image de "ImageTutorial"
    public float duracionAntesDeDesvanecer = 5f;  // Segundos que late antes de empezar a desaparecer
    public float duracionDesvanecimiento = 1.5f;  // Duración del fade out
    public float velocidadLatido = 2f;            // Velocidad del pulso (mayor = más rápido)
    public float escalaMinima = 0.95f;
    public float escalaMaxima = 1.05f;

    // Pool interno
    private Queue<Oveja> poolOvejas = new Queue<Oveja>();
    private Oveja ovejaActual;

    // Estado
    public bool juegoTerminado { get; private set; }
    private bool juegoIniciado = false;        // <-- NUEVA BANDERA
    private int vidas;
    private float tiempoActual;
    private Coroutine rutinaTemblor;
    private Coroutine corrutinaTutorial;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Solo creamos el pool de ovejas una vez, pero sin activar ninguna
        for (int i = 0; i < poolSize; i++)
        {
            RectTransform obj = Instantiate(ovejaPrefab, panelEsteMinijuego.transform);
            Oveja oveja = obj.GetComponent<Oveja>();
            obj.gameObject.SetActive(false);
            poolOvejas.Enqueue(oveja);
        }
        // El juego se inicia cuando el panel se active (ver Update)
    }

    void Update()
    {
        // Si el panel está activo y aún no se ha iniciado el juego → reiniciar e iniciar
        if (panelEsteMinijuego.activeInHierarchy && !juegoIniciado)
        {
            ReiniciarMinijuego();
            juegoIniciado = true;
            return;
        }

        // Si el panel no está activo o el juego terminó → no hacer nada
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

    // ----- NUEVO MÉTODO DE REINICIO -----
    public void ReiniciarMinijuego()
    {
        // Restablecer estado
        juegoTerminado = false;
        vidas = imagenesVida.Count;
        tiempoActual = tiempoLimite;
        ActualizarUI();

        // Detener cualquier temblor residual
        if (rutinaTemblor != null)
        {
            StopCoroutine(rutinaTemblor);
            rutinaTemblor = null;
        }
        // Asegurar la posición original del jugador (por si quedó desplazado por un temblor)
        // (No tenemos almacenada la posición original, pero podemos asumir que es la actual antes de modificarse; 
        //  como el temblor la restaura, no debería ser necesario. Por seguridad, podrías guardarla al inicio.)

        // Limpiar todas las ovejas activas y rehacer el pool
        Oveja[] ovejas = panelEsteMinijuego.GetComponentsInChildren<Oveja>(true);
        foreach (Oveja o in ovejas)
        {
            o.gameObject.SetActive(false);
        }
        poolOvejas.Clear();
        foreach (Oveja o in ovejas)
        {
            poolOvejas.Enqueue(o);
        }
        // Si faltan ovejas para alcanzar el tamaño del pool, instanciar más
        while (poolOvejas.Count < poolSize)
        {
            RectTransform obj = Instantiate(ovejaPrefab, panelEsteMinijuego.transform);
            Oveja oveja = obj.GetComponent<Oveja>();
            obj.gameObject.SetActive(false);
            poolOvejas.Enqueue(oveja);
        }

        // Spawnear la primera oveja
        SpawnOveja();

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
        juegoIniciado = false;              // <-- PERMITE REINICIAR
        StopAllCoroutines();
        corrutinaTutorial = null; // StopAllCoroutines ya detuvo la corrutina del tutorial
        if (imageTutorial != null) imageTutorial.gameObject.SetActive(false);

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
