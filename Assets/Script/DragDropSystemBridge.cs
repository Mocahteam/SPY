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

        }
        else
        {
            Debug.Log("Up droit detecte");
            DragDropSystem.instance.deleteElement(e.selectedObject);
        }

    }

    public void dropElement(GameObject element)
    {
        DragDropSystem.instance.dropElementInContainer(element);
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
