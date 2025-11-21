using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Configurações UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Image notificationIcon;
    
    [Header("Configurações de Animação")]
    [SerializeField] private float duracaoNotificacao = 3f;
    [SerializeField] private float tempoFadeIn = 0.3f;
    [SerializeField] private float tempoFadeOut = 0.5f;

    private CanvasGroup canvasGroup;
    private Coroutine notificationCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (notificationPanel != null)
        {
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0f;
            notificationPanel.SetActive(false);
        }
    }

    public void MostrarNotificacao(string mensagem, Sprite icone = null)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("NotificationManager: Painel ou texto não configurado!");
            return;
        }

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(ExibirNotificacao(mensagem, icone));
    }

    private IEnumerator ExibirNotificacao(string mensagem, Sprite icone)
    {
        notificationText.text = mensagem;
        
        if (notificationIcon != null)
        {
            if (icone != null)
            {
                notificationIcon.sprite = icone;
                notificationIcon.gameObject.SetActive(true);
            }
            else
            {
                notificationIcon.gameObject.SetActive(false);
            }
        }

        notificationPanel.SetActive(true);

        // Fade In
        float elapsed = 0f;
        while (elapsed < tempoFadeIn)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / tempoFadeIn);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Aguarda
        yield return new WaitForSeconds(duracaoNotificacao);

        // Fade Out
        elapsed = 0f;
        while (elapsed < tempoFadeOut)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / tempoFadeOut);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }
}
