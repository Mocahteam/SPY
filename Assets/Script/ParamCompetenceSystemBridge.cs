using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParamCompetenceSystemBridge : MonoBehaviour
{
    public void infoCompetence()
    {
        ParamCompetenceSystem.instance.infoCompetence(GetComponent<Competency>());
    }

    public void refreshUI()
    {
        ParamCompetenceSystem.instance.refreshUI((RectTransform)transform);
    }

    public void showLevelInfo()
    {
        ParamCompetenceSystem.instance.showLevelInfo(GetComponentInChildren<TMP_Text>().text);
    }

    public void removeLevelFromScenario()
    {
        ParamCompetenceSystem.instance.removeLevelFromScenario(gameObject);
    }

    public void moveLevelInScenario(int step)
    {
        ParamCompetenceSystem.instance.moveLevelInScenario(gameObject, step);
    }
}
