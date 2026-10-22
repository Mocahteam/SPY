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

	public void AddContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "AddContainer", null);
	}

	public void ResetScriptContainer(UnityEngine.GameObject scriptContainer)
	{
		MainLoop.callAppropriateSystemMethod (system, "ResetScriptContainer", scriptContainer);
	}

	public void EditRobotName(TMPro.TMP_Text name)
	{
		MainLoop.callAppropriateSystemMethod (system, "EditRobotName", name);
	}

	public void NewNameContainer(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "NewNameContainer", newName);
	}

}
