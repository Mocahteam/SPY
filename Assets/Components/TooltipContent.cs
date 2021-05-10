using UnityEngine.EventSystems;
using UnityEngine;

public class TooltipContent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string text;
    private Tooltip tooltip;

    private bool isOver = false;

    private void Start()
    {
        tooltip = GameObject.Find("TooltipUI").GetComponent<Tooltip>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.ShowTooltip(text);
        isOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.HideTooltip();
        isOver = false;
    }

    public void OnDisable()
    {
        if (isOver)
        {
            tooltip.HideTooltip();
            isOver = false;
        }
    }
}
