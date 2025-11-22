using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.DualShock;
using TMPro;

public class RestartButtonHint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MenuActions menu;                 // arraste o MenuActions da cena
    [SerializeField] private TextMeshProUGUI label;            // arraste o TextMeshProUGUI do texto

    [Header("Display Options")]
    [Tooltip("Use sprites do TMP para mostrar ícones de botões (ex.: <sprite name=\"XboxA\">).")]
    [SerializeField] private bool useSpriteIcons = false;

    // Caso use sprites, você pode alternar o Sprite Asset conforme o controle.
    [SerializeField] private TMP_SpriteAsset xboxSpriteAsset;   // sprite asset com ícone do A (Xbox)
    [SerializeField] private TMP_SpriteAsset psSpriteAsset;     // sprite asset com ícone do X (PlayStation)
    [SerializeField] private string xboxSpriteName = "XboxA";   // nome do sprite no asset
    [SerializeField] private string psSpriteName = "PSX";       // nome do sprite no asset

    [Header("Texts (fallback sem ícone)")]
    [SerializeField] private string keyboardText = "Pressione E para reiniciar";
    [SerializeField] private string xboxText     = "Pressione A para reiniciar";
    [SerializeField] private string psText       = "Pressione X para reiniciar";
    [SerializeField] private string genericPadText = "Pressione E para reiniciar";

    [Header("Blink Animation")]
    [SerializeField] private float blinkSpeed = 1.5f;   // quanto maior, mais rápido pisca
    [SerializeField] private float minAlpha  = 0.35f;
    [SerializeField] private float maxAlpha  = 1.0f;

    private enum InputMode { KeyboardMouse, Xbox, PlayStation, GenericGamepad }
    private InputMode currentMode = InputMode.KeyboardMouse;
    private float blinkT;

    private void Reset()
    {
        label = GetComponent<TextMeshProUGUI>();
        if (!menu) menu = FindAnyObjectByType<MenuActions>();
    }

    private void Awake()
    {
        if (!label) label = GetComponent<TextMeshProUGUI>();
        UpdateLabelForDevice(GuessInitialMode());
    }

    private void Update()
    {
        // 1) Detecta último dispositivo usado e atualiza o texto se mudou
        var newMode = DetectLastUsedDevice() ?? currentMode;
        if (newMode != currentMode)
        {
            UpdateLabelForDevice(newMode);
        }

        // 2) Ouve o "botão para iniciar" e chama o MenuActions.IniciarJogo()
        if (currentMode == InputMode.KeyboardMouse)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                TryRestartGame();
        }
        else
        {
            var gp = Gamepad.current;
            if (gp != null && gp.buttonSouth.wasPressedThisFrame) // A (Xbox) / X (PS)
                TryRestartGame();
        }

        // 3) Animação de piscar (lerp de alpha no TextMeshPro)
        AnimateBlink();
    }

    private void TryRestartGame()
    {
        if (menu != null)
            menu.Menu();
        else
            Debug.LogWarning("[StartButtonHint] MenuActions não atribuído.");
    }

    public void OnMobileRestartButton()
    {
        TryRestartGame();
    }

    private InputMode GuessInitialMode()
    {
        if (Gamepad.current != null) return ClassifyGamepad(Gamepad.current);
        return InputMode.KeyboardMouse;
    }

    /// <summary>
    /// Tenta identificar qual dispositivo foi usado nesta frame.
    /// Se teclado/mouse pressionou algo, KeyboardMouse.
    /// Se gamepad pressionou algo, classifica o modelo.
    /// Se nada foi pressionado, retorna null (mantém estado atual).
    /// </summary>
    private InputMode? DetectLastUsedDevice()
    {
        // Teclado/Mouse
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return InputMode.KeyboardMouse;

        if (Mouse.current != null && (
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame ||
                Mouse.current.forwardButton.wasPressedThisFrame ||
                Mouse.current.backButton.wasPressedThisFrame))
            return InputMode.KeyboardMouse;

        // Gamepad
        var gp = Gamepad.current;
        if (gp != null && AnyGamepadButtonPressedThisFrame(gp))
            return ClassifyGamepad(gp);

        return null; // nenhum input novo — mantém o modo atual
    }

    private bool AnyGamepadButtonPressedThisFrame(Gamepad gp)
    {
        return gp.buttonSouth.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame ||
               gp.buttonWest.wasPressedThisFrame  || gp.buttonNorth.wasPressedThisFrame ||
               gp.startButton.wasPressedThisFrame || gp.selectButton.wasPressedThisFrame ||
               gp.leftShoulder.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame ||
               gp.leftStickButton.wasPressedThisFrame || gp.rightStickButton.wasPressedThisFrame ||
               gp.dpad.up.wasPressedThisFrame || gp.dpad.down.wasPressedThisFrame ||
               gp.dpad.left.wasPressedThisFrame || gp.dpad.right.wasPressedThisFrame;
    }

    private InputMode ClassifyGamepad(Gamepad gp)
    {
        // Detecta por tipo concreto (mais confiável)
        if (gp is XInputController) return InputMode.Xbox;
        if (gp is DualShockGamepad) return InputMode.PlayStation;

        // Heurística por nome/descrição (fallback)
        var name = (gp.displayName ?? gp.name ?? string.Empty).ToLower();
        var prod = (gp.description.product ?? string.Empty).ToLower();
        var manu = (gp.description.manufacturer ?? string.Empty).ToLower();

        if (name.Contains("xbox") || prod.Contains("xbox") || manu.Contains("microsoft"))
            return InputMode.Xbox;

        if (name.Contains("dual") || name.Contains("playstation") || prod.Contains("sony") || manu.Contains("sony"))
            return InputMode.PlayStation;

        return InputMode.GenericGamepad;
    }

    private void UpdateLabelForDevice(InputMode mode)
    {
        currentMode = mode;

        if (!label) return;

        if (useSpriteIcons)
        {
            switch (mode)
            {
                case InputMode.Xbox:
                    if (xboxSpriteAsset) label.spriteAsset = xboxSpriteAsset;
                    label.text = $"Pressione <sprite name=\"{xboxSpriteName}\"> para iniciar";
                    break;

                case InputMode.PlayStation:
                    if (psSpriteAsset) label.spriteAsset = psSpriteAsset;
                    label.text = $"Pressione <sprite name=\"{psSpriteName}\"> para iniciar";
                    break;

                case InputMode.GenericGamepad:
                    // usa o asset atual, mas mantém a ideia de "A"
                    label.text = $"Pressione <sprite name=\"{xboxSpriteName}\"> para iniciar";
                    break;

                default: // KeyboardMouse
                    label.text = keyboardText;
                    break;
            }
        }
        else
        {
            switch (mode)
            {
                case InputMode.Xbox:          label.text = xboxText;        break;
                case InputMode.PlayStation:   label.text = psText;          break;
                case InputMode.GenericGamepad:label.text = genericPadText;  break;
                default:                      label.text = keyboardText;    break;
            }
        }
    }

    private void AnimateBlink()
    {
        if (!label) return;

        // ping-pong de 0..1 com tempo não escalado (funciona mesmo com timescale 0 no menu)
        blinkT += Time.unscaledDeltaTime * blinkSpeed;
        float p = Mathf.PingPong(blinkT, 1f); // 0..1..0
        float a = Mathf.Lerp(minAlpha, maxAlpha, p);

        var c = label.color;
        c.a = a;
        label.color = c;
    }
}
