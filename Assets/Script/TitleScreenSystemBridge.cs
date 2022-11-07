using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenSystemBridge : MonoBehaviour
{
    public void onScenarioSelected()
    {
        TitleScreenSystem.instance.onScenarioSelected(gameObject);
    }
}
