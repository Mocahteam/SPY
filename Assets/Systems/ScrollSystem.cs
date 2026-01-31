using FYFY;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Scroll automatically editable panel when player drag an item on the borders of the editable panel
public class ScrollSystem : FSystem
{
    private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction), typeof(LibraryItemRef), typeof(CurrentAction)));
    private Family f_autoScroll = FamilyManager.getFamily(new AllOfComponents(typeof(AutoScroll), typeof(ScrollRect)));
    private Family f_scrollOn = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollOn)));

    private EventSystem eventSystem;
    private GameObject lastSelected = null;

    public static ScrollSystem instance;

    public ScrollSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        eventSystem = EventSystem.current;

        f_currentActions.addEntryCallback(focusViewOn);
        f_scrollOn.addEntryCallback(delegate (GameObject go) { MainLoop.instance.StartCoroutine(delayFocusOn(go)); });
    }

    private IEnumerator delayFocusOn(GameObject go)
    {
        yield return null;
        focusViewOn(go);
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        foreach (GameObject autoGo in f_autoScroll)
        {
            ScrollRect scrollRect = autoGo.GetComponent<ScrollRect>();
            AutoScroll autoscroll = autoGo.GetComponent<AutoScroll>();

            scrollRect.verticalScrollbar.value += autoscroll.verticalSpeed;
            scrollRect.horizontalScrollbar.value += autoscroll.horizontalSpeed;

            // check if we overlap autoscrolls => useful for keyboard dragging accessibility
            if (f_dragging.Count > 0)
            {
                // check up
                Vector3 draggedInAutoScroll = autoscroll.autoScrollUp.InverseTransformPoint(f_dragging.First().transform.position);
                if (Math.Abs(draggedInAutoScroll.x) < autoscroll.autoScrollUp.rect.width / 2 && 0 < draggedInAutoScroll.y && draggedInAutoScroll.y < autoscroll.autoScrollUp.rect.height)
                    setVerticalSpeed(autoscroll, 0.01f);
                // check down
                draggedInAutoScroll = autoscroll.autoScrollDown.InverseTransformPoint(f_dragging.First().transform.position);
                if (Math.Abs(draggedInAutoScroll.x) < autoscroll.autoScrollDown.rect.width / 2 && 0 < draggedInAutoScroll.y && draggedInAutoScroll.y < autoscroll.autoScrollDown.rect.height)
                    setVerticalSpeed(autoscroll, -0.01f);
                // check left
                draggedInAutoScroll = autoscroll.autoScrollLeft.InverseTransformPoint(f_dragging.First().transform.position);
                if (0 < draggedInAutoScroll.x && draggedInAutoScroll.x < autoscroll.autoScrollLeft.rect.width && Math.Abs(draggedInAutoScroll.y) < autoscroll.autoScrollLeft.rect.height / 2)
                    setHorizontalSpeed(autoscroll, -0.01f);
                // check right
                draggedInAutoScroll = autoscroll.autoScrollRight.InverseTransformPoint(f_dragging.First().transform.position);
                if (0 < draggedInAutoScroll.x && draggedInAutoScroll.x < autoscroll.autoScrollRight.rect.width && Math.Abs(draggedInAutoScroll.y) < autoscroll.autoScrollRight.rect.height / 2)
                    setHorizontalSpeed(autoscroll, 0.01f);
            }

            if (f_dragging.Count == 0 && !RectTransformUtility.RectangleContainsScreenPoint(scrollRect.transform as RectTransform, Pointer.current.position.value))
            {
                setVerticalSpeed(autoscroll, 0);
                setHorizontalSpeed(autoscroll, 0);
            }
        }

        // ---- Auto scroll on item selection ----
        // Get the currently selected UI element from the event system.
        GameObject selected = eventSystem.currentSelectedGameObject;
        // Do nothing if there are none OR if the selected game object is not a child of a scroll rect OR if the selected game object is the same as it was last frame,
        // meaning we haven't to move.
        if (selected != null && selected.GetComponentInParent<ScrollRect>() != null && selected != lastSelected)
            focusViewOn(selected);

        lastSelected = selected;
        // ---- end ----
    }

    private void focusViewOn(GameObject target)
    {
        if (!target.activeInHierarchy)
            return;
        // Get the content
        ScrollRect scrollRect = target.GetComponentInParent<ScrollRect>();
        RectTransform viewport = scrollRect.viewport;
        RectTransform contentPanel = scrollRect.content;

        float targetInContent_Y = Mathf.Abs(contentPanel.InverseTransformPoint(target.transform.position).y);
        float targetInContent_X = Mathf.Abs(contentPanel.InverseTransformPoint(target.transform.position).x);

        Vector2 targetAnchoredPosition = new Vector2(contentPanel.anchoredPosition.x, contentPanel.anchoredPosition.y);
        // we auto focus vertically on target object only if vertical scroll is enabled and it is not visible
        if (scrollRect.vertical && (targetInContent_Y - contentPanel.anchoredPosition.y < 0 || (targetInContent_Y + (target.transform as RectTransform).rect.height) - contentPanel.anchoredPosition.y > viewport.rect.height))
        {
            // check if selected object is too high
            if (targetInContent_Y - contentPanel.anchoredPosition.y < 0)
            {
                targetAnchoredPosition = new Vector2(
                    targetAnchoredPosition.x,
                    targetInContent_Y - (target.transform as RectTransform).rect.height
                );
            }
            // selected object is too low
            else
            {
                targetAnchoredPosition = new Vector2(
                    targetAnchoredPosition.x,
                    targetInContent_Y + (target.transform as RectTransform).rect.height * 1.5f - viewport.rect.height
                );
            }

            contentPanel.anchoredPosition = targetAnchoredPosition;
        }

        // we auto focus horizontally on selected object only if horizontal scroll is enabled and it is not visible
        // WARNING : on horizontal scroll contentPanel.anchoredPosition.x is negative
        if (scrollRect.horizontal && (targetInContent_X + contentPanel.anchoredPosition.x < 0 || (targetInContent_X + (target.transform as RectTransform).rect.width) + contentPanel.anchoredPosition.x > viewport.rect.width))
        {
            // check if selected object is too high
            if (targetInContent_X + contentPanel.anchoredPosition.x < 0)
            {
                targetAnchoredPosition = new Vector2(
                    -(targetInContent_X - (target.transform as RectTransform).rect.width),
                    targetAnchoredPosition.y
                );
            }
            // selected object is too low
            else
            {
                targetAnchoredPosition = new Vector2(
                    -(targetInContent_X + (target.transform as RectTransform).rect.width - viewport.rect.width),
                    targetAnchoredPosition.y
                );
            }
            contentPanel.anchoredPosition = targetAnchoredPosition;
        }
        if (target.GetComponent<ScrollOn>())
            GameObjectManager.removeComponent<ScrollOn>(target);
    }

    public void setVerticalSpeed(AutoScroll autoScroll, float newSpeed)
    {
        autoScroll.verticalSpeed = newSpeed;
    }

    public void setHorizontalSpeed(AutoScroll autoScroll, float newSpeed)
    {
        autoScroll.horizontalSpeed = newSpeed;
    }
}
