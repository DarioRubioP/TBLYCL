using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollInfinite : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public float sensibilidad = 0.5f; // Velocidad de desplazamiento
    private Material material;
    private CelularManager manager;

    void Start()
    {
        // Obtener la instancia del manager
        manager = CelularManager.Instancia;
        // Conseguir el material de la imagen (asegúrate de que sea una copia o usa material instanciado)
        material = GetComponent<UnityEngine.UI.Image>().material;
        if (material != null)
            material = Instantiate(material); // Para no modificar el asset original
        GetComponent<UnityEngine.UI.Image>().material = material;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (manager != null)
            manager.EmpezarScroll();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (material == null) return;

        // El desplazamiento en Y del puntero (delta) se traduce en offset de textura
        float deltaY = eventData.delta.y * sensibilidad * 0.01f; // Ajusta la sensibilidad
        // Acumular offset en la propiedad _MainTex_ST o un vector de offset
        // Nuestro shader UI/ScrollDown usa desplazamiento automático con _Time, mejor usar otro shader con parámetro Offset.
        // Pero para control manual, podemos modificar el offset directamente en el material.
        Vector2 offset = material.mainTextureOffset;
        offset.y -= deltaY;
        material.mainTextureOffset = offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (manager != null)
            manager.TerminarScroll();
    }
}
