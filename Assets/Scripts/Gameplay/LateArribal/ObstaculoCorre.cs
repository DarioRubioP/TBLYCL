using UnityEngine;

public class ObstaculoCorre : MonoBehaviour
{
    private CorreManager manager;
    private RectTransform rectTransform;
    private float velocidad;

    public void Inicializar(CorreManager mgr, float vel)
    {
        manager = mgr;
        velocidad = vel;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (manager == null) return;

        // Moverse hacia la izquierda
        rectTransform.anchoredPosition += Vector2.left * velocidad * Time.deltaTime;

        // Llegó a la zona de eliminación
        if (manager.RectOverlaps(rectTransform, manager.zonaEliminacion))
        {
            manager.DevolverObstaculo(rectTransform);
            return;
        }

        // Colisión con el jugador
        if (manager.RectOverlaps(rectTransform, manager.jugadorRect))
        {
            // Si el jugador NO es invulnerable (no está saltando), causa daño y se recicla
            if (!manager.EsInvulnerable)
            {
                manager.Perder();
                manager.DevolverObstaculo(rectTransform);
            }
            // Si es invulnerable (saltando), simplemente el obstáculo continúa como si nada
        }
    }
}