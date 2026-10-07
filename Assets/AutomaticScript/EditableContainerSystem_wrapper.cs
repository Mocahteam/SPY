using UnityEngine;
using FYFY;

public class EditableContainerSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject EditableCanvas;
	public UnityEngine.GameObject prefabViewportScriptContainer;
	public UnityEngine.UI.Button addContainerButton;
	public System.Int32 maxWidth;
	public CurrentSettingsValues currentSettingsValues;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "EditableCanvas", EditableCanvas);
		MainLoop.initAppropriateSystemField (system, "prefabViewportScriptContainer", prefabViewportScriptContainer);
		MainLoop.initAppropriateSystemField (system, "addContainerButton", addContainerButton);
		MainLoop.initAppropriateSystemField (system, "maxWidth", maxWidth);
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
	}

	public void addContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "addContainer", null);
	}

	public void resetScriptContainer(UnityEngine.GameObject scriptContainer)
	{
		MainLoop.callAppropriateSystemMethod (system, "resetScriptContainer", scriptContainer);
	}

	public void editRobotName(TMPro.TMP_Text name)
	{
		MainLoop.callAppropriateSystemMethod (system, "editRobotName", name);
	}

	public void checkDoubleClick(UnityEngine.EventSystems.BaseEventData element)
	{
		MainLoop.callAppropriateSystemMethod (system, "checkDoubleClick", element);
	}

	public void newNameContainer(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "newNameContainer", newName);
	}

}
