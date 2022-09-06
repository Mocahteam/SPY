using UnityEngine.EventSystems;
using UnityEngine;
using FYFY;

public class TooltipContent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string text;
    private Tooltip tooltip;

    private bool isOver = false;

    private void Start()
    {
        GameObject tooltipGO = GameObject.Find("TooltipUI");
        if (!tooltipGO)
        {
            GameObjectManager.unbind(gameObject);
            GameObject.Destroy(this);
        }
        else
            tooltip = tooltipGO.GetComponent<Tooltip>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(text.Contains("#agentName"))
            text = text.Replace("#agentName", GetComponent<AgentEdit>().agentName);
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
