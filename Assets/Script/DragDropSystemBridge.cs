using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropSystemBridge : MonoBehaviour
{

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
        // check that the element dragged is the one where the DragDropSystemBridge is added or it's one of his child (this case can occurs if we try to drag&drop a child like the reduce button of a canvas)
        if (((PointerEventData)e).hovered.Contains(gameObject) || e.selectedObject == gameObject)
        {
            e.selectedObject = gameObject;
            DragDropSystem.instance.beginDragElementFromEditableScript(e);
        }
    }

    public void dragElement(BaseEventData e)
    {
        DragDropSystem.instance.dragElement();
    }

    public void endDragElement(BaseEventData e)
    {
        DragDropSystem.instance.endDragElement();
    }

    public void refreshHierarchyContainers()
    {
        DragDropSystem.instance.refreshHierarchyContainers(gameObject);
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
