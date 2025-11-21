using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    [Serializable]
    public class Req
    {
        public string name;
        public int qty;
    }

    [Header("Referências")]
    public Pizza pizza;            // arraste a Pizza da cena aqui
    public TextMeshProUGUI uiText;
          // arraste um Text da UI aqui

    [Header("Pedido")]
    public List<Req> recipe = new List<Req>
    {
        new Req { name = "Molho", qty = 1 },
        new Req { name = "Calabresa", qty = 2 },
        new Req { name = "Queijo", qty = 1 }
    };

    [Header("Progresso")]
    public float speedIncrement = 0.6f;
    public int niveisCompletados = 0;
    public int maxNiveis = 3;

    [Header("Trocar de Cena")]
    [Tooltip("Cena para onde vai após completar os 3 níveis")]
    public string cenaDestino;

    void Update()
    {
        if (pizza == null) return;

        // atualiza UI simples
        if (uiText != null) uiText.text = BuildStatus();

        // checa conclusão
        if (IsRecipeComplete())
        {
            niveisCompletados++;

            // Verifica se completou os 3 níveis
            if (niveisCompletados >= maxNiveis)
            {
                if (cenaDestino != null)
                {
                    SceneManager.LoadScene(cenaDestino);
                }
                else
                {
                    Debug.LogWarning("GameController: Completou 3 níveis mas não há cena destino configurada!");
                }
                return;
            }

            // acelera a esteira
            pizza.speed += speedIncrement;

            // próxima pizza, limpa e reposiciona
            pizza.ResetPizza();
            var pos = pizza.transform.position;
            pos.x = pizza.spawnX;
            pizza.transform.position = pos;

            // opcional: embaralhar nova ordem fixa simples
            ShuffleRecipe();
        }
    }

    private bool IsRecipeComplete()
    {
        foreach (var r in recipe)
        {
            if (pizza.GetCount(r.name) < r.qty) return false;
        }
        return true;
    }

    private string BuildStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>Nível: {niveisCompletados + 1}/{maxNiveis}</b>");
        sb.AppendLine("<b>Pedido:</b>");
        foreach (var r in recipe)
        {
            int have = pizza.GetCount(r.name);
            sb.AppendLine($"{r.name}: {have}/{r.qty}");
        }
        sb.AppendLine($"Velocidade: {pizza.speed:0.0}");
        return sb.ToString();
    }

    private void ShuffleRecipe()
    {
        // troca só quantidades de 1 a 3, mantém nomes
        for (int i = 0; i < recipe.Count; i++)
            recipe[i].qty = UnityEngine.Random.Range(1, 4);
    }
}
