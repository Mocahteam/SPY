using UnityEngine;
using FYFY;

public class EditableContainerSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject agentSelected;
	public UnityEngine.GameObject EditableCanvas;
	public UnityEngine.GameObject prefabViewportScriptContainer;
	public UnityEngine.UI.Button addContainerButton;
	public System.Int32 maxWidth;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "agentSelected", agentSelected);
		MainLoop.initAppropriateSystemField (system, "EditableCanvas", EditableCanvas);
		MainLoop.initAppropriateSystemField (system, "prefabViewportScriptContainer", prefabViewportScriptContainer);
		MainLoop.initAppropriateSystemField (system, "addContainerButton", addContainerButton);
		MainLoop.initAppropriateSystemField (system, "maxWidth", maxWidth);
	}

	public void selectContainer(UIRootContainer container)
	{
		MainLoop.callAppropriateSystemMethod (system, "selectContainer", container);
	}

	public void addContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "addContainer", null);
	}

	public void resetScriptContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetScriptContainer", null);
	}

	public void removeContainer(UnityEngine.GameObject container)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeContainer", container);
	}

	public void newNameContainer(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "newNameContainer", newName);
	}

}
