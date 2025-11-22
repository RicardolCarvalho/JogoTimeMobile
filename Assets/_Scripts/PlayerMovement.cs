using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 4f;
    public Animator animator;

    Rigidbody2D rb;
    SpriteRenderer sr;

    Vector2 input;
    Vector2 lastMoveDir = Vector2.down;

    int lockedAxis = 0; // 0 = nenhum, 1 = horizontal, 2 = vertical

    // ===============================
    // ADIÇÃO PARA MOBILE
    // ===============================
    [Header("Botões Mobile (opcional)")]
    public HoldButton btnUp;     // W
    public HoldButton btnDown;   // S
    public HoldButton btnLeft;   // A
    public HoldButton btnRight;  // D
    // ===============================


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ============================================
        // 1) Ler teclado OU botões mobile
        // ============================================

        // TECLADO
        float ix = Input.GetAxisRaw("Horizontal"); 
        float iy = Input.GetAxisRaw("Vertical");

        // MOBILE (usar apenas se existirem)
        if (btnLeft && btnLeft.isPressed)   ix = -1;
        if (btnRight && btnRight.isPressed) ix =  1;
        if (btnUp && btnUp.isPressed)       iy =  1;
        if (btnDown && btnDown.isPressed)   iy = -1;
        // ============================================


        // ============================================
        // 2) SEU CÓDIGO ORIGINAL — TRAVAMENTO DE EIXO
        // ============================================

        // detectar qual eixo deve travar
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            (btnLeft && btnLeft.isPressed) || (btnRight && btnRight.isPressed))
        {
            lockedAxis = 1; // horizontal
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
            (btnUp && btnUp.isPressed) || (btnDown && btnDown.isPressed))
        {
            lockedAxis = 2; // vertical
        }

        // aplicar travamento
        if (lockedAxis == 1)
        {
            input.x = ix;
            input.y = 0f;
            if (Mathf.Approximately(ix, 0f))
                lockedAxis = 0;
        }
        else if (lockedAxis == 2)
        {
            input.x = 0f;
            input.y = iy;
            if (Mathf.Approximately(iy, 0f))
                lockedAxis = 0;
        }
        else
        {
            if (Mathf.Abs(ix) > 0.01f && Mathf.Abs(iy) < 0.01f)
            {
                lockedAxis = 1;
                input.x = ix; input.y = 0f;
            }
            else if (Mathf.Abs(iy) > 0.01f && Mathf.Abs(ix) < 0.01f)
            {
                lockedAxis = 2;
                input.x = 0f; input.y = iy;
            }
            else
            {
                input = Vector2.zero;
            }
        }

        // ============================================


        // seu animator original
        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);
        animator.SetFloat("Speed", input.sqrMagnitude);

        if (input.sqrMagnitude > 0.0001f)
            lastMoveDir = input;

        animator.SetFloat("LastX", lastMoveDir.x);
        animator.SetFloat("LastY", lastMoveDir.y);

        // flip horizontal
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y) && Mathf.Abs(input.x) > 0.01f)
            sr.flipX = input.x > 0f;
        else
            sr.flipX = lastMoveDir.x > 0f;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }
}
