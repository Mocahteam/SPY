using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ParamCompetenceSystemBridge : MonoBehaviour
{

    public void startLevel()
    {
        ParamCompetenceSystem.instance.startLevel();
    }

    public void infoCompetence(GameObject target)
    {
        ParamCompetenceSystem.instance.infoCompetence(target);
    }

    public void resetViewInfoCompetence(GameObject target)
    {
        ParamCompetenceSystem.instance.resetViewInfoCompetence(target);
    }

}
