using UnityEngine.EventSystems;
using UnityEngine;
using FYFY;

public class TooltipContent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public string text;
    private Tooltip tooltip = null;

    private void Start()
    {
        GameObject tooltipGO = GameObject.Find("TooltipUI_Pointer");
        if (!tooltipGO)
        {
            GameObjectManager.unbind(gameObject);
            GameObject.Destroy(this);
        }
        else
            tooltip = tooltipGO.GetComponent<Tooltip>();
    }

    private void formatContent()
    {
        if (tooltip != null)
        {
            string formatedContent = text;
            if (text.Contains("#agentName"))
                formatedContent = text.Replace("#agentName", GetComponent<AgentEdit>().associatedScriptName);
            tooltip.ShowTooltip(formatedContent);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        formatContent();
    }
    public void OnSelect(BaseEventData eventData)
    {
        formatContent();
    }
    
    private void hideTooltip()
    {
        if (tooltip != null)
            tooltip.HideTooltip();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hideTooltip();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        hideTooltip();
    }

    public void OnDisable()
    {
        hideTooltip();
    }
}
