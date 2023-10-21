using UnityEngine;
using FYFY;

public class OpenFileSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject loadingLevelContent;
	public LevelData levelData;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "loadingLevelContent", loadingLevelContent);
		MainLoop.initAppropriateSystemField (system, "levelData", levelData);
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
