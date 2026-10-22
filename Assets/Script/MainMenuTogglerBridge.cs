using UnityEngine;

public class MainMenuTogglerBridge : MonoBehaviour
{
    public void SetCanvasInterractable(bool state)
    {
        MainMenuToggler.instance.SetCanvasInterractable(state);
    }

    public void ShowMainMenu()
    {
        MainMenuToggler.instance.ShowMainMenu();
    }
}
