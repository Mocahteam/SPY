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

    public GameObject fade;

    protected override void onStart()
    {
        MainLoop.instance.StartCoroutine(fadeOut());
        f_sceneLoader.addEntryCallback(delegate (GameObject go) {
            MainLoop.instance.StartCoroutine(fadeIn(go.GetComponent<AskToLoadScene>().sceneName));
        });
        Pause = true;
    }

    private IEnumerator fadeOut()
    {
        GameObjectManager.setGameObjectState(fade, true);
        Image logo = fade.transform.Find("SPYLogo").GetComponent<Image>();
        logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, 1);
        while (logo.color.a > 0)
        {
            yield return null;
            logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, logo.color.a-Time.deltaTime);
        }
        GameObjectManager.setGameObjectState(fade, false);
    }

    private IEnumerator fadeIn(string sceneName)
    {
        Image logo = fade.transform.Find("SPYLogo").GetComponent<Image>();
        logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, 1);
        GameObjectManager.setGameObjectState(fade, true);
        // Freeze all canvas groups
        foreach (GameObject canvas in f_canvasGroups)
            canvas.GetComponent<CanvasGroup>().interactable = false;
        yield return null;
        GameObjectManager.loadScene(sceneName);
    }
}