using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class InteragivelCompostoPorta : MonoBehaviour
{
    [Header("Configurações de Cena")]
    public string cenaDestino;

    [Header("Itens Necessários")]
    [Tooltip("Lista de itens que o player precisa ter coletado para interagir")]
    public List<string> itensNecessarios = new List<string>();
    
    [Tooltip("Mensagem quando faltam itens")]
    public string mensagemBloqueado = "Você precisa coletar todos os itens antes!";

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
            Debug.LogWarning($"{gameObject.name}: InteragivelCompostoPorta precisa ter sprites filhos!");
            return;
        }
        
        RebuildOutlineAndBounds();
        SetOutlineActive(false);
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

        // Apaga contorno anterior
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

        // Bounds combinados (WORLD)
        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        // Cria raiz do contorno
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
                go.transform.position = baseSr.transform.position + off;
                go.transform.rotation = baseSr.transform.rotation;
                go.transform.localScale = baseSr.transform.lossyScale;
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
        if (cenaDestino == null)
        {
            Debug.LogWarning($"Cena não configurada em {gameObject.name}!");
            return;
        }

        // Verifica se precisa de itens
        if (itensNecessarios != null && itensNecessarios.Count > 0)
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("InventoryManager não encontrado! Não é possível verificar itens.");
                return;
            }

            // Verifica se tem todos os itens
            if (!InventoryManager.Instance.TemTodosItens(itensNecessarios))
            {
                // Mostra quais itens estão faltando
                var faltando = InventoryManager.Instance.GetItensFaltando(itensNecessarios);
                
                string mensagem = mensagemBloqueado;
                if (faltando.Count > 0)
                {
                    mensagem += "\nFaltam: " + string.Join(", ", faltando);
                }

                // Mostra notificação
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.MostrarNotificacao(mensagem);
                }
                else
                {
                    Debug.Log(mensagem);
                }
                
                return; // Não permite interagir
            }
        }

        try
        {
            SceneManager.LoadScene(cenaDestino);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar cena: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (contornoRoot != null) Destroy(contornoRoot);
    }
}
