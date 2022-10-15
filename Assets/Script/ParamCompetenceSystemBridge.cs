using UnityEngine;
using UnityEngine.UI;

public class ParamCompetenceSystemBridge : MonoBehaviour
{
    public bool closePanelParamComp = false;

    public void infoCompetence()
    {
        ParamCompetenceSystem.instance.infoCompetence(GetComponent<Competency>());
    }

    public void refreshUI()
    {
        ParamCompetenceSystem.instance.refreshUI((RectTransform)transform);
    }
}
