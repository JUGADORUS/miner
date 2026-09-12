using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonNotifier : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameEvents.OnButtonHovered?.Invoke(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameEvents.OnButtonClicked?.Invoke(gameObject);
    }
}