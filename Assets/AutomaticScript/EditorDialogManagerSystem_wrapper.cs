using UnityEngine;
using FYFY;

public class EditorDialogManagerSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject dialogEditPanel;
	public UnityEngine.GameObject dialogEditPopup;
	public UnityEngine.GameObject viewPortContent;
	public UnityEngine.GameObject listEntryPrefab;
	public UnityEngine.GameObject selected;
	public UnityEngine.UI.Toggle moveCameraToggle;
	public UnityEngine.UI.InputField dialogTextField;
	public System.Collections.Generic.List<UnityEngine.UI.InputField> cameraFields;
	public PaintableGrid paintableGrid;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "dialogEditPanel", dialogEditPanel);
		MainLoop.initAppropriateSystemField (system, "dialogEditPopup", dialogEditPopup);
		MainLoop.initAppropriateSystemField (system, "viewPortContent", viewPortContent);
		MainLoop.initAppropriateSystemField (system, "listEntryPrefab", listEntryPrefab);
		MainLoop.initAppropriateSystemField (system, "selected", selected);
		MainLoop.initAppropriateSystemField (system, "moveCameraToggle", moveCameraToggle);
		MainLoop.initAppropriateSystemField (system, "dialogTextField", dialogTextField);
		MainLoop.initAppropriateSystemField (system, "cameraFields", cameraFields);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
	}

	public void showPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "showPanel", null);
	}

	public void setSelection(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "setSelection", go);
	}

	public void newButtonPressed()
	{
		MainLoop.callAppropriateSystemMethod (system, "newButtonPressed", null);
	}

	public void editButtonPressed()
	{
		MainLoop.callAppropriateSystemMethod (system, "editButtonPressed", null);
	}

	public void deleteButtonPressed()
	{
		MainLoop.callAppropriateSystemMethod (system, "deleteButtonPressed", null);
	}

	public void cancelButtonPressed()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelButtonPressed", null);
	}

	public void confirmButtonPressed()
	{
		MainLoop.callAppropriateSystemMethod (system, "confirmButtonPressed", null);
	}

	public void checkCoordRange(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "checkCoordRange", go);
	}

	public void dialogToggleChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "dialogToggleChanged", null);
	}

}
