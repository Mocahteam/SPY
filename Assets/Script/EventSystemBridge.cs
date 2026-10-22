using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemBridge : MonoBehaviour
{
    public void SetNextFocusedGameObject(GameObject go)
    {
        EventSystem.current.SetSelectedGameObject(go);
    }
}
