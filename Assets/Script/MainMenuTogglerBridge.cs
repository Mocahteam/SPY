using UnityEngine;

public class MainMenuTogglerBridge : MonoBehaviour
{
    public void setCanvasInterractable(bool state)
    {
        MainMenuToggler.instance.setCanvasInterractable(state);
    }
}
