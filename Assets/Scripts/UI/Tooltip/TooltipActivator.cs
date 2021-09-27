using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipActivator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [TextArea(10, 20)]
    public string text;

    private bool isHovering;

    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
    }
	
	void Update () {
        if (isHovering) TooltipController.ActivateTooltip(text);
	}
}
