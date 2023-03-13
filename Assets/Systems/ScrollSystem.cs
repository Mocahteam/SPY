using FYFY;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Scroll automatically editable panel when player drag an item on the borders of the editable panel
// Bubble scroll event in editable panel
public class ScrollSystem : FSystem
{
    private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));

    public ScrollRect scrollRect;

    public RectTransform autoScrollUp;
    public RectTransform autoScrollDown;
    public RectTransform autoScrollLeft;
    public RectTransform autoScrollRight;

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

            // check if we overlap autoscrolls
            if (f_dragging.Count > 0)
            {
                setVerticalSpeed(0);
                setHorizontalSpeed(0);
                // check up
                Vector3 draggedInAutoScroll = autoScrollUp.InverseTransformPoint(f_dragging.First().transform.position);
                if (Math.Abs(draggedInAutoScroll.x) < autoScrollUp.rect.width / 2 && 0 < draggedInAutoScroll.y && draggedInAutoScroll.y < autoScrollUp.rect.height)
                    setVerticalSpeed(0.01f);
                // check down
                draggedInAutoScroll = autoScrollDown.InverseTransformPoint(f_dragging.First().transform.position);
                if (Math.Abs(draggedInAutoScroll.x) < autoScrollDown.rect.width / 2 && 0 < draggedInAutoScroll.y && draggedInAutoScroll.y < autoScrollDown.rect.height)
                    setVerticalSpeed(-0.01f);
                // check left
                draggedInAutoScroll = autoScrollLeft.InverseTransformPoint(f_dragging.First().transform.position);
                if (0 < draggedInAutoScroll.x && draggedInAutoScroll.x < autoScrollLeft.rect.width && Math.Abs(draggedInAutoScroll.y) < autoScrollLeft.rect.height / 2)
                    setHorizontalSpeed(-0.01f);
                // check right
                draggedInAutoScroll = autoScrollRight.InverseTransformPoint(f_dragging.First().transform.position);
                if (0 < draggedInAutoScroll.x && draggedInAutoScroll.x < autoScrollRight.rect.width && Math.Abs(draggedInAutoScroll.y) < autoScrollRight.rect.height / 2)
                    setHorizontalSpeed(0.01f);
            }
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
