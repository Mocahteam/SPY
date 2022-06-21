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

    public void selectContainer(UIRootContainer container)
    {
        UISystem.instance.selectContainer(container);
    }

    public void removeContainer()
    {
        UISystem.instance.removeContainer(gameObject);
    }

}
