using FYFY;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Scroll automatically editable panel when player drag an item on the borders of the editable panel
// Bubble scroll event in editable panel
public class ScrollSystem : FSystem
{
    public ScrollRect scrollRect;

    private float verticalSpeed;
    private float horizontalSpeed;

    public static ScrollSystem instance;

    public ScrollSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        verticalSpeed = 0;
        horizontalSpeed = 0;
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (scrollRect != null)
        {
            scrollRect.verticalScrollbar.value += verticalSpeed;
            scrollRect.horizontalScrollbar.value += horizontalSpeed;
        }
    }

    public void setVerticalSpeed(float newSpeed)
    {
        verticalSpeed = newSpeed;
    }

    public void setHorizontalSpeed(float newSpeed)
    {
        horizontalSpeed = newSpeed;
    }

    public void onScroll(GameObject target, BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(target.transform.parent.gameObject, ev, ExecuteEvents.scrollHandler);
    }
}
