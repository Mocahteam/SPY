using UnityEngine;
using FYFY;

public class MainMenuToggler : FSystem
{
    public GameObject menuCanvas;
    public CanvasGroup[] canvasGroups;

    public static MainMenuToggler instance;

    public MainMenuToggler()
    {
        instance = this;
    }

    protected override void onStart()
    {
        if (menuCanvas != null)
            GameObjectManager.setGameObjectState(menuCanvas, false);
        Pause = true;
    }

    public void showMainMenu()
    {
        menuCanvas.SetActive(true);
        setCanvasInterractable(false);
    }

    public void hideMainMenu()
    {
        menuCanvas.SetActive(false);
        setCanvasInterractable(true);
    }

    public void setCanvasInterractable(bool state)
    {
        foreach (CanvasGroup g in canvasGroups)
            g.interactable = state;
    }
}