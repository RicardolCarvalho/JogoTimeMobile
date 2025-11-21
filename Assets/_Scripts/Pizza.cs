using System.Collections.Generic;
using UnityEngine;

public class Pizza : MonoBehaviour
{
    [Header("Movimento da esteira")]
    public float speed = 1.6f;
    public float exitX = 6.5f;
    public float spawnX = -6.5f;
    public float laneY = -2.5f;

    [Header("Comportamento")]
    public bool loopOnExit = true;
    public bool clearOnRespawn = true;

    [Header("Sprites da Pizza")]
    [Tooltip("Sprite inicial (pizza vazia)")]
    public Sprite spriteInicial;
    
    [Tooltip("Sprite com 1 ingrediente")]
    public Sprite sprite1Ingrediente;
    
    [Tooltip("Sprite com 2 ingredientes")]
    public Sprite sprite2Ingredientes;
    
    [Tooltip("Sprite com 3+ ingredientes (pizza completa)")]
    public Sprite sprite3Ingredientes;

    private CircleCollider2D circle;
    private SpriteRenderer spriteRenderer;
    private readonly Dictionary<string, int> counts = new();
    private int totalIngredientes = 0;

    void Start()
    {
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Define sprite inicial
        if (spriteRenderer != null && spriteInicial != null)
        {
            spriteRenderer.sprite = spriteInicial;
        }
        
        var p = transform.position;
        p.y = laneY;
        transform.position = p;
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        if (transform.position.x >= exitX)
        {
            if (loopOnExit)
            {
                if (clearOnRespawn) ResetPizza();
                transform.position = new Vector3(spawnX, laneY, transform.position.z);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public bool ContainsPoint(Vector2 worldPoint)
    {
        if (circle == null) circle = GetComponent<CircleCollider2D>();
        return circle != null && circle.OverlapPoint(worldPoint);
    }

    public void AddIngredient(string name)
    {
        if (!counts.ContainsKey(name)) counts[name] = 0;
        counts[name] += 1;
        totalIngredientes++;
        
        AtualizarSprite();
    }

    private void AtualizarSprite()
    {
        if (spriteRenderer == null) return;

        if (totalIngredientes == 1 && sprite1Ingrediente != null)
        {
            spriteRenderer.sprite = sprite1Ingrediente;
        }
        else if (totalIngredientes == 2 && sprite2Ingredientes != null)
        {
            spriteRenderer.sprite = sprite2Ingredientes;
        }
        else if (totalIngredientes >= 3 && sprite3Ingredientes != null)
        {
            spriteRenderer.sprite = sprite3Ingredientes;
        }
    }

    public int GetCount(string name)
    {
        return counts.TryGetValue(name, out var v) ? v : 0;
    }

    public void ResetPizza()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        counts.Clear();
        totalIngredientes = 0;
        
        // Volta para sprite inicial
        if (spriteRenderer != null && spriteInicial != null)
        {
            spriteRenderer.sprite = spriteInicial;
        }
    }
}
