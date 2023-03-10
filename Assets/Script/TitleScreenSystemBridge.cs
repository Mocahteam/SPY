using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenSystemBridge : MonoBehaviour
{
    public void onScenarioSelected()
    {
        TitleScreenSystem.instance.onScenarioSelected(gameObject);
    }

    public void loadScenario()
    {
        gameObject.transform.parent.parent.parent.parent.Find("Buttons").Find("LoadButton").GetComponent<Button>().onClick.Invoke();
    }
}
