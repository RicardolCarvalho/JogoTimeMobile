using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InteragivelComposto : MonoBehaviour
{
    [Header("Configurações de Item")]
    [Tooltip("Nome do item coletado")]
    public string nomeItem = "Item";
    
    [Tooltip("Ícone do item (opcional)")]
    public Sprite iconeItem;

    [Header("Configurações Visuais")]
    public Color corContorno = Color.yellow;
    [Range(1f, 10f)] public float velocidadePiscada = 3f;
    [Range(1, 5)] public int espessuraContorno = 2;
    [Range(1f, 3f), Tooltip("Multiplicador de brilho do contorno")]
    public float brilhoContorno = 1.5f;

    private GameObject contornoRoot;
    private bool playerProximo;
    private float tempoPiscada;
    private bool _building; // evita reentrada

    void Awake()
    {
        // Verifica se tem sprites filhos
        var renderers = GetComponentsInChildren<SpriteRenderer>(false);
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: InteragivelComposto precisa ter sprites filhos!");
            return;
        }
        
        RebuildOutlineAndBounds(); // cria uma vez
        SetOutlineActive(false);   // começa desligado
    }

    void Start()
    {
        SetOutlineActive(false);
    }

    void SetOutlineActive(bool active)
    {
        if (contornoRoot != null)
            contornoRoot.SetActive(active);
    }

    [ContextMenu("Rebuild Outline (Editor)")]
    public void RebuildOutlineAndBounds()
    {
        if (_building) return;
        _building = true;

        // Apaga contorno anterior sem disparar loucuras
        if (contornoRoot != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(contornoRoot);
            else Destroy(contornoRoot);
#else
            Destroy(contornoRoot);
#endif
            contornoRoot = null;
        }

        var renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        if (renderers.Length == 0)
        {
            _building = false;
            return;
        }

        // Bounds combinados (WORLD) - apenas para referência visual
        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        // Cria raiz do contorno (nasce DESLIGADO)
        contornoRoot = new GameObject("ContornoRoot");
        contornoRoot.transform.SetParent(transform, false);
        contornoRoot.SetActive(false);

        // Duplicar cada peça com 4 offsets
        foreach (var baseSr in renderers)
        {
            if (baseSr.sprite == null) continue;

            float ppu = baseSr.sprite.pixelsPerUnit > 0 ? baseSr.sprite.pixelsPerUnit : 100f;
            float offset = (1f / ppu) * espessuraContorno;

            Vector3[] offsets = new Vector3[]
            {
                new Vector3(-offset, 0, 0),
                new Vector3(offset, 0, 0),
                new Vector3(0, -offset, 0),
                new Vector3(0, offset, 0)
            };

            foreach (var off in offsets)
            {
                var go = new GameObject($"Outline_{baseSr.gameObject.name}");
                // manter WORLD transform da peça
                go.transform.position = baseSr.transform.position + off;
                go.transform.rotation = baseSr.transform.rotation;
                go.transform.localScale = baseSr.transform.lossyScale;
                // agora parent mantendo world
                go.transform.SetParent(contornoRoot.transform, true);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = baseSr.sprite;

                Color cor = corContorno * brilhoContorno;
                cor.a = corContorno.a;
                sr.color = cor;

                sr.sortingLayerID = baseSr.sortingLayerID;
                sr.sortingOrder = baseSr.sortingOrder - 1;
                sr.flipX = baseSr.flipX;
                sr.flipY = baseSr.flipY;
            }
        }

        _building = false;
    }

    void Update()
    {
        // pisca
        if (playerProximo && contornoRoot != null && contornoRoot.activeSelf)
        {
            tempoPiscada += Time.unscaledDeltaTime * velocidadePiscada;
            float alpha = (Mathf.Sin(tempoPiscada * Mathf.PI) + 1f) / 2f;

            foreach (var sr in contornoRoot.GetComponentsInChildren<SpriteRenderer>())
            {
                Color c = corContorno * brilhoContorno;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    public void AtivarContorno()
    {
        playerProximo = true;
        tempoPiscada = 0f;
        SetOutlineActive(true);
    }

    public void DesativarContorno()
    {
        playerProximo = false;
        SetOutlineActive(false);
    }

    public void Interagir()
    {
        if (string.IsNullOrEmpty(nomeItem))
        {
            Debug.LogWarning($"Nome do item não configurado em {gameObject.name}!");
            return;
        }

        // Adiciona ao inventário
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AdicionarItem(nomeItem);
        }
        else
        {
            Debug.LogWarning("InventoryManager não encontrado na cena!");
        }

        // Mostra notificação
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.MostrarNotificacao($"Coletado: {nomeItem}", iconeItem);
        }
        else
        {
            Debug.LogWarning("NotificationManager não encontrado na cena!");
        }

        // Desativa o objeto após coletar
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (contornoRoot != null) Destroy(contornoRoot);
    }

}
