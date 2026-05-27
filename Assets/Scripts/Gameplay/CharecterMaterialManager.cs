using UnityEngine;
using UnityEngine.UI; // Necesario para la UI (Image)

public class CharacterMaterialManager : MonoBehaviour
{
    [Header("Personaje Principal (Quad)")]
    [Tooltip("El MeshRenderer del Quad de tu personaje estático.")]
    public MeshRenderer characterQuadRenderer;

    [Header("Opciones de Materiales")]
    [Tooltip("Coloca aquí todos los materiales posibles para el personaje.")]
    public Material[] availableMaterials;

    [Header("Paneles de Minijuegos")]
    [Tooltip("Coloca aquí los componentes Image (UI normal) de los personajes.")]
    public Image[] minigameCharacterImages; // Ahora usamos Image en lugar de RawImage

    private Material selectedMaterial;

    void Start()
    {
        AssignRandomMaterial();
    }

    private void AssignRandomMaterial()
    {
        // 1. Verificamos que haya materiales
        if (availableMaterials == null || availableMaterials.Length == 0)
        {
            Debug.LogWarning("¡No has asignado materiales en el manager!");
            return;
        }

        // 2. Elegimos al azar
        int randomIndex = Random.Range(0, availableMaterials.Length);
        selectedMaterial = availableMaterials[randomIndex];

        // 3. Aplicamos al Quad
        if (characterQuadRenderer != null)
        {
            characterQuadRenderer.material = selectedMaterial;
        }

        // 4. Sincronizamos la UI
        UpdateMinigamePanels();
    }

    private void UpdateMinigamePanels()
    {
        // Obtenemos la textura principal del material y la leemos como Texture2D
        Texture2D characterTexture = selectedMaterial.mainTexture as Texture2D;

        if (characterTexture == null)
        {
            Debug.LogWarning("El material seleccionado no tiene una textura principal asignada.");
            return;
        }

        // Creamos un Sprite nuevo a partir de la textura obtenida
        Rect spriteRect = new Rect(0, 0, characterTexture.width, characterTexture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f); // Centro de la imagen
        Sprite createdSprite = Sprite.Create(characterTexture, spriteRect, pivot);

        // Recorremos las Images de la UI y les asignamos el nuevo Sprite
        foreach (Image uiImage in minigameCharacterImages)
        {
            if (uiImage != null)
            {
                uiImage.sprite = createdSprite;
            }
        }
    }
}