using System.Collections;
using System.Collections.Generic;
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
        DragDropSystem.instance.beginDragElementFromEditableScript(e);
    }

    public void dragElement(BaseEventData e)
    {
        DragDropSystem.instance.dragElement();
    }

    public void endDragElement(BaseEventData e)
    {
        DragDropSystem.instance.endDragElement();
    }
}
