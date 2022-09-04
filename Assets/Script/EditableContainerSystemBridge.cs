using UnityEngine;

public class EditableContainerSystemBridge : MonoBehaviour
{
    // Methode appellée pour changer le nom de l'agent
    public void setAgentName(string newName)
	{
		EditableContainerSystem.instance.setAgentName(newName);
	}
	public void resetScriptContainer()
	{
		EditableContainerSystem.instance.resetScriptContainer();
	}

	public void removeContainer()
	{
		EditableContainerSystem.instance.removeContainer(gameObject);
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
