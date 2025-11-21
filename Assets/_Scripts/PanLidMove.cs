using UnityEngine;

public class PanLidMove : MonoBehaviour
{
    [Header("Movimento vertical")]
    public float moveSpeed = 6f;
    public float minY = -3.5f;
    public float maxY = 3.5f;

    [Header("Inclinação")]
    public float rotateSpeed = 120f;
    public float maxAngle = 45f;  // limite de inclinação para cada lado

    private float currentAngle;

    void Update()
    {
        MoverVertical();
        Inclinar();
    }

    private void MoverVertical()
    {
        float inputY = Input.GetAxisRaw("Vertical"); // W S setas
        float delta = inputY * moveSpeed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y + delta, minY, maxY);
        transform.position = pos;
    }

    private void Inclinar()
    {
        float inputX = Input.GetAxisRaw("Horizontal"); // A D setas
        if (Mathf.Abs(inputX) > 0.01f)
        {
            currentAngle += -inputX * rotateSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, -maxAngle, maxAngle);
        }

        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}
