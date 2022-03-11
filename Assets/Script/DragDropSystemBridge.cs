using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropSystemBridge : MonoBehaviour
{

  public void pointerUpElement(BaseEventData e)
    {
        Debug.Log("Up pointer detecte");
        // On verifie si c'est un up droit ou gauche
        PointerEventData pointerEventData = e as PointerEventData;
        if (pointerEventData.button == PointerEventData.InputButton.Left && e.selectedObject != null)
        {
            Debug.Log("Up gauche detecte");
            DragDropSystem.instance.pointerRightUpElement(e.selectedObject);
        }
        else
        {
            DragDropSystem.instance.pointerLeftUpElement(e.selectedObject);
        }

    }

    public void pointerDownElement(BaseEventData e)
    {
        Debug.Log("Pointer down detecte");
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
        Debug.Log("Drag detecte");

        DragDropSystem.instance.dragElement(e.selectedObject);
    }
}
