using UnityEngine;
using UnityEngine.EventSystems;

public class PersonajeTarget : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // Si soltamos algo sobre el personaje y tiene el script de la cerveza...
        if (eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<CervezaDraggable>() != null)
        {
            // Le decimos al Manager que nos tomamos una cerveza
            DrinkThemAllManager.Instancia.TomarCerveza();
        }
    }
}