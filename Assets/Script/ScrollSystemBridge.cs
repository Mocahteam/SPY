using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollSystemBridge : MonoBehaviour
{
    public void onScroll(BaseEventData ev)
    {
        ScrollSystem.instance.onScroll(gameObject, ev);
    }
}
