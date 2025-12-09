using UnityEngine;

public class TitleScreenSystemBridge : MonoBehaviour
{
    public void showLevels(GameKeys key)
    {
        TitleScreenSystem.instance.showLevels(key);
    }

    public void showDetails(GameKeys key)
    {
        TitleScreenSystem.instance.showDetails(key);
    }

    public void launchLevel(GameKeys key)
    {
        TitleScreenSystem.instance.launchLevel(key);
    }
}
