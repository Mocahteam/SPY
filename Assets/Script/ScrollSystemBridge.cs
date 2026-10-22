using UnityEngine;

public class ScrollSystemBridge : MonoBehaviour
{
    public void SetVerticalSpeed(float newSpeed)
    {
        AutoScroll autoScroll = GetComponent<AutoScroll>();
        if (autoScroll != null)
            ScrollSystem.instance.SetVerticalSpeed(autoScroll, newSpeed);
    }

    public void SetHorizontalSpeed(float newSpeed)
    {
        AutoScroll autoScroll = GetComponent<AutoScroll>();
        if (autoScroll != null)
            ScrollSystem.instance.SetHorizontalSpeed(autoScroll, newSpeed);
    }
}
