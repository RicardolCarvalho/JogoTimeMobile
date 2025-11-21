using UnityEngine;
using UnityEditor;

public class Ball : MonoBehaviour
{
    [Header("Configuração")]
    public float speed = 3f;          // velocidade inicial
    public float range = 4f;          // ida e volta a partir da posição inicial
    public Color[] colors = { Color.cyan, Color.magenta, Color.green };

    [Header("Trocar de Cena ao Destruir")]
    [Tooltip("Cena para onde vai ao destruir a bola")]
    public string cenaDestino;

    private SpriteRenderer sr;
    private Vector3 startPos;
    private int direction = 1;

    // controle de cortes
    private int cuts;                 // quantas vezes foi cortada
    private bool leaving;             // modo sair da tela
    private float leaveTimer;
    private float leaveDuration = 1.2f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = colors.Length > 0 ? colors[0] : Color.white;
        startPos = transform.position;
    }

    void Update()
    {
        if (leaving)
        {
            transform.Translate(Vector2.right * speed * 3f * Time.deltaTime);
            leaveTimer += Time.deltaTime;
            if (leaveTimer >= leaveDuration)
            {
                if (cenaDestino != null)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(cenaDestino);
                }
                Destroy(gameObject);
            }
            return;
        }

        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - startPos.x) >= range)
        {
            direction *= -1;
        }
    }

    // chamado pela faca durante a janela de corte
    public void OnCutHit()
    {
        cuts++;

        if (cuts >= 3)
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            leaving = true;
            leaveTimer = 0f;
            return;
        }

        speed += 10f; // aumentada a velocidade depois de cada corte

        if (sr != null && colors.Length > 0)
        {
            sr.color = colors[cuts % colors.Length];
        }
    }
}
