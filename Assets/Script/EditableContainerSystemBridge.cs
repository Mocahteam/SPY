using TMPro;
using UnityEngine;

public class EditableContainerSystemBridge : MonoBehaviour
{
	public void ResetScriptContainer(GameObject scriptContainer)
	{
		EditableContainerSystem.instance.ResetScriptContainer(scriptContainer);
	}

	public void RemoveContainer()
	{
		EditableContainerSystem.instance.RemoveContainer(gameObject, false);
	}

	public void NewNameContainer(string name)
	{
		EditableContainerSystem.instance.NewNameContainer(name);
	}

    public void EditRobotName(TMP_Text name)
	{
        EditableContainerSystem.instance.EditRobotName(name);
    }
}
