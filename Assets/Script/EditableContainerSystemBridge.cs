using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditableContainerSystemBridge : MonoBehaviour
{
	public void resetScriptContainer(GameObject scriptContainer)
	{
		EditableContainerSystem.instance.resetScriptContainer(scriptContainer);
	}

	public void removeContainer()
	{
		EditableContainerSystem.instance.removeContainer(gameObject, false);
	}

	public void newNameContainer(string name)
	{
		EditableContainerSystem.instance.newNameContainer(name);
	}

    public void editRobotName(TMP_Text name)
	{
        EditableContainerSystem.instance.editRobotName(name);
    }
    public void checkDoubleClick(BaseEventData e)
    {
        EditableContainerSystem.instance.checkDoubleClick(e);
    }
}
