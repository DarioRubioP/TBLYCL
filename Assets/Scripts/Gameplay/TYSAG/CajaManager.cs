using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CajaManager : MonoBehaviour
{
    public static CajaManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA; // Victoria
    public GameObject panelMinijuegoB; // Derrota

    [Header("Paneles de Secuencia Narrativa")]
    public GameObject panelNarrativo;
    public RectTransform posInicioCompanero;
    public RectTransform posMediaCompanero;
    public RectTransform posFinalCompanero;
    public GameObject imagenCompanero1;
    public GameObject imagenCompanero2;
    public GameObject imagenCajaVacia;
    public float velocidadCaminata = 150f;
    public float pausaEnMedia = 0.5f;

    [Header("Panel de Juego")]
    public GameObject panelJuego;
    public RectTransform cajaRect;               // La caja que arrastramos
    public float limiteIzquierdo = -300f;
    public float limiteDerecho = 300f;

    [Header("Vidas")]
    public List<Image> imagenesVida;

    [Header("Objetos que caen")]
    public RectTransform objetoPrefab;           // Prefab de objeto (taza, grapadora, etc.)
    public Transform[] puntosSpawn;              // Posiciones superiores de caída
    public float velocidadCaida = 200f;
    public float intervaloMinSpawn = 1.0f;
    public float intervaloMaxSpawn = 2.5f;
    public RectTransform zonaSuelo;              // Zona inferior (si llega aquí sin atrapar, pierdes vida)
    public int tamanoPool = 10;

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
    private int vidas;
    private Coroutine corrutinaTutorial;

    // Pooling
    private Queue<RectTransform> poolObjetos = new Queue<RectTransform>();

    // --- NUEVAS VARIABLES PARA EL RESETEO ---
    private Vector2 posicionInicialCaja;
    private bool panelEstabaActivo = false;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Guardamos la posición inicial de la caja
        if (cajaRect != null)
        {
            posicionInicialCaja = cajaRect.anchoredPosition;
        }

        tiempoActual = tiempoLimite;
        ActualizarTextoTiempo();
        vidas = imagenesVida.Count;
        ActualizarUI();

        for (int i = 0; i < tamanoPool; i++)
        {
            RectTransform obj = Instantiate(objetoPrefab, panelJuego.transform);
            obj.gameObject.SetActive(false);
            poolObjetos.Enqueue(obj);
        }
    }

    void Update()
    {
        bool panelActivoAhora = panelEsteMinijuego.activeInHierarchy;

        // --- DETECCIÓN DE APAGADO (RESETEO) ---
        if (!panelActivoAhora && panelEstabaActivo)
        {
            ResetearValores();
        }
        panelEstabaActivo = panelActivoAhora;

        // Iniciar secuencia si el panel se enciende
        if (panelActivoAhora && !juegoIniciado && !juegoTerminado)
        {
            IniciarMinijuego();
        }

        if (!panelJuego.activeInHierarchy || juegoTerminado || !juegoIniciado) return;

        tiempoActual -= Time.deltaTime;
        ActualizarTextoTiempo();
        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(true);
        }
    }

    public void IniciarMinijuego()
    {
        if (juegoIniciado) return;
        StartCoroutine(SecuenciaNarrativa());
    }

    // ---------- SECUENCIA NARRATIVA ----------
    IEnumerator SecuenciaNarrativa()
    {
        juegoIniciado = true;
        panelNarrativo.SetActive(true);
        imagenCompanero1.SetActive(true);
        imagenCompanero1.GetComponent<RectTransform>().anchoredPosition = posInicioCompanero.anchoredPosition;
        imagenCompanero2.SetActive(false);
        imagenCajaVacia.SetActive(false);

        // Mover compañero1 a media
        while (Vector2.Distance(imagenCompanero1.GetComponent<RectTransform>().anchoredPosition, posMediaCompanero.anchoredPosition) > 1f)
        {
            imagenCompanero1.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(
                imagenCompanero1.GetComponent<RectTransform>().anchoredPosition,
                posMediaCompanero.anchoredPosition,
                velocidadCaminata * Time.deltaTime);
            yield return null;
        }

        imagenCajaVacia.SetActive(true);
        imagenCompanero1.SetActive(false);
        imagenCompanero2.SetActive(true);
        imagenCompanero2.GetComponent<RectTransform>().anchoredPosition = posMediaCompanero.anchoredPosition;

        yield return new WaitForSeconds(pausaEnMedia);

        // Mover compañero2 a final
        while (Vector2.Distance(imagenCompanero2.GetComponent<RectTransform>().anchoredPosition, posFinalCompanero.anchoredPosition) > 1f)
        {
            imagenCompanero2.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(
                imagenCompanero2.GetComponent<RectTransform>().anchoredPosition,
                posFinalCompanero.anchoredPosition,
                velocidadCaminata * Time.deltaTime);
            yield return null;
        }

        imagenCompanero2.SetActive(false);
        panelJuego.SetActive(true);

        StartCoroutine(SpawnearObjetos());

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

    // ---------- ARRASTRE DE LA CAJA ----------
    public void MoverCaja(Vector2 nuevaPos)
    {
        if (juegoTerminado || !panelJuego.activeInHierarchy) return;
        float clampedX = Mathf.Clamp(nuevaPos.x, limiteIzquierdo, limiteDerecho);
        cajaRect.anchoredPosition = new Vector2(clampedX, cajaRect.anchoredPosition.y);
    }

    // ---------- VIDAS ----------
    public void PerderVida()
    {
        if (juegoTerminado) return;
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
            if (imagenesVida[i] != null)
                imagenesVida[i].enabled = i < vidas;
        }
    }

    // ---------- POOLING ----------
    IEnumerator SpawnearObjetos()
    {
        while (!juegoTerminado && panelJuego.activeInHierarchy)
        {
            while (!panelJuego.activeInHierarchy && !juegoTerminado)
                yield return null;

            float espera = Random.Range(intervaloMinSpawn, intervaloMaxSpawn);
            yield return new WaitForSeconds(espera);

            if (juegoTerminado || !panelJuego.activeInHierarchy) continue;

            RectTransform obj = ObtenerObjetoPool();
            if (obj != null)
            {
                int indice = Random.Range(0, puntosSpawn.Length);
                obj.anchoredPosition = puntosSpawn[indice].GetComponent<RectTransform>().anchoredPosition;
                obj.gameObject.SetActive(true);

                ObjetoQueCae script = obj.GetComponent<ObjetoQueCae>();
                if (script) script.Inicializar(this, velocidadCaida);
            }
        }
    }

    RectTransform ObtenerObjetoPool()
    {
        if (poolObjetos.Count > 0)
            return poolObjetos.Dequeue();
        return null;
    }

    public void DevolverObjeto(RectTransform obj)
    {
        obj.gameObject.SetActive(false);
        poolObjetos.Enqueue(obj);
    }

    // ---------- FINAL ----------
    void TerminarMinijuego(bool sobrevivio)
    {
        juegoTerminado = true;
        StopAllCoroutines();

        panelJuego.SetActive(false);
        panelNarrativo.SetActive(false);
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

    void ActualizarTextoTiempo()
    {
        if (textoTiempo != null)
            textoTiempo.text = Mathf.CeilToInt(tiempoActual).ToString();
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

    // ---------- RESETEO TOTAL DE VALORES ----------
    private void ResetearValores()
    {
        // 1. Restaurar lógica
        juegoTerminado = false;
        juegoIniciado = false;
        tiempoActual = tiempoLimite;
        vidas = imagenesVida.Count;

        ActualizarTextoTiempo();
        ActualizarUI();

        // 2. Restaurar posiciones fijas
        if (cajaRect != null)
        {
            cajaRect.anchoredPosition = posicionInicialCaja;
        }

        // 3. Limpiar objetos del Pool que quedaron cayendo en pantalla
        // Buscamos cualquier script "ObjetoQueCae" que esté activo en la jerarquía del panel de juego
        if (panelJuego != null)
        {
            ObjetoQueCae[] objetosActivos = panelJuego.GetComponentsInChildren<ObjetoQueCae>(false);
            foreach (ObjetoQueCae obj in objetosActivos)
            {
                DevolverObjeto(obj.GetComponent<RectTransform>());
            }
        }

        // 4. Apagar todos los elementos visuales base
        if (panelJuego != null) panelJuego.SetActive(false);
        if (panelNarrativo != null) panelNarrativo.SetActive(false);
        if (imagenCompanero1 != null) imagenCompanero1.SetActive(false);
        if (imagenCompanero2 != null) imagenCompanero2.SetActive(false);
        if (imagenCajaVacia != null) imagenCajaVacia.SetActive(false);

        // 5. Apagar y resetear la imagen de tutorial por si quedó a mitad de la animación
        if (corrutinaTutorial != null)
        {
            StopCoroutine(corrutinaTutorial);
            corrutinaTutorial = null;
        }
        if (imageTutorial != null) imageTutorial.gameObject.SetActive(false);
    }
}
