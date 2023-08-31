using UnityEngine;

public class EditorEditableContainerSystemBridge : MonoBehaviour
{
	public void resetScriptContainer()
	{
		EditorEditableContainerSystem.instance.resetScriptContainer();
	}

	public void removeContainer()
	{
		EditorEditableContainerSystem.instance.removeContainer(gameObject);
	}

	public void newNameContainer(string name)
	{
		EditorEditableContainerSystem.instance.newNameContainer(name);
	}

	public void selectContainer(UIRootContainer container)
	{
		EditorEditableContainerSystem.instance.selectContainer(container);
	}
}