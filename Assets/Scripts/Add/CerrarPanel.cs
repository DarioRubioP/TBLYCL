using UnityEngine;

public class CerrarPanel : MonoBehaviour
{
    // Arrastra el panel que quieres cerrar desde el Inspector a esta variable
    public GameObject panel;

    // Esta es la función que asignaremos al evento OnClick del botón
    public void OcultarPanel()
    {
        if (panel != null)
        {
            // Desactiva el GameObject del panel, haciéndolo invisible
            panel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No has asignado ningún panel al script CerrarPanel.");
        }
    }
}