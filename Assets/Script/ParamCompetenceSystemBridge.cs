using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParamCompetenceSystemBridge : MonoBehaviour
{
    public void InfoCompetence()
    {
        ParamCompetenceSystem.instance.InfoCompetence(GetComponent<Competency>());
    }

    public void ShowLevelInfo()
    {
        TryGetComponent<DataLevelBehaviour>(out DataLevelBehaviour overridedData);
        ParamCompetenceSystem.instance.ShowLevelInfo(GetComponentInChildren<TMP_Text>().text, overridedData);
    }

    public void ShowBriefingOverride()
    {
        ParamCompetenceSystem.instance.ShowBriefingOverride();
    }

    public void SetNextFocusedGameObject(GameObject go)
    {
        EventSystem.current.SetSelectedGameObject(go);
    }
    public void OnScenarioSelected()
    {
        ParamCompetenceSystem.instance.OnScenarioSelected(gameObject);
    }

    public void LoadScenario()
    {
        gameObject.transform.parent.parent.parent.parent.Find("Buttons/LoadButton").GetComponent<Button>().onClick.Invoke();
    }
}
