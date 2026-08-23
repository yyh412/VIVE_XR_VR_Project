using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image hoverBackground;

    private void Start()
    {
        if (hoverBackground != null)
            hoverBackground.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Enter: " + gameObject.name);

        if (hoverBackground != null)
            hoverBackground.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer Exit: " + gameObject.name);

        if (hoverBackground != null)
            hoverBackground.enabled = false;
    }
}