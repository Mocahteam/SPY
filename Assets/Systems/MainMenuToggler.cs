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

    public void toggleMainMenu()
    {
        // si le menu n'est pas affiché, on l'affiche
        if (!menuCanvas.activeInHierarchy)
        {
            menuCanvas.SetActive(true);
            setCanvasInterractable(false);
        }
        // sinon faire l'inverse
        else
        {
            menuCanvas.SetActive(false);
            setCanvasInterractable(true);
        }
    }

    public void setCanvasInterractable(bool state)
    {
        foreach (CanvasGroup g in canvasGroups)
            g.interactable = state;
    }
}