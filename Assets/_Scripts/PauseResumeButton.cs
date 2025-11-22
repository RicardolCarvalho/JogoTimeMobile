using UnityEngine;

public class PauseResumeButton : MonoBehaviour
{
    public void OnClickResume()
    {
        // Procura o PauseMenu na cena principal
        var pm = FindAnyObjectByType<PauseMenu>();
        if (pm != null)
        {
            pm.TogglePause();
        }
        else
        {
            Debug.LogWarning("PauseMenu não encontrado para retomar o jogo.");
        }
    }
}
