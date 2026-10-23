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

    private void ShowTooltip()
    {
        if (tooltip != null)
            tooltip.ShowTooltip(text);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pointActionUI != null && pointActionUI.enabled)
            ShowTooltip();
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (pointActionUI != null)
            pointActionUI.Disable(); // Pour ne pas que l'objet sélectionné par code ait son tooltip qui continue à suivre le curseur de la souris
        ShowTooltip();
    }
    
    private void HideTooltip()
    {
        if (tooltip != null)
            tooltip.HideTooltip();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (pointActionUI != null && pointActionUI.enabled)
            HideTooltip();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        HideTooltip();
    }

    public void OnDisable()
    {
        HideTooltip();
    }

    public void SetText(string newContent)
    {
        text = newContent;
    }
}
