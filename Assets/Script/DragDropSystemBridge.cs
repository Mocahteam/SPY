using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropSystemBridge : MonoBehaviour
{
  public void DropElement(BaseEventData e)
    {
        Debug.Log(e.selectedObject.name);
        DragDropSystem.instance.DropElement(e.selectedObject);
    }

  public void EndDragAction(GameObject go)
    {
        Debug.Log("EndDrag action");
        if(go == null)
        {
            Debug.Log("Event is null");
        }
        else
        {
            Debug.Log("Name object");
            Debug.Log(go.name);
        }
    }

    public void MoveAction()
    {
        Debug.Log("move action");
    }
}
