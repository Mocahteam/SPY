using UnityEngine;
using UnityEngine.EventSystems;

public class BubbleScrollEvent : MonoBehaviour
{
    public void onScroll(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, ev, ExecuteEvents.scrollHandler);
    }
}
