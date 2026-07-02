using UnityEngine;

public class ObjetoQueCae : MonoBehaviour
{
    private CajaManager manager;
    private RectTransform rectTransform;
    private float velocidad;
    private bool atrapado = false;

    public void Inicializar(CajaManager mgr, float vel)
    {
        manager = mgr;
        velocidad = vel;
        atrapado = false;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (manager == null) return;

        rectTransform.anchoredPosition += Vector2.down * velocidad * Time.deltaTime;

        // Si aún no ha sido atrapado, comprobar colisión con la caja
        if (!atrapado && manager.RectOverlaps(rectTransform, manager.cajaRect))
        {
            atrapado = true;
            manager.DevolverObjeto(rectTransform);  // Atrapado: se guarda
            return;
        }

        // Si llega al suelo sin ser atrapado, pierde vida
        if (manager.RectOverlaps(rectTransform, manager.zonaSuelo))
        {
            if (!atrapado)
            {
                manager.PerderVida();
            }
            manager.DevolverObjeto(rectTransform); // Se recicla siempre
        }
    }
}