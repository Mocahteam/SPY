using UnityEngine;
using UnityEngine.EventSystems;
using FYFY;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Fade transition between scene loading
/// </summary>
public class FadeSystem : FSystem {
    private Family f_sceneLoader = FamilyManager.getFamily(new AllOfComponents(typeof(AskToLoadScene)));
    private Family f_canvasGroups = FamilyManager.getFamily(new AllOfComponents(typeof(CanvasGroup)));
    private Family f_fadeOutEnd = FamilyManager.getFamily(new AllOfComponents(typeof(FadeOutEnd)));

    private Image logo;

    public GameObject fade;

    protected override void onStart()
    {
        logo = fade.transform.Find("SPYLogo").GetComponent<Image>();
        GameObjectManager.setGameObjectState(fade, true);
        logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, 1);
        MainLoop.instance.StartCoroutine(fadeOut());

        f_sceneLoader.addEntryCallback(delegate (GameObject go) {
            MainLoop.instance.StartCoroutine(fadeIn(go.GetComponent<AskToLoadScene>().sceneName));
        });
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        foreach (GameObject go in f_fadeOutEnd)
            GameObjectManager.removeComponent<FadeOutEnd>(go); // On supprime dans le onProcess pour laisser la possibilité aux autres système de capter l'évènement
    }

    private IEnumerator fadeOut()
    {
        while (logo.color.a > 0)
        {
            yield return null;
            logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, logo.color.a-Time.deltaTime);
        }
        GameObjectManager.addComponent<FadeOutEnd>(fade);
        yield return null;
        yield return null;
        GameObjectManager.setGameObjectState(fade, false);
    }

    private IEnumerator fadeIn(string sceneName)
    {
        logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, 1);
        GameObjectManager.setGameObjectState(fade, true);
        // Freeze all canvas groups
        foreach (GameObject canvas in f_canvasGroups)
            canvas.GetComponent<CanvasGroup>().interactable = false;
        yield return null;
        GameObjectManager.loadScene(sceneName);
    }
}