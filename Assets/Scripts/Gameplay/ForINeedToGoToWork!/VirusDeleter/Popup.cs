using UnityEngine;

public class Popup : MonoBehaviour
{
    private VirusDeleterManager manager;

    public void Inicializar(VirusDeleterManager mgr)
    {
        manager = mgr;
    }

    // Asignar este método al botón de cerrar del pop‑up
    public void Cerrar()
    {
        if (manager != null)
            manager.CerrarPopup(gameObject);
    }
}