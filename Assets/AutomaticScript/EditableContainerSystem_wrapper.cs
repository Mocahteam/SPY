using UnityEngine;
using FYFY;

public class EditableContainerSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject agentSelected;
	public UnityEngine.GameObject EditableCanvas;
	public UnityEngine.GameObject prefabViewportScriptContainer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "agentSelected", agentSelected);
		MainLoop.initAppropriateSystemField (system, "EditableCanvas", EditableCanvas);
		MainLoop.initAppropriateSystemField (system, "prefabViewportScriptContainer", prefabViewportScriptContainer);
	}

	public void selectContainer(UIRootContainer container)
	{
		MainLoop.callAppropriateSystemMethod (system, "selectContainer", container);
	}

	public void addContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "addContainer", null);
	}

	public void setContainerName()
	{
		MainLoop.callAppropriateSystemMethod (system, "setContainerName", null);
	}

	public void addSpecificContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "addSpecificContainer", null);
	}

	public void resetScriptContainer(System.Boolean refund)
	{
		MainLoop.callAppropriateSystemMethod (system, "resetScriptContainer", refund);
	}

	public void removeContainer(UnityEngine.GameObject container)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeContainer", container);
	}

	public void newNameContainer(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "newNameContainer", newName);
	}

	public void setAgentName(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "setAgentName", newName);
	}

}
