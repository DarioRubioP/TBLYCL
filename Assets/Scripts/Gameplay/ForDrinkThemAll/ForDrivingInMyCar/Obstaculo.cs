using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    private DrunkDrivingManager manager;
    private RectTransform rectTransform;
    private float velocidad;

    public void Inicializar(DrunkDrivingManager mgr)
    {
        manager = mgr;
        velocidad = mgr.velocidadObstaculo;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (manager == null) return;

        // Moverse hacia abajo
        rectTransform.anchoredPosition += Vector2.down * velocidad * Time.deltaTime;

        // Comprobar si llegó a la zona de eliminación
        if (manager.RectOverlaps(rectTransform, manager.zonaEliminacion))
        {
            manager.DevolverObstaculo(rectTransform);
            return;
        }

        // Comprobar colisión con el coche
        if (manager.RectOverlaps(rectTransform, manager.autoRect))
        {
            manager.PerderVida();
            manager.DevolverObstaculo(rectTransform);
        }
    }
}