using UnityEngine;
using UnityEngine.EventSystems;

public class Arrastrable : MonoBehaviour, IDragHandler
{
    private RectTransform rectTransform;
    private CajaManager manager;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        manager = CajaManager.Instancia;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (manager == null) return;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            manager.MoverCaja(localPoint);
        }
    }
}