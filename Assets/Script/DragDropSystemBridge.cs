using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropSystemBridge : MonoBehaviour
{
    public void CheckSpecialClick(BaseEventData e)
    {
        DragDropSystem.instance.CheckSpecialClick(e);
    }

    public void CheckRightClickForDelete(BaseEventData e)
    {
        // On verifie si c'est bien un clic-droit
        if ((e as PointerEventData).button == PointerEventData.InputButton.Right)
            DragDropSystem.instance.DeleteElement(gameObject);
    }

    public void SetDefaultDropZone(BaseEventData e)
    {
        // On verifie si c'est bien un clic-gauche
        if ((e as PointerEventData).button == PointerEventData.InputButton.Left)
            DragDropSystem.instance.SetDefaultDropZone(gameObject);
    }

    public void CheckHighlightDropArea()
    {
        DragDropSystem.instance.CheckHighlightDropArea(gameObject);
    }

    public void UnhighlightDropArea()
    {
        DragDropSystem.instance.UnhighlightDropArea(gameObject);
    }

    public void BeginDragElement(BaseEventData e)
    {
        DragDropSystem.instance.BeginDragElementFromEditableScript(e);
    }

    public void BeginDragElementFromLibrary(BaseEventData e)
    {
        DragDropSystem.instance.BeginDragElementFromLibrary(e);
    }

    public void DragElement(BaseEventData e)
    {
        DragDropSystem.instance.DragElement();
    }

    public void EndDragElement(BaseEventData e)
    {
        DragDropSystem.instance.EndDragElement();
    }

    public void OnlyPositiveInteger(string newValue)
    {
        DragDropSystem.instance.OnlyPositiveInteger(gameObject, newValue);
    }

    public void LockItem()
    {
        DragDropSystem.instance.LockItem(transform.parent);
    }

    public void UnlockItem()
    {
        DragDropSystem.instance.UnlockItem(transform.parent);
    }
}
