using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InteragivelItem : MonoBehaviour
{
    [Header("Configurações de Item")]
    [Tooltip("Nome do item coletado")]
    public string nomeItem = "Item";
    
    [Tooltip("Ícone do item (opcional, usa o sprite se não definido)")]
    public Sprite iconeItem;

    [Header("Configurações Visuais")]
    public Color corContorno = Color.yellow;
    [Range(1f, 10f)] public float velocidadePiscada = 3f;
    [Range(1, 5)] public int espessuraContorno = 2;
    [Range(1f, 3f), Tooltip("Multiplicador de brilho do contorno")]
    public float brilhoContorno = 1.5f;

    private SpriteRenderer spriteRenderer;
    private GameObject contornoObject;
    private bool playerProximo = false;
    private float tempoPiscada = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogError($"{gameObject.name}: SpriteRenderer ou sprite não encontrado!");
            return;
        }
        
        // Se não definiu ícone, usa o próprio sprite
        if (iconeItem == null)
            iconeItem = spriteRenderer.sprite;
            
        CriarContorno();
    }

    void Start()
    {
        if (contornoObject != null)
            contornoObject.SetActive(false);
    }

    void CriarContorno()
    {
        contornoObject = new GameObject("Contorno");
        contornoObject.transform.SetParent(transform);
        contornoObject.transform.localPosition = Vector3.zero;
        contornoObject.transform.localRotation = Quaternion.identity;
        contornoObject.transform.localScale = Vector3.one;

        float offset = (1f / spriteRenderer.sprite.pixelsPerUnit) * espessuraContorno;

        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-offset, 0, 0),
            new Vector3(offset, 0, 0),
            new Vector3(0, -offset, 0),
            new Vector3(0, offset, 0)
        };

        foreach (Vector3 pos in offsets)
        {
            GameObject parte = new GameObject("Parte");
            parte.transform.SetParent(contornoObject.transform);
            parte.transform.localPosition = pos;
            parte.transform.localScale = Vector3.one;

            SpriteRenderer sr = parte.AddComponent<SpriteRenderer>();
            sr.sprite = spriteRenderer.sprite;
            
            Color corBrilhante = corContorno * brilhoContorno;
            corBrilhante.a = corContorno.a;
            sr.color = corBrilhante;
            
            sr.sortingLayerName = spriteRenderer.sortingLayerName;
            sr.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
    }

    void Update()
    {
        if (playerProximo && contornoObject != null && contornoObject.activeSelf)
        {
            tempoPiscada += Time.deltaTime * velocidadePiscada;
            float alpha = (Mathf.Sin(tempoPiscada * Mathf.PI) + 1f) / 2f;
            
            foreach (SpriteRenderer sr in contornoObject.GetComponentsInChildren<SpriteRenderer>())
            {
                Color cor = corContorno * brilhoContorno;
                cor.a = alpha;
                sr.color = cor;
            }
        }
    }

    public void AtivarContorno()
    {
        playerProximo = true;
        tempoPiscada = 0f;
        if (contornoObject != null)
            contornoObject.SetActive(true);
    }

    public void DesativarContorno()
    {
        playerProximo = false;
        if (contornoObject != null)
            contornoObject.SetActive(false);
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
        if (contornoObject != null)
            Destroy(contornoObject);
    }
}
