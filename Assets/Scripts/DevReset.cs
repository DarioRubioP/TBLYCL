using UnityEngine;
using UnityEngine.SceneManagement;

public class DevReset : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetScene()
    {
        //Cargar MainScene
        SceneManager.LoadScene("SampleScene");
    }
}

//HolaGit