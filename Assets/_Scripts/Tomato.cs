using UnityEngine;

public class Tomato : MonoBehaviour
{
    private TomatoSpawner spawner;
    private bool ended; // garante que só finaliza uma vez

    void Start()
    {
        spawner = FindFirstObjectByType<TomatoSpawner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ended) return;

        if (other.CompareTag("Cauldron"))
        {
            ended = true;
            MolhoController.instance.AddPoint();
            NotifyAndDestroy();
        }
        else if (other.CompareTag("Ground"))
        {
            ended = true;
            MolhoController.instance.AddError();
            NotifyAndDestroy();
        }
    }

    private void OnBecameInvisible()
    {
        // Se o tomate saiu da tela sem encostar em nada, só limpa.
        // Não conta ponto nem erro, apenas libera o próximo.
        if (ended) return;
        ended = true;
        NotifyAndDestroy();
    }

    private void NotifyAndDestroy()
    {
        if (spawner != null)
        {
            spawner.NotifyTomatoDestroyed();
        }

        Destroy(gameObject);
    }
}
