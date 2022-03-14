using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropSystemBridge : MonoBehaviour
{

  public void pointerUpElement(BaseEventData e)
    {
        //Debug.Log("Up pointer detecte "+e.selectedObject.name);
        // On verifie si c'est un up droit ou gauche
        if ((e as PointerEventData).button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Up gauche detecte");
           DragDropSystem.instance.dropElementInContainer(e.selectedObject);
        }
        else
        {
            Debug.Log("Up droit detecte");
            //DragDropSystem.instance.pointerRightUpElement(e.selectedObject);
        }

    }

    public void dropElement(GameObject element)
    {
        DragDropSystem.instance.dropElementInContainer(element);
    }

    public void pointerDownElement(BaseEventData e)
    {
        //Debug.Log("Pointer down detecte" + e.selectedObject.name);
        // On verifie si c'est un up droit ou gauche
        PointerEventData pointerEventData = e as PointerEventData;
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Down gauche detecte");
            DragDropSystem.instance.pointerDownElement(e.selectedObject);
        }
    }

    public void beginDragElement(BaseEventData e)
    {
        //Debug.Log("Begin Drag detecte" + e.selectedObject.name);

        DragDropSystem.instance.beginDragElementFromEditableScript(e);
    }


    public void dragElement(BaseEventData e)
    {
        //Debug.Log("Drag detecte" + e.selectedObject.name);

        DragDropSystem.instance.dragElement();
    }

    public void endDragElement(BaseEventData e)
    {
       // Debug.Log("End Drag detecte" + e.selectedObject.name);
        DragDropSystem.instance.endDragElement();
    }
}
