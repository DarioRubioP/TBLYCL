using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool estaPresionado = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        estaPresionado = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        estaPresionado = false;
    }
}