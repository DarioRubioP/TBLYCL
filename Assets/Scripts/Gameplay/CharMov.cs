using UnityEngine;

public class CharMov : MonoBehaviour
{
    //Script de hacer una ligera rotacion de tipo balanceo de izquierda a derecha del objeto quad en escena, para darle un poco de vida, como si estuviera flotando o moviéndose suavemente.
    //El objeto debe tener posicion base 0, 0, 0 para que el movimiento sea correcto.

    public float amplitude = 0.5f; // Amplitud del movimiento de balanceo
    public float frequency = 1f; // Frecuencia del movimiento de balanceo
    public GameObject targetObject; // El objeto que se moverá

    void Start()
    {
        if (targetObject == null)
        {
            targetObject = gameObject; // Si no se asigna un objeto, usar este
        }
    }

    
    void Update()
    {
        if (targetObject != null)
        {
            // Calcular el ángulo de rotación basado en el tiempo
            float angle = Mathf.Sin(Time.time * frequency) * amplitude;
            // Aplicar la rotación al objeto
            targetObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }


}
