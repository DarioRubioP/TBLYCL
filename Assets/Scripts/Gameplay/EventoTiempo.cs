using UnityEngine;
using System.Collections;

public class EventoTiempo : MonoBehaviour
{
    [Header("Objetos con Shader")]
    public GameObject[] objetosConShader;

    [Header("UI y Paneles de Control")]
    public GameObject canvasPrincipal;
    public GameObject panelInicial;
    public GameObject[] opcionesPaneles;

    [Header("Validación")]
    public GameObject panelBloqueo1; // Panel específico 1
    public GameObject panelBloqueo2; // Panel específico 2

    private Coroutine secuenciaCorrutina;
    private bool secuenciaTerminada = false; // Nueva bandera

    void Update()
    {
        // Si ya terminó, no hacemos nada más
        if (secuenciaTerminada) return;

        bool estanCerrados = !panelBloqueo1.activeSelf && !panelBloqueo2.activeSelf;

        if (estanCerrados && secuenciaCorrutina == null)
        {
            secuenciaCorrutina = StartCoroutine(SecuenciaEventos());
        }
        else if (!estanCerrados && secuenciaCorrutina != null)
        {
            StopCoroutine(secuenciaCorrutina);
            secuenciaCorrutina = null;
        }
    }

    IEnumerator SecuenciaEventos()
    {
        Debug.Log("Inicio secuencia");

        yield return new WaitForSeconds(5f);

        Debug.Log("Pasaron 5 segundos");

        foreach (GameObject obj in objetosConShader)
        {
            obj.GetComponent<Renderer>().material.SetFloat("_Speed", 0f);
        }

        canvasPrincipal.SetActive(true);
        panelInicial.SetActive(true);

        Debug.Log("Panel inicial activado");

        yield return new WaitForSeconds(3f);

        Debug.Log("Intentando activar panel random");

        panelInicial.SetActive(false);

        int randomIndex = Random.Range(0, opcionesPaneles.Length);
        opcionesPaneles[randomIndex].SetActive(true);

        Debug.Log("Panel random activado");

        secuenciaTerminada = true;
    }
}