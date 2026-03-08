using UnityEngine;
using FYFY;

public class OpenFileSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject loadingLevelContent;
	public DataLevelBehaviour dataLevel;
	public UnityEngine.UI.Button closeBriefing;
	public UnityEngine.UI.Button mapEditorTab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "loadingLevelContent", loadingLevelContent);
		MainLoop.initAppropriateSystemField (system, "dataLevel", dataLevel);
		MainLoop.initAppropriateSystemField (system, "closeBriefing", closeBriefing);
		MainLoop.initAppropriateSystemField (system, "mapEditorTab", mapEditorTab);
	}

	public void refreshListOfLevels(System.String filter)
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshListOfLevels", filter);
	}

	public void onLevelSelected(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "onLevelSelected", go);
	}

	public void resetFileData()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetFileData", null);
	}

	public void loadLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "loadLevel", null);
	}

}
