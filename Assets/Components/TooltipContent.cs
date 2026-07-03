using UnityEngine.EventSystems;
using UnityEngine;
using FYFY;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class TooltipContent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public string text;
    private Tooltip tooltip = null;
    private InputAction pointActionUI;

    private void Awake()
    {
        GameObject tooltipGO = GameObject.Find("TooltipUI_Pointer");
        if (!tooltipGO)
        {
            GameObjectManager.unbind(gameObject);
            GameObject.Destroy(this);
        }
        else
            tooltip = tooltipGO.GetComponent<Tooltip>();

        pointActionUI = EventSystem.current.GetComponent<InputSystemUIInputModule>().point.action;
    }

    private void showTooltip()
    {
        if (tooltip != null)
            tooltip.ShowTooltip(text);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pointActionUI.enabled)
            showTooltip();
    }
    public void OnSelect(BaseEventData eventData)
    {
        pointActionUI.Disable(); // Pour ne pas que l'objet sélectionné par code ait son tooltip qui continue à suivre le curseur de la souris
        showTooltip();
    }
    
    private void hideTooltip()
    {
        if (tooltip != null)
            tooltip.HideTooltip();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (pointActionUI.enabled)
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

    public void setText(string newContent)
    {
        text = newContent;
    }
}
