using UnityEngine;
using UnityEngine.EventSystems;

public class TapButton : MonoBehaviour, IPointerDownHandler
{
    [HideInInspector] public bool wasTappedThisFrame = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        wasTappedThisFrame = true;
    }

    void LateUpdate()
    {
        wasTappedThisFrame = false;
    }
}
