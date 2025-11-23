using UnityEngine;

public class KnifeMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;

    [Header("Limitar à tela")]
    public bool constrainToScreen = true;
    public float padding = 0.05f;

    [Header("Botões Mobile (Opcional)")]
    public HoldButton btnUp;
    public HoldButton btnDown;
    public HoldButton btnLeft;
    public HoldButton btnRight;

    private Camera cam;
    private float halfW;
    private float halfH;

    void Start()
    {
        cam = Camera.main;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var ext = sr.bounds.extents;
            halfW = ext.x;
            halfH = ext.y;
        }
        else
        {
            halfW = 0.25f;
            halfH = 0.25f;
        }
    }

    void Update()
    {
        // WASD e setas compartilham os eixos Horizontal e Vertical
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Botões mobile
        if (btnLeft && btnLeft.isPressed) moveX = -1;
        if (btnRight && btnRight.isPressed) moveX = 1;
        if (btnUp && btnUp.isPressed) moveY = 1;
        if (btnDown && btnDown.isPressed) moveY = -1;

        Vector2 movement = new Vector2(moveX, moveY).normalized;

        transform.Translate(movement * speed * Time.deltaTime);

        if (constrainToScreen && cam != null)
        {
            ClampToCameraBounds();
        }
    }

    private void ClampToCameraBounds()
    {
        // Cálculo do retângulo visível da câmera ortográfica
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        Vector3 camPos = cam.transform.position;
        Vector3 pos = transform.position;

        float minX = camPos.x - horzExtent + halfW + padding;
        float maxX = camPos.x + horzExtent - halfW - padding;
        float minY = camPos.y - vertExtent + halfH + padding;
        float maxY = camPos.y + vertExtent - halfH - padding;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}
