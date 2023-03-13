using UnityEngine;

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

	public void selectContainer(UIRootContainer container)
	{
		EditableContainerSystem.instance.selectContainer(container);
	}
}
