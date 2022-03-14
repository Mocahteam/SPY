using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropSystemBridge : MonoBehaviour
{

  public void pointerUpElement(BaseEventData e)
    {
        Debug.Log("Up pointer detecte "+e.selectedObject.name);
        // On verifie si c'est un up droit ou gauche
        PointerEventData pointerEventData = e as PointerEventData;
        if (pointerEventData.button == PointerEventData.InputButton.Left && e.selectedObject != null)
        {
            Debug.Log("Up gauche detecte");
           // DragDropSystem.instance.pointerRightUpElement(e.selectedObject);
        }
        else
        {
            DragDropSystem.instance.pointerLeftUpElement(e.selectedObject);
        }

    }

    public void pointerDownElement(BaseEventData e)
    {
        Debug.Log("Pointer down detecte" + e.selectedObject.name);
        // On verifie si c'est un up droit ou gauche
        PointerEventData pointerEventData = e as PointerEventData;
        if (pointerEventData.button == PointerEventData.InputButton.Left && e.selectedObject != null)
        {
            Debug.Log("Down gauche detecte");
            DragDropSystem.instance.pointerDownElement(e.selectedObject);
        }
    }

    public void dragElement(BaseEventData e)
    {
        Debug.Log("Drag detecte" + e.selectedObject.name);

        DragDropSystem.instance.dragElement(e.selectedObject);
    }

    public void beginDragElement(BaseEventData e)
    {
        Debug.Log("Begin Drag detecte" + e.selectedObject.name);

        DragDropSystem.instance.beginDragElement(e.selectedObject);
    }

    public void endDragElement(BaseEventData e)
    {
        Debug.Log("End Drag detecte" + e.selectedObject.name);
        DragDropSystem.instance.endDragElement(e.selectedObject);
    }
}
