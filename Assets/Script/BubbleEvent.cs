using UnityEngine;
using UnityEngine.EventSystems;

public class BubbleEvent : MonoBehaviour
{

    // Fait remonter l'évènement de scroll sur le parent
    public void BubbleScroll(BaseEventData ev)
    {
        // on propage l'évènement sur le parent
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.scrollHandler);
    }

    // Fait remonter l'évènement de drag sur le parent
    public void BubbleDrag(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.dragHandler);
    }

    // Fait remonter l'évènement de beginDrag sur le parent
    public void BubbleBeginDrag(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.beginDragHandler);
    }

    // Fait remonter l'évènement de EndDrag sur le parent
    public void BubbleEndDrag(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.endDragHandler);
    }

    // Fait remonter l'évènement de PointerUp sur le parent
    public void BubblePointerUp(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.pointerUpHandler);
    }

    // Fait remonter l'évènement de PointerDown sur le parent
    public void BubblePointerDown(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(gameObject.transform.parent.gameObject, ev, ExecuteEvents.pointerDownHandler);
    }
}
