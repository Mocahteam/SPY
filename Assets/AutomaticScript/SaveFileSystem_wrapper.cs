using UnityEngine;
using FYFY;

public class SaveFileSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.InputField executionLimitField;
	public UnityEngine.GameObject editableContainer;
	public LevelData levelData;
	public PaintableGrid paintableGrid;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "executionLimitField", executionLimitField);
		MainLoop.initAppropriateSystemField (system, "editableContainer", editableContainer);
		MainLoop.initAppropriateSystemField (system, "levelData", levelData);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
	}

	public void SaveAndQuit()
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveAndQuit", null);
	}

	public void saveXmlFile()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveXmlFile", null);
	}

}
