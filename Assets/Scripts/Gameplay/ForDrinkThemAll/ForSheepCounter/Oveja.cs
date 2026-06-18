using UnityEngine;
using System.Collections;

public class Oveja : MonoBehaviour
{
    private SheepCountManager manager;
    private RectTransform rectTransform;
    private float velocidadHorizontal;
    private float duracionSalto;       // <-- NUEVO
    private float baseY;
    private bool saltando = false;
    private bool haChocado = false;

    [System.NonSerialized]
    public bool activa = false;

    public void Inicializar(SheepCountManager mgr, Vector2 posInicio, float velocidad, float duracionSalto)
    {
        manager = mgr;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = posInicio;
        baseY = posInicio.y;
        velocidadHorizontal = velocidad;
        this.duracionSalto = duracionSalto;  // <-- Guardamos la duración actual
        saltando = false;
        haChocado = false;
        activa = true;
    }

    void Update()
    {
        if (manager == null || !activa || manager.juegoTerminado) return;

        // Movimiento constante hacia la izquierda
        rectTransform.anchoredPosition += Vector2.left * velocidadHorizontal * Time.deltaTime;

        // ¿Llegó a la meta?
        if (rectTransform.anchoredPosition.x <= manager.puntoFin.anchoredPosition.x)
        {
            Reciclar();
            manager.SpawnOveja();
            return;
        }

        // Colisión con el obstáculo
        if (!haChocado && manager.RectOverlaps(rectTransform, manager.obstaculoRect))
        {
            // Consideramos que evita el obstáculo si está saltando y su altura Y >= umbral
            if (!saltando || rectTransform.anchoredPosition.y < manager.alturaEvitarObstaculo)
            {
                haChocado = true;
                manager.PerderVida();
                Reciclar();
                manager.SpawnOveja();
            }
        }
    }

    public void Saltar()
    {
        if (saltando) return;
        saltando = true;
        StartCoroutine(CoroutineSalto());
    }

    IEnumerator CoroutineSalto()
    {
        // if (saltando) yield break;  <-- ELIMINAR ESTA LÍNEA
        saltando = true;                // <-- ESTA LÍNEA YA LA TIENES EN Saltar(), pero puedes dejarla aquí para asegurar (o moverla aquí)
        float tiempo = 0f;
        while (tiempo < duracionSalto)
        {
            float deltaY = manager.curvaSalto.Evaluate(tiempo / duracionSalto) * manager.alturaSalto;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, baseY + deltaY);
            tiempo += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, baseY);
        saltando = false;
    }

    void Reciclar()
    {
        activa = false;
        manager.DevolverOveja(this);
    }
}