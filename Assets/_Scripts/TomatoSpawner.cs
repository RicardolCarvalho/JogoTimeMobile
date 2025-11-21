using UnityEngine;

public class TomatoSpawner : MonoBehaviour
{
    public GameObject tomatoPrefab;

    [Header("Força do lançamento")]
    public float forceX = 8f;
    public float minForceY = 10f;
    public float maxForceY = 20f;

    [Header("Tempos")]
    public float firstDelay = 1.0f;
    public float betweenDelay = 0.5f;

    [HideInInspector] public bool canSpawn = true;

    private GameObject currentTomato;
    private float timer;
    private bool firstTomato = true;

    void Update()
    {
        if (!canSpawn) return;
        if (currentTomato != null) return;

        timer += Time.deltaTime;

        float waitTime = firstTomato ? firstDelay : betweenDelay;

        if (timer >= waitTime)
        {
            timer = 0f;
            firstTomato = false;
            SpawnTomato();
        }
    }

    void SpawnTomato()
    {
        currentTomato = Instantiate(tomatoPrefab, transform.position, Quaternion.identity);

        var rb = currentTomato.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float fy = Random.Range(minForceY, maxForceY);
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(forceX, fy), ForceMode2D.Impulse);
        }
    }

    public void NotifyTomatoDestroyed()
    {
        currentTomato = null;
    }
}
