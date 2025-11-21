using UnityEngine;

public class DragItem : MonoBehaviour
{
    private string ingredientName;
    private Camera cam;
    private bool dragging = true;

    public void Init(string name)
    {
        ingredientName = name;
        cam = Camera.main;
        transform.localScale = Vector3.one * 0.6f;
        SnapToMouse();
    }

    private void Update()
    {
        if (dragging)
        {
            SnapToMouse();
            if (Input.GetMouseButtonUp(0))
            {
                dragging = false;
                TryDropOnPizza();
            }
        }
    }

    private void SnapToMouse()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
        m.z = 0f;
        transform.position = m;
    }

    private void TryDropOnPizza()
    {
        var pizza = FindFirstObjectByType<Pizza>();
        if (pizza == null) { Destroy(gameObject); return; }

        Vector2 p = transform.position;
        if (pizza.ContainsPoint(p))
        {
            transform.SetParent(pizza.transform);
            transform.Rotate(0f, 0f, Random.Range(-20f, 20f));
            pizza.AddIngredient(ingredientName);   // informa a pizza
            return;
        }

        Destroy(gameObject);
    }
}
