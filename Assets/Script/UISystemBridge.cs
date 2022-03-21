using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISystemBridge : MonoBehaviour
{
    public void resetScriptContainer()
    {
        UISystem.instance.resetScript();
    }


	public void newNameContainer(string name)
    {
        UISystem.instance.newNameContainer(name);
    }


    public void horizontalName(string name)
    {
        UISystem.instance.horizontalName(name);
    }

    public void noChangeName(string name)
    {
        UISystem.instance.noChangeName(name);
    }

}
