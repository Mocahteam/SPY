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
        TryGetComponent<DataLevelBehaviour>(out DataLevelBehaviour overridedData);
        ParamCompetenceSystem.instance.showLevelInfo(GetComponentInChildren<TMP_Text>().text, overridedData);
    }

    public void removeItemFromParent()
    {
        ParamCompetenceSystem.instance.removeItemFromParent(gameObject);
    }

    public void moveItemInParent(int step)
    {
        ParamCompetenceSystem.instance.moveItemInParent(gameObject, step);
    }

    public void showBriefingOverride()
    {
        ParamCompetenceSystem.instance.showBriefingOverride(gameObject.GetComponent< DataLevelBehaviour>());
    }
}
