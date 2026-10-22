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
            menuCanvas.SetActive(false);
        Pause = true;
    }

    public void ShowMainMenu()
    {
        menuCanvas.SetActive(true);
        SetCanvasInterractable(false);
    }

    public void HideMainMenu()
    {
        menuCanvas.SetActive(false);
        SetCanvasInterractable(true);
    }

    public void SetCanvasInterractable(bool state)
    {
        foreach (CanvasGroup g in canvasGroups)
            g.interactable = state;
    }
}