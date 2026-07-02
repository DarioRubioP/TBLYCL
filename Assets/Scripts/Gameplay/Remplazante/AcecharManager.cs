using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AcecharManager : MonoBehaviour
{
    public static AcecharManager Instancia;

    [Header("UI Principal – Minijuego")]
    public GameObject panelEsteMinijuego;
    public GameObject panelDivergencia;
    public GameObject panelMinijuegoA; // Victoria
    public GameObject panelMinijuegoB; // Derrota

    [Header("Jugador")]
    public RectTransform jugadorContenedor;     // Objeto vacío que se moverá
    public GameObject imagenDisimulo;           // Jugador en modo normal
    public GameObject imagenAcercamiento;       // Jugador moviéndose
    public float velocidadMovimiento = 50f;     // Velocidad en píxeles por segundo

    [Header("Objetivo (Compañero)")]
    public RectTransform objetivoRect;          // Posición del compañero (no se mueve)
    public GameObject imagenTrabajando;         // Compañero de espaldas
    public GameObject imagenSospecha;           // Compañero mirando hacia atrás
    public float intervaloCambioMin = 2f;       // Tiempo mínimo entre cambios de estado
    public float intervaloCambioMax = 5f;       // Tiempo máximo
    public GameObject ContenedorCompañero;

    [Header("Coyote Time")]
    public float coyoteTime = 0.3f;             // Tiempo extra que puede seguir moviéndose tras ser visto

    [Header("Secuencia Final")]
    public GameObject imagenGolpe;              // Jugador pegando al compañero
    public GameObject imagenCaida;              // Compañero en el suelo
    public GameObject imagenRemplazo;           // Jugador ocupando el lugar
    public float duracionGolpe = 1f;            // Tiempo mostrando el golpe antes de cambiar a caída/remplazo
    public float duracionRemplazo = 3f;         // Tiempo final antes de terminar el juego

    [Header("Tiempo")]
    public float tiempoLimite = 15f;
    public TextMeshProUGUI textoTiempo;

    // Estado interno
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    private float tiempoActual;
    private bool presionado = false;            // true = botón presionado (acechando)
    private bool moviendose = false;            // se actualiza cada frame según presionado
    private bool objetivoSospechando = false;   // true = compañero mirando
    private bool visto = false;                 // se activa si nos pillan moviéndonos
    private float coyoteTimer = 0f;

    // --- NUEVAS VARIABLES PARA EL RESETEO ---
    private Vector2 posicionInicialJugador;
    private bool panelEstabaActivo = false;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Guardar la posición inicial para poder restaurarla después
        if (jugadorContenedor != null)
        {
            posicionInicialJugador = jugadorContenedor.anchoredPosition;
        }

        tiempoActual = tiempoLimite;
        ActualizarTextoTiempo();
        ActualizarEstadoJugador();
        objetivoSospechando = false;
        ActualizarEstadoObjetivo();
    }

    void Update()
    {
        bool panelActivoAhora = panelEsteMinijuego.activeInHierarchy;

        // --- DETECCIÓN DE APAGADO (RESETEO) ---
        // Detectamos el momento exacto en que el panel pasa de Activo a Inactivo
        if (!panelActivoAhora && panelEstabaActivo)
        {
            ResetearValores();
        }
        panelEstabaActivo = panelActivoAhora;

        // Activar juego cuando el panel principal se enciende
        if (panelActivoAhora && !juegoIniciado && !juegoTerminado)
        {
            juegoIniciado = true;
            StartCoroutine(RutinaCambioCompanero());
        }

        if (!panelActivoAhora || juegoTerminado || !juegoIniciado) return;

        // Actualizar movimiento según si el botón está presionado
        moviendose = presionado;
        ActualizarEstadoJugador();

        // Temporizador
        tiempoActual -= Time.deltaTime;
        ActualizarTextoTiempo();
        if (tiempoActual <= 0f)
        {
            TerminarMinijuego(false);
            return;
        }

        // Movimiento del jugador si está en modo acercamiento
        if (moviendose && !visto)
        {
            Vector2 direccion = (objetivoRect.anchoredPosition - jugadorContenedor.anchoredPosition).normalized;
            jugadorContenedor.anchoredPosition += direccion * velocidadMovimiento * Time.deltaTime;

            // ¿Ha llegado al objetivo?
            if (Vector2.Distance(jugadorContenedor.anchoredPosition, objetivoRect.anchoredPosition) < 30f)
            {
                IniciarSecuenciaFinal();
                return;
            }
        }

        // Lógica de ser visto
        if (moviendose && objetivoSospechando && !visto)
        {
            visto = true;
            coyoteTimer = coyoteTime;
        }

        if (visto)
        {
            coyoteTimer -= Time.deltaTime;
            // Si durante el coyote time el jugador deja de moverse o el compañero deja de sospechar, se salva
            if (!moviendose || !objetivoSospechando)
            {
                visto = false;
                coyoteTimer = 0f;
            }
            else if (coyoteTimer <= 0f)
            {
                TerminarMinijuego(false);
            }
        }
    }

    // ---------- MÉTODOS PARA EL BOTÓN DE MANTENER PRESIONADO ----------
    public void EmpezarAcecho()
    {
        presionado = true;
    }

    public void TerminarAcecho()
    {
        presionado = false;
        if (visto)
        {
            visto = false;
            coyoteTimer = 0f;
        }
    }

    void ActualizarEstadoJugador()
    {
        if (imagenDisimulo != null) imagenDisimulo.SetActive(!moviendose);
        if (imagenAcercamiento != null) imagenAcercamiento.SetActive(moviendose);
    }

    // ---------- RUTINA DEL COMPAÑERO ----------
    IEnumerator RutinaCambioCompanero()
    {
        while (!juegoTerminado && juegoIniciado)
        {
            while (!panelEsteMinijuego.activeInHierarchy && !juegoTerminado)
                yield return null;

            float espera = Random.Range(intervaloCambioMin, intervaloCambioMax);
            yield return new WaitForSeconds(espera);

            if (juegoTerminado || !panelEsteMinijuego.activeInHierarchy) continue;

            objetivoSospechando = !objetivoSospechando;
            ActualizarEstadoObjetivo();
        }
    }

    void ActualizarEstadoObjetivo()
    {
        if (imagenTrabajando != null) imagenTrabajando.SetActive(!objetivoSospechando);
        if (imagenSospecha != null) imagenSospecha.SetActive(objetivoSospechando);
    }

    // ---------- SECUENCIA FINAL ----------
    void IniciarSecuenciaFinal()
    {
        juegoIniciado = false;
        if (jugadorContenedor != null) jugadorContenedor.gameObject.SetActive(false);
        if (imagenGolpe != null) imagenGolpe.SetActive(true);

        //tiempo deja de contar mientras se muestra la secuencia final
        tiempoActual = 999;

        StartCoroutine(SecuenciaFinal());
    }

    IEnumerator SecuenciaFinal()
    {
        yield return new WaitForSeconds(duracionGolpe);

        if (ContenedorCompañero != null) ContenedorCompañero.SetActive(false);

        if (imagenTrabajando != null) imagenTrabajando.SetActive(false);
        if (imagenSospecha != null) imagenSospecha.SetActive(false);
        if (imagenGolpe != null) imagenGolpe.SetActive(false);

        if (imagenCaida != null) imagenCaida.SetActive(true);
        if (imagenRemplazo != null) imagenRemplazo.SetActive(true);

        yield return new WaitForSeconds(duracionRemplazo);

        if (imagenCaida != null) imagenCaida.SetActive(false);
        if (imagenRemplazo != null) imagenRemplazo.SetActive(false);

        TerminarMinijuego(true);
    }

    // ---------- UI ----------
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
        StopAllCoroutines(); // Detenemos la rutina del compañero y cualquier otra activa

        // Apagar imágenes por seguridad (el reseteo las volverá a ordenar en el próximo frame)
        if (jugadorContenedor != null) jugadorContenedor.gameObject.SetActive(false);
        if (imagenTrabajando != null) imagenTrabajando.SetActive(false);
        if (imagenSospecha != null) imagenSospecha.SetActive(false);
        if (imagenGolpe != null) imagenGolpe.SetActive(false);
        if (imagenCaida != null) imagenCaida.SetActive(false);
        if (imagenRemplazo != null) imagenRemplazo.SetActive(false);

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

    // ---------- RESETEO TOTAL DE VALORES ----------
    private void ResetearValores()
    {
        // 1. Restaurar variables lógicas
        juegoTerminado = false;
        juegoIniciado = false;
        tiempoActual = tiempoLimite;
        presionado = false;
        moviendose = false;
        objetivoSospechando = false;
        visto = false;
        coyoteTimer = 0f;

        ActualizarTextoTiempo();

        // 2. Restaurar posiciones y activar contenedores base
        if (jugadorContenedor != null)
        {
            jugadorContenedor.anchoredPosition = posicionInicialJugador;
            jugadorContenedor.gameObject.SetActive(true);
        }
        if (ContenedorCompañero != null)
        {
            ContenedorCompañero.SetActive(true);
        }

        // 3. Restaurar estado visual inicial (Imágenes)
        if (imagenDisimulo != null) imagenDisimulo.SetActive(true);
        if (imagenAcercamiento != null) imagenAcercamiento.SetActive(false);
        if (imagenTrabajando != null) imagenTrabajando.SetActive(true);
        if (imagenSospecha != null) imagenSospecha.SetActive(false);

        if (imagenGolpe != null) imagenGolpe.SetActive(false);
        if (imagenCaida != null) imagenCaida.SetActive(false);
        if (imagenRemplazo != null) imagenRemplazo.SetActive(false);
    }
}