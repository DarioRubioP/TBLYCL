using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class UIAnimationHelper
{
    /// <summary>
    /// Hace que una imagen "lata" (escala 1 → 1.2 → 1) y luego se desvanezca en el tiempo indicado.
    /// </summary>
    /// <param name="imagen">La imagen a animar.</param>
    /// <param name="duracionTotal">Tiempo total en segundos (debe ser al menos 0.5f).</param>
    public static IEnumerator LatidoYDesvanecer(Image imagen, float duracionTotal = 5f)
    {
        if (imagen == null) yield break;

        // Asegurar que empieza visible y con escala normal
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 1f);
        imagen.transform.localScale = Vector3.one;

        // 1. Latido (primeros 0.5 segundos)
        float tiempoLatido = Mathf.Min(0.5f, duracionTotal * 0.1f);
        float t = 0f;
        while (t < tiempoLatido)
        {
            t += Time.deltaTime;
            float progress = t / tiempoLatido;
            // Escala: 1 → 1.2 → 1 (usando una curva senoidal)
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
            imagen.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        imagen.transform.localScale = Vector3.one;

        // 2. Desvanecer durante el resto del tiempo
        float tiempoDesvanecer = duracionTotal - tiempoLatido;
        if (tiempoDesvanecer > 0f)
        {
            float alphaInicial = imagen.color.a;
            float elapsed = 0f;
            while (elapsed < tiempoDesvanecer)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(alphaInicial, 0f, elapsed / tiempoDesvanecer);
                imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, alpha);
                yield return null;
            }
        }

        // Asegurar que termina completamente transparente
        imagen.color = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 0f);
    }
}