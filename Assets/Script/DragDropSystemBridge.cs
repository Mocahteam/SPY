using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropSystemBridge : MonoBehaviour
{
    public void checkDoubleClick(BaseEventData e)
    {
        DragDropSystem.instance.checkDoubleClick(e);
    }
    public void checkRightClickForDelete(BaseEventData e)
    {
        // On verifie si c'est bien un clic-droit
        if ((e as PointerEventData).button == PointerEventData.InputButton.Right)
            DragDropSystem.instance.deleteElement(gameObject);
    }

    public void checkHighlightDropArea()
    {
        DragDropSystem.instance.checkHighlightDropArea(gameObject);
    }
    public void unhighlightDropArea()
    {
        DragDropSystem.instance.unhighlightDropArea(gameObject);
    }

    public void beginDragElement(BaseEventData e)
    {
        DragDropSystem.instance.beginDragElementFromEditableScript(e);
    }

    public void beginDragElementFromEditableScript(BaseEventData e)
    {
        DragDropSystem.instance.beginDragElementFromEditableScript(e);
    }

    public void beginDragElementFromLibrary(BaseEventData e)
    {
        DragDropSystem.instance.beginDragElementFromLibrary(e);
    }
    
    public void dragElement(BaseEventData e)
    {
        DragDropSystem.instance.dragElement();
    }

    public void endDragElement(BaseEventData e)
    {
        DragDropSystem.instance.endDragElement();
    }

    public void onlyPositiveInteger(string newValue)
    {
        DragDropSystem.instance.onlyPositiveInteger(gameObject, newValue);
    }
    public void setNextFocusedGameObject(GameObject go)
    {
        EventSystem.current.SetSelectedGameObject(go);
    }
}
