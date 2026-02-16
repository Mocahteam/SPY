using UnityEngine;
using FYFY;

public class SaveFileSystem_wrapper : BaseWrapper
{
	public TMPro.TMP_InputField saveName;
	public UnityEngine.UI.Toggle DragDrop;
	public UnityEngine.UI.Toggle Fog;
	public UnityEngine.UI.Toggle ExecutionLimit;
	public UnityEngine.UI.Toggle HideExits;
	public TMPro.TMP_InputField score2;
	public TMPro.TMP_InputField score3;
	public UnityEngine.GameObject editableContainer;
	public LevelData levelData;
	public PaintableGrid paintableGrid;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "saveName", saveName);
		MainLoop.initAppropriateSystemField (system, "DragDrop", DragDrop);
		MainLoop.initAppropriateSystemField (system, "Fog", Fog);
		MainLoop.initAppropriateSystemField (system, "ExecutionLimit", ExecutionLimit);
		MainLoop.initAppropriateSystemField (system, "HideExits", HideExits);
		MainLoop.initAppropriateSystemField (system, "score2", score2);
		MainLoop.initAppropriateSystemField (system, "score3", score3);
		MainLoop.initAppropriateSystemField (system, "editableContainer", editableContainer);
		MainLoop.initAppropriateSystemField (system, "levelData", levelData);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
	}

	public void testLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "testLevel", null);
	}

	public void displaySavingPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "displaySavingPanel", null);
	}

	public void saveXmlFile()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveXmlFile", null);
	}

}
