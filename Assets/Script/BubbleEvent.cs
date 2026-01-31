using UnityEngine;
using UnityEngine.EventSystems;

public class BubbleEvent : MonoBehaviour
{

    // Fait remonter l'évènement de scroll sur le parent
    public void bubbleScroll(BaseEventData ev)
    {
        // on propage l'évènement sur le parent
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.scrollHandler);
    }

    // Fait remonter l'évènement de drag sur le parent
    public void bubbleDrag(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.dragHandler);
    }

    // Fait remonter l'évènement de beginDrag sur le parent
    public void bubbleBeginDrag(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.beginDragHandler);
    }

    // Fait remonter l'évènement de EndDrag sur le parent
    public void bubbleEndDrag(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.endDragHandler);
    }

    // Fait remonter l'évènement de PointerUp sur le parent
    public void bubblePointerUp(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.pointerUpHandler);
    }
}
