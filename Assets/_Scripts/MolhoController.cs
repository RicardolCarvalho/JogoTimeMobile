using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MolhoController : MonoBehaviour
{
    public static MolhoController instance;

    [Header("Pontuação e Erros")]
    public int points = 0;
    public int errors = 0;
    public int maxPoints = 5;
    public int maxErrors = 3;

    [Header("Referências")]
    public TomatoSpawner spawner;
    public GameObject screenFlash; // arraste o painel vermelho do Canvas
    public ParticleSystem fireParticles; // arraste o sistema de partículas do fogo
    
    [Header("Cenas")]
    [Tooltip("Arraste a cena de vitória (arraste o SceneAsset aqui)")]
    public string cenaVitoria;

    [Tooltip("Arraste a cena do mapa (para voltar ao perder)")]
    public string cenaMapa;

    [Header("Configuração do Fogo")]
    [Tooltip("Quanto aumenta a emissão por erro")]
    public float fireEmissionPerError = 50f;
    
    [Tooltip("Quanto aumenta o tamanho por erro")]
    public float fireSizePerError = 0.5f;
    
    [Tooltip("Quanto aumenta a velocidade por erro")]
    public float fireSpeedPerError = 2f;
    
    [Tooltip("Taxa de emissão quando explodir (tela cheia)")]
    public float fireEmissionExplosion = 500f;

    [Header("Áudio")]
    [Tooltip("Clip tocado quando um tomate é perdido/erro")]
    public AudioClip tomatoMissClip;

    [Tooltip("AudioSource opcional. Se vazio, o script tentará GetComponent<AudioSource>()")]
    public AudioSource audioSource;

    [Range(0f,1f)]
    [Tooltip("Volume do som de erro")]
    public float missVolume = 1f;
    
    [Tooltip("Clip tocado quando o jogador vence o minigame")]
    public AudioClip victoryClip;

    [Range(0f,1f)]
    [Tooltip("Volume do som de vitória")]
    public float victoryVolume = 1f;

    private float initialEmission;
    private float initialSize;
    private float initialSpeed;

    private bool gameEnded = false;
    private float flashTimer;
    private bool flashing;

    void Awake()
    {
        instance = this;
        
        // Garante que o flash fique invisível no início
        if (screenFlash != null)
        {
            var canvas = screenFlash.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = screenFlash.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999; // muito alto para ficar na frente de tudo
            
            var img = screenFlash.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                var c = img.color;
                c.a = 0f;
                img.color = c;
            }
        }
        
        // Configura valores iniciais do fogo (captura do próprio ParticleSystem)
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            initialEmission = emission.rateOverTime.constant;
            
            var main = fireParticles.main;
            initialSize = main.startSize.constant;
            initialSpeed = main.startSpeed.constant;
        }

        // Inicializa AudioSource se necessário
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (flashing)
        {
            flashTimer += Time.deltaTime;
            float alpha = Mathf.PingPong(flashTimer * 2f, 1f);
            var img = screenFlash.GetComponent<UnityEngine.UI.Image>();
            var c = img.color;
            c.a = alpha * 0.4f;
            img.color = c;
        }
    }

    public void AddPoint()
    {
        if (gameEnded) return;
        points++;

        if (points >= maxPoints)
        {
            Victory();
        }
    }

    public void AddError()
    {
        if (gameEnded) return;
        errors++;

        // Aumenta o fogo gradualmente
        if (fireParticles != null)
        {
            // Calcula novos valores baseados no número de erros
            float newEmission = initialEmission + (errors * fireEmissionPerError);
            float newSize = initialSize + (errors * fireSizePerError);
            float newSpeed = initialSpeed + (errors * fireSpeedPerError);
            
            // Modifica emissão
            var emission = fireParticles.emission;
            emission.rateOverTime = newEmission;
            
            // Modifica tamanho e velocidade
            var main = fireParticles.main;
            main.startSize = newSize;
            main.startSpeed = newSpeed;
            
            Debug.Log($"🔥 Fogo aumentado! Erro {errors} | Emissão: {newEmission} | Tamanho: {newSize} | Velocidade: {newSpeed}");
        }

        // Toca som de erro quando um tomate é perdido
        if (tomatoMissClip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(tomatoMissClip, missVolume);
            }
            else
            {
                var pos = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(tomatoMissClip, pos, missVolume);
            }
        }

        if (errors >= maxErrors)
        {
            Explosion();
        }
    }

    

    private void Explosion()
    {
        gameEnded = true;
        spawner.canSpawn = false;
        flashing = true;

        // Fogo explode e toma conta da tela
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = fireEmissionExplosion;
            
            // Aumenta o tamanho das partículas
            var main = fireParticles.main;
            main.startSize = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        }

        Debug.Log("💥 Explosão de Molho!");
        StartCoroutine(HandleLose());
    }

    private void Victory()
    {
        gameEnded = true;
        spawner.canSpawn = false;
        Debug.Log("✅ Vitória! Você salvou o jantar!");

        // Toca som de vitória
        if (victoryClip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(victoryClip, victoryVolume);
            }
            else
            {
                var pos = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(victoryClip, pos, victoryVolume);
            }
        }

        StartCoroutine(HandleWin());
    }

    private IEnumerator HandleWin()
    {
        // pequeno delay para permitir que efeitos sejam vistos
        yield return new WaitForSeconds(0.8f);
        if (cenaVitoria != null)
        {
            SceneManager.LoadScene(cenaVitoria);
        }
        else
        {
            Debug.LogWarning("Cena de vitória não atribuída em MolhoController.");
        }
    }

    private IEnumerator HandleLose()
    {
        // permite ver a explosão / flash antes de voltar ao mapa
        yield return new WaitForSeconds(1.2f);
        if (cenaMapa != null)
        {
            SceneManager.LoadScene(cenaMapa);
        }
        else
        {
            Debug.LogWarning("Cena do mapa não atribuída em MolhoController.");
        }
    }
}
