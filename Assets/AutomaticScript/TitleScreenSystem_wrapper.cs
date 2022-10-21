using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public GameData prefabGameData;
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.GameObject campagneMenu;
	public UnityEngine.GameObject compLevelButton;
	public UnityEngine.GameObject listOfCampaigns;
	public UnityEngine.GameObject listOfLevels;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "campagneMenu", campagneMenu);
		MainLoop.initAppropriateSystemField (system, "compLevelButton", compLevelButton);
		MainLoop.initAppropriateSystemField (system, "listOfCampaigns", listOfCampaigns);
		MainLoop.initAppropriateSystemField (system, "listOfLevels", listOfLevels);
	}

	public void launchLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", null);
	}

	public void quitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitGame", null);
	}

}
