using UnityEngine;

public class KnifeCut : MonoBehaviour
{
    [Header("Corte")]
    public KeyCode cutKey = KeyCode.Space;
    public float cutWindow = 0.15f;

    [Header("Feedback visual")]
    public Color cuttingColor = Color.yellow;
    public float scaleBoost = 1.15f;

    private bool isCutting;
    private bool consumedHitThisWindow;   // evita múltiplos acertos na mesma janela
    private float cutTimer;
    private SpriteRenderer sr;
    private Color originalColor;
    private Vector3 originalScale;

    private AudioSource audioSource;
    public AudioClip cutSound;

    public bool IsCutting => isCutting;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (Input.GetKeyDown(cutKey))
        {
            StartCut();
        }

        if (isCutting)
        {
            cutTimer -= Time.deltaTime;
            if (cutTimer <= 0f)
            {
                EndCut();
            }
        }
    }

    private void StartCut()
    {
        isCutting = true;
        if (cutSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cutSound);
        }

        consumedHitThisWindow = false;     // libera novo acerto
        cutTimer = cutWindow;

        if (sr != null) sr.color = cuttingColor;
        transform.localScale = originalScale * scaleBoost;
    }

    private void EndCut()
    {
        isCutting = false;

        if (sr != null) sr.color = originalColor;
        transform.localScale = originalScale;
    }

    private void TryHit(Collider2D other)
    {
        if (!isCutting) return;
        if (consumedHitThisWindow) return;

        var ball = other.GetComponent<Ball>();
        if (ball == null) return;

        consumedHitThisWindow = true;
        ball.OnCutHit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // cobre o caso de já estar encostado quando você aperta Espaço
        TryHit(other);
    }
}
