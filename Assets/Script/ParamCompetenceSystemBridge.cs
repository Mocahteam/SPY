using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParamCompetenceSystemBridge : MonoBehaviour
{
    public bool closePanelParamComp = false;

    public void startLevel()
    {
        ParamCompetenceSystem.instance.startLevel();
    }

    public void onScroll(BaseEventData ev)
    {
        ExecuteEvents.ExecuteHierarchy(transform.Find("Scroll View").gameObject, ev, ExecuteEvents.scrollHandler);
    }

    public void infoCompetence(GameObject target)
    {
        ParamCompetenceSystem.instance.infoCompetence(target);
    }

    public void resetViewInfoCompetence(GameObject target)
    {
        ParamCompetenceSystem.instance.resetViewInfoCompetence(target);
    }

    public void MAJLinkCompetence(GameObject target)
    {
        ParamCompetenceSystem.instance.saveListUser();
        // Si il n'est pas selectionner, il va le devenir donc on active se qu'il faut
        if (!target.GetComponent<Toggle>().isOn)
        {
            ParamCompetenceSystem.instance.selectComp(target, true);
        }
        else
        {
            ParamCompetenceSystem.instance.unselectComp(target, true);
        }
    }

    public void closeSelectCompetencePanel()
    {
        if (closePanelParamComp)
        {
            ParamCompetenceSystem.instance.closeSelectCompetencePanel();
            closePanelParamComp = false;
        }
    }

    public void changeSizeButtonCategory(GameObject button)
    {
        ParamCompetenceSystem.instance.changeSizeButtonCategory(button, 15.0f);
    }

}
