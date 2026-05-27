using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleMinigame : MonoBehaviour
{
    public void OnWin()
    {
        //Cambiar de escena al SampleScene
        SceneManager.LoadScene("SampleScene");
        Debug.Log("WinCondition");
    }

    public void OnLose()
    {
        //Cambiar de escena al SampleScene
        SceneManager.LoadScene("SampleScene");
        Debug.Log("LoseCondition");
    }
}
