using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class InteragivelSimples : MonoBehaviour
{
    [Header("Configurações de Interação")]
    public string cenaDestino;

    [Header("Itens Necessários")]
    [Tooltip("Lista de itens que o player precisa ter coletado para interagir")]
    public List<string> itensNecessarios = new List<string>();
    
    [Tooltip("Mensagem quando faltam itens")]
    public string mensagemBloqueado = "Você precisa coletar todos os itens antes!";

    [Header("Configurações Visuais")]
    public Color corContorno = Color.yellow;
    
    [Range(1f, 10f)]
    public float velocidadePiscada = 3f;

    [Range(1, 5)]
    public int espessuraContorno = 2;
    
    [Range(1f, 3f)]
    [Tooltip("Multiplicador de brilho do contorno")]
    public float brilhoContorno = 1.5f;

    private SpriteRenderer spriteRenderer;
    private GameObject contornoObject;
    private bool playerProximo = false;
    private float tempoPiscada = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            
            // Aplicar cor com brilho aumentado
            Color corBrilhante = corContorno * brilhoContorno;
            corBrilhante.a = corContorno.a; // Manter alpha original
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

        // Tem todos os itens (ou não precisa de nenhum), pode trocar de cena
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
        if (contornoObject != null)
            Destroy(contornoObject);
    }
}

    