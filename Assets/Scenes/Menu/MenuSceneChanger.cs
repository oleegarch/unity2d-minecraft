using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneChanger : MonoBehaviour
{
    public void StartSpectatorMode()
    {
        SceneManager.LoadScene("SpectatorMode");
    }
    public void StartPlayerMode()
    {
        SceneManager.LoadScene("PlayerMode");
    }
}
