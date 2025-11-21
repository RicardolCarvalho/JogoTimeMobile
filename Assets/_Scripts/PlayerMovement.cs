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

    // 0 = nenhum, 1 = horizontal, 2 = vertical
    int lockedAxis = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1) Ler teclado (Raw para evitar aceleração)
        float ix = Input.GetAxisRaw("Horizontal"); // -1,0,1
        float iy = Input.GetAxisRaw("Vertical");   // -1,0,1

        // 2) Descobrir qual eixo deve ficar "travado" (última tecla pressionada)
        //    - Se apertar A/D, travamos horizontal
        //    - Se apertar W/S, travamos vertical
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            lockedAxis = 1; // horizontal
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            lockedAxis = 2; // vertical
        }

        // 3) Aplicar o travamento de eixo
        if (lockedAxis == 1)      // horizontal
        {
            input.x = ix;
            input.y = 0f;
            if (Mathf.Approximately(ix, 0f)) lockedAxis = 0; // soltou -> libera
        }
        else if (lockedAxis == 2) // vertical
        {
            input.x = 0f;
            input.y = iy;
            if (Mathf.Approximately(iy, 0f)) lockedAxis = 0; // soltou -> libera
        }
        else
        {
            // Nenhum eixo travado ainda: se só uma direção está ativa, use ela;
            // se ambas estão ativas, priorize a mais forte (ou nada)
            if (Mathf.Abs(ix) > 0.01f && Mathf.Abs(iy) < 0.01f)
            {
                lockedAxis = 1; input.x = ix; input.y = 0f;
            }
            else if (Mathf.Abs(iy) > 0.01f && Mathf.Abs(ix) < 0.01f)
            {
                lockedAxis = 2; input.x = 0f; input.y = iy;
            }
            else
            {
                // ambas pressionadas ao mesmo tempo: opcionalmente priorize a última tecla
                input = Vector2.zero;
            }
        }

        // 4) Animator params
        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);
        animator.SetFloat("Speed", input.sqrMagnitude);

        if (input.sqrMagnitude > 0.0001f)
            lastMoveDir = input;

        animator.SetFloat("LastX", lastMoveDir.x);
        animator.SetFloat("LastY", lastMoveDir.y);

        // 5) Flip apenas para movimento horizontal
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y) && Mathf.Abs(input.x) > 0.01f)
            sr.flipX = input.x > 0f;      // seu sprite base olha para ESQUERDA
        else
            sr.flipX = lastMoveDir.x > 0f;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }
}
