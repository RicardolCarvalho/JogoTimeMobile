using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool mostrarDebugLog = true;

    private HashSet<string> itensColetados = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantém entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adiciona um item ao inventário
    /// </summary>
    public void AdicionarItem(string nomeItem)
    {
        if (string.IsNullOrEmpty(nomeItem))
        {
            Debug.LogWarning("InventoryManager: Tentou adicionar item sem nome!");
            return;
        }

        if (itensColetados.Add(nomeItem))
        {
            if (mostrarDebugLog)
                Debug.Log($"✓ Item coletado: {nomeItem} | Total: {itensColetados.Count}");
        }
        else
        {
            if (mostrarDebugLog)
                Debug.Log($"Item '{nomeItem}' já estava no inventário");
        }
    }

    /// <summary>
    /// Remove um item do inventário
    /// </summary>
    public void RemoverItem(string nomeItem)
    {
        if (itensColetados.Remove(nomeItem))
        {
            if (mostrarDebugLog)
                Debug.Log($"✗ Item removido: {nomeItem}");
        }
    }

    /// <summary>
    /// Verifica se tem um item específico
    /// </summary>
    public bool TemItem(string nomeItem)
    {
        return itensColetados.Contains(nomeItem);
    }

    /// <summary>
    /// Verifica se tem TODOS os itens da lista
    /// </summary>
    public bool TemTodosItens(List<string> itensNecessarios)
    {
        if (itensNecessarios == null || itensNecessarios.Count == 0)
            return true; // Se não precisa de nada, retorna true

        foreach (string item in itensNecessarios)
        {
            if (!itensColetados.Contains(item))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Verifica quais itens estão faltando
    /// </summary>
    public List<string> GetItensFaltando(List<string> itensNecessarios)
    {
        List<string> faltando = new List<string>();
        
        if (itensNecessarios == null) return faltando;

        foreach (string item in itensNecessarios)
        {
            if (!itensColetados.Contains(item))
                faltando.Add(item);
        }
        
        return faltando;
    }

    /// <summary>
    /// Retorna quantos itens foram coletados
    /// </summary>
    public int GetTotalItens()
    {
        return itensColetados.Count;
    }

    /// <summary>
    /// Retorna lista de todos os itens coletados
    /// </summary>
    public List<string> GetItensColetados()
    {
        return new List<string>(itensColetados);
    }

    /// <summary>
    /// Limpa todo o inventário
    /// </summary>
    public void LimparInventario()
    {
        itensColetados.Clear();
        if (mostrarDebugLog)
            Debug.Log("Inventário limpo!");
    }

    /// <summary>
    /// Debug - Mostra todos os itens no console
    /// </summary>
    [ContextMenu("Mostrar Itens no Console")]
    public void MostrarItensNoConsole()
    {
        if (itensColetados.Count == 0)
        {
            Debug.Log("Inventário vazio");
            return;
        }

        Debug.Log($"=== INVENTÁRIO ({itensColetados.Count} itens) ===");
        foreach (string item in itensColetados)
        {
            Debug.Log($"  - {item}");
        }
    }
}
