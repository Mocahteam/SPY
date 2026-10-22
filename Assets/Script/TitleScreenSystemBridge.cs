using UnityEngine;

public class TitleScreenSystemBridge : MonoBehaviour
{
    public void ShowLevels(GameKeys key)
    {
        TitleScreenSystem.instance.ShowLevels(key);
    }

    public void ShowDetails(GameKeys key)
    {
        TitleScreenSystem.instance.ShowDetails(key);
    }

    public void LaunchLevel(GameKeys key)
    {
        TitleScreenSystem.instance.LaunchLevel(key);
    }

    public void SetDraggingState(bool newSate)
    {
        TitleScreenSystem.instance.SetDraggingState(newSate);
    }
}
