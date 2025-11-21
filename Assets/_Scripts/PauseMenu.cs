using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private bool isPauseSceneLoaded = false;
    private int pauseSceneIndex = 9;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPauseSceneLoaded)
                LoadPauseScene();
            else
                UnloadPauseScene();
        }
    }

    void LoadPauseScene()
    {
        SceneManager.LoadScene(pauseSceneIndex, LoadSceneMode.Additive);
        Time.timeScale = 0f; // pausa o jogo
        isPauseSceneLoaded = true;
    }

    void UnloadPauseScene()
    {
        SceneManager.UnloadSceneAsync(pauseSceneIndex);
        Time.timeScale = 1f; // volta o jogo ao normal
        isPauseSceneLoaded = false;
    }
}
