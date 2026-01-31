using UnityEngine;

public class ScrollSystemBridge : MonoBehaviour
{
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
