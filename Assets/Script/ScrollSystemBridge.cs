using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollSystemBridge : MonoBehaviour
{
    public void onScroll(BaseEventData ev)
    {
        ScrollSystem.instance.onScroll(gameObject, ev);
    }
    public void onDrag(BaseEventData ev)
    {
        ScrollSystem.instance.onDrag(gameObject, ev);
    }
    public void onBeginDrag(BaseEventData ev)
    {
        ScrollSystem.instance.onBeginDrag(gameObject, ev);
    }
    public void onEndDrag(BaseEventData ev)
    {
        ScrollSystem.instance.onEndDrag(gameObject, ev);
    }

    public void setVerticalSpeed(float newSpeed)
    {
        AutoScroll autoScroll = GetComponent<AutoScroll>();
        if (autoScroll != null)
            ScrollSystem.instance.setVerticalSpeed(autoScroll, newSpeed);
    }

    public void setHorizontalSpeed(float newSpeed)
    {
        AutoScroll autoScroll = GetComponent<AutoScroll>();
        if (autoScroll != null)
            ScrollSystem.instance.setHorizontalSpeed(autoScroll, newSpeed);
    }
}
