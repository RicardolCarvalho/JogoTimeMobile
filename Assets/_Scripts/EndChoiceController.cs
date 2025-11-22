using UnityEngine;
using UnityEngine.SceneManagement;

public class EndChoiceController : MonoBehaviour
{
    [System.Serializable]
    public class Choice
    {
        public string label;          // só organizador visual no Inspector
        public Transform icon;        // objeto visual na cena (em qualquer lugar)
        public int sceneIndex;        // índice da cena de EndGame
    }

    [Header("Configurações")]
    public Choice[] choices;

    [Header("Feedback visual")]
    public float selectedScale = 1.2f;
    public float normalScale = 1f;

    private int currentIndex = 0;

    void Start()
    {
        if (choices == null || choices.Length == 0)
        {
            Debug.LogError("Nenhuma escolha configurada no EndChoiceController.");
            enabled = false;
            return;
        }

        UpdateVisuals();
    }

    void Update()
    {
        // Navegar entre escolhas com A / D / setas
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(+1);
        }

        // Confirmar com E, Enter ou botão do controle (X/A = JoystickButton0)
        if (Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            SelectCurrent();
        }
    }

    void Move(int direction)
    {
        if (choices.Length == 0) return;

        currentIndex = (currentIndex + direction + choices.Length) % choices.Length;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i].icon == null) continue;

            float scale = (i == currentIndex) ? selectedScale : normalScale;
            choices[i].icon.localScale = Vector3.one * scale;
        }
    }

    void SelectCurrent()
    {
        if (choices.Length == 0) return;

        int sceneIndex = choices[currentIndex].sceneIndex;
        if (sceneIndex < 0)
        {
            Debug.LogWarning("SceneIndex inválido na escolha: " + choices[currentIndex].label);
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    // =========================================================
    // MÉTODOS PÚBLICOS PARA OS BOTÕES DE TELA (MOBILE)
    // =========================================================

    public void OnLeftChoiceButton()
    {
        Move(-1);
    }

    public void OnRightChoiceButton()
    {
        Move(+1);
    }

    public void OnConfirmChoiceButton()
    {
        SelectCurrent();
    }
}
