using UnityEngine;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{


    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
