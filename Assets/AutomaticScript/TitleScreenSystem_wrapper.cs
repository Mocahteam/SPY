using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public GameData prefabGameData;
	public UnityEngine.GameObject campagneMenu;
	public UnityEngine.GameObject playButton;
	public UnityEngine.GameObject quitButton;
	public UnityEngine.GameObject backButton;
	public UnityEngine.GameObject cList;
	public System.String pathFileParamFunct;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "campagneMenu", campagneMenu);
		MainLoop.initAppropriateSystemField (system, "playButton", playButton);
		MainLoop.initAppropriateSystemField (system, "quitButton", quitButton);
		MainLoop.initAppropriateSystemField (system, "backButton", backButton);
		MainLoop.initAppropriateSystemField (system, "cList", cList);
		MainLoop.initAppropriateSystemField (system, "pathFileParamFunct", pathFileParamFunct);
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

}
