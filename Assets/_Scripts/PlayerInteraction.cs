using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Objetos Interagíveis")]
    [Tooltip("Arraste aqui os GameObjects dos interagíveis")]
    public List<GameObject> objetosInteragiveis = new List<GameObject>();

    [Header("Configurações de Detecção")]
    [Tooltip("Distância máxima para detectar objetos interagíveis (usado como fallback)")]
    [Range(0.5f, 5f)]
    public float distanciaDeteccao = 2f;

    private GameObject goMaisProximo;

    // NOVO: conjunto dos que estão DENTRO da área de detecção (trigger do Player)
    private readonly HashSet<GameObject> dentroDaArea = new HashSet<GameObject>();

    void Update()
    {
        VerificarObjetoMaisProximo();

        bool interactPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        if (Gamepad.current != null)
            interactPressed |= Gamepad.current.buttonNorth.wasPressedThisFrame;

        if (interactPressed && goMaisProximo != null)
        {
            var interagivel = GetInteragivel(goMaisProximo);
            if (interagivel != null) interagivel.Interagir();
        }
    }

    void VerificarObjetoMaisProximo()
    {
        GameObject novoGoProximo = null;
        float menorDistancia = float.MaxValue;

        // 1) Se há itens dentro do trigger, priorize-os
        if (dentroDaArea.Count > 0)
        {
            foreach (var go in dentroDaArea)
            {
                if (go == null) continue;
                float d = CalcularDistancia(go);
                if (d < menorDistancia) { menorDistancia = d; novoGoProximo = go; }
            }
        }
        else
        {
            // 2) Fallback: varre por distância como antes
            menorDistancia = distanciaDeteccao;
            foreach (var go in objetosInteragiveis)
            {
                if (go == null) continue;
                float d = CalcularDistancia(go);
                if (d <= distanciaDeteccao && d < menorDistancia)
                {
                    menorDistancia = d;
                    novoGoProximo = go;
                }
            }
        }

        if (novoGoProximo != goMaisProximo)
        {
            if (goMaisProximo != null)
            {
                var interAnterior = GetInteragivel(goMaisProximo);
                if (interAnterior != null) interAnterior.DesativarContorno();
            }

            if (novoGoProximo != null)
            {
                var interNovo = GetInteragivel(novoGoProximo);
                if (interNovo != null) interNovo.AtivarContorno();
            }

            goMaisProximo = novoGoProximo;
        }
    }

    // ---------- TRIGGERS da área do Player (já existente) ----------
    void OnTriggerEnter2D(Collider2D other)
    {
        var alvo = PegaAlvoListado(other.gameObject);
        if (alvo != null) dentroDaArea.Add(alvo);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var alvo = PegaAlvoListado(other.gameObject);
        if (alvo != null)
        {
            if (goMaisProximo == alvo)
            {
                var inter = GetInteragivel(goMaisProximo);
                if (inter != null) inter.DesativarContorno();
                goMaisProximo = null;
            }
            dentroDaArea.Remove(alvo);
        }
    }

    // ---------- Calcula distância considerando objetos compostos ----------
    float CalcularDistancia(GameObject go)
    {
        // Se for InteragivelComposto ou InteragivelCompostoPorta (GameObject pai sem SpriteRenderer), usa o centro dos sprites filhos
        var composto = go.GetComponent<InteragivelComposto>();
        var compostoPorta = go.GetComponent<InteragivelCompostoPorta>();
        
        if (composto != null || compostoPorta != null)
        {
            var renderers = go.GetComponentsInChildren<SpriteRenderer>(false);
            if (renderers.Length > 0)
            {
                // Calcula o centro dos bounds de todos os sprites
                Bounds combined = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    combined.Encapsulate(renderers[i].bounds);
                
                return Vector2.Distance(transform.position, combined.center);
            }
        }
        
        // Para InteragivelSimples ou fallback, usa a posição do GameObject
        return Vector2.Distance(transform.position, go.transform.position);
    }

    // ---------- helpers p/ manter sua lista exatamente como está ----------
    bool EstaNaLista(GameObject go) => objetosInteragiveis.Contains(go);

    GameObject PegaAlvoListado(GameObject go)
    {
        // aceita o próprio GO ou o root dele (caso você tenha arrastado o pai)
        if (EstaNaLista(go)) return go;
        var root = go.transform.root.gameObject;
        if (EstaNaLista(root)) return root;
        return null;
    }

    // ---------- shims para não mudar seus interagíveis ----------
    private IInteragivelShim GetInteragivel(GameObject go)
    {
        var s = go.GetComponent<InteragivelSimples>();
        if (s != null) return new ShimSimples(s);

        var c = go.GetComponent<InteragivelComposto>();
        if (c != null) return new ShimComposto(c);

        var p = go.GetComponent<InteragivelCompostoPorta>();
        if (p != null) return new ShimCompostoPorta(p);

        var i = go.GetComponent<InteragivelItem>();
        if (i != null) return new ShimItem(i);

        return null;
    }

    private interface IInteragivelShim { void Interagir(); void AtivarContorno(); void DesativarContorno(); }

    private class ShimSimples : IInteragivelShim
    {
        private readonly InteragivelSimples _i; public ShimSimples(InteragivelSimples i) { _i = i; }
        public void Interagir() => _i.Interagir();
        public void AtivarContorno() => _i.AtivarContorno();
        public void DesativarContorno() => _i.DesativarContorno();
    }

    private class ShimComposto : IInteragivelShim
    {
        private readonly InteragivelComposto _i; public ShimComposto(InteragivelComposto i) { _i = i; }
        public void Interagir() => _i.Interagir();
        public void AtivarContorno() => _i.AtivarContorno();
        public void DesativarContorno() => _i.DesativarContorno();
    }

    private class ShimCompostoPorta : IInteragivelShim
    {
        private readonly InteragivelCompostoPorta _i; public ShimCompostoPorta(InteragivelCompostoPorta i) { _i = i; }
        public void Interagir() => _i.Interagir();
        public void AtivarContorno() => _i.AtivarContorno();
        public void DesativarContorno() => _i.DesativarContorno();
    }

    private class ShimItem : IInteragivelShim
    {
        private readonly InteragivelItem _i; public ShimItem(InteragivelItem i) { _i = i; }
        public void Interagir() => _i.Interagir();
        public void AtivarContorno() => _i.AtivarContorno();
        public void DesativarContorno() => _i.DesativarContorno();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);
    }
}
