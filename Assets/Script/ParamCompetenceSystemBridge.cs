using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParamCompetenceSystemBridge : MonoBehaviour
{
    public void infoCompetence()
    {
        ParamCompetenceSystem.instance.infoCompetence(GetComponent<Competency>());
    }

    public void showLevelInfo()
    {
        TryGetComponent<DataLevelBehaviour>(out DataLevelBehaviour overridedData);
        ParamCompetenceSystem.instance.showLevelInfo(GetComponentInChildren<TMP_Text>().text, overridedData);
    }

    public void showBriefingOverride()
    {
        ParamCompetenceSystem.instance.showBriefingOverride();
    }

    public void setNextFocusedGameObject(GameObject go)
    {
        EventSystem.current.SetSelectedGameObject(go);
    }
    public void onScenarioSelected()
    {
        ParamCompetenceSystem.instance.onScenarioSelected(gameObject);
    }

    public void loadScenario()
    {
        gameObject.transform.parent.parent.parent.parent.Find("Buttons/LoadButton").GetComponent<Button>().onClick.Invoke();
    }
}
