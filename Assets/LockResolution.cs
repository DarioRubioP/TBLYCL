using UnityEngine;

public class LockResolution : MonoBehaviour
{
    void Awake()
    {
        // Fuerza la resolución a 1340x800 en modo ventana
        Screen.SetResolution(1340, 800, false);

        // Deshabilita la opción de cambiar el tamaño de la ventana por el usuario
#if UNITY_STANDALONE
        Screen.fullScreenMode = FullScreenMode.Windowed;
#endif
    }

    void Update()
    {
        // Evita que el tamaño cambie si el usuario accidentalmente arrastra la ventana
        if (Screen.width != 1340 || Screen.height != 800)
        {
            Screen.SetResolution(1340, 800, false);
        }
    }
}
