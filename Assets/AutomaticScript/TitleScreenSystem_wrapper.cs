using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public GameData prefabGameData;
	public UnityEngine.GameObject mainMenu;
	public UnityEngine.GameObject skinMenu;
	public UnityEngine.GameObject skins;
	public UnityEngine.GameObject campagneMenu;
	public UnityEngine.GameObject compLevelButton;
	public UnityEngine.GameObject cList;
	public UnityEngine.GameObject robotKyle;
	public System.String pathFileParamFunct;
	public System.String pathFileParamRequiermentLibrary;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "mainMenu", mainMenu);
		MainLoop.initAppropriateSystemField (system, "skinMenu", skinMenu);
		MainLoop.initAppropriateSystemField (system, "skins", skins);
		MainLoop.initAppropriateSystemField (system, "campagneMenu", campagneMenu);
		MainLoop.initAppropriateSystemField (system, "compLevelButton", compLevelButton);
		MainLoop.initAppropriateSystemField (system, "cList", cList);
		MainLoop.initAppropriateSystemField (system, "robotKyle", robotKyle);
		MainLoop.initAppropriateSystemField (system, "pathFileParamFunct", pathFileParamFunct);
		MainLoop.initAppropriateSystemField (system, "pathFileParamRequiermentLibrary", pathFileParamRequiermentLibrary);
	}

	public void showCampagneMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "showCampagneMenu", null);
	}

	public void launchLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", null);
	}

	public void backFromCampagneMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "backFromCampagneMenu", null);
	}

	public void quitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitGame", null);
	}

	public void showSkinMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "showSkinMenu", null);
	}

	public void backToMain()
	{
		MainLoop.callAppropriateSystemMethod (system, "backToMain", null);
	}

	public void LogName(System.Int32 skinNum)
	{
		MainLoop.callAppropriateSystemMethod (system, "LogName", skinNum);
	}

}
