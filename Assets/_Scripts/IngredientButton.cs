using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class IngredientButton : MonoBehaviour
{
    public string ingredientName = "Molho";

    private void OnMouseDown()
    {
        // pega o SpriteRenderer do próprio botão
        var srcSR = GetComponent<SpriteRenderer>();

        // cria o item que já nasce seguindo o mouse
        var go = new GameObject($"Drag_{ingredientName}");

        // adiciona SpriteRenderer ao item e copia as propriedades visuais
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = srcSR != null ? srcSR.sprite : null;
        sr.color  = srcSR != null ? srcSR.color  : Color.white;
        sr.sortingOrder = 100; // garante que fique visível sobre os outros

        // adiciona um collider simples
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // adiciona o script de arrasto com o nome do ingrediente
        var drag = go.AddComponent<DragItem>();
        drag.Init(ingredientName);
    }
}
