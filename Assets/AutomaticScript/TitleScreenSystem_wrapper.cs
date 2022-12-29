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
	public UnityEngine.GameObject loadingScenarioContent;
	public UnityEngine.GameObject scenarioContent;
	public UnityEngine.GameObject quitButton;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "campagneMenu", campagneMenu);
		MainLoop.initAppropriateSystemField (system, "compLevelButton", compLevelButton);
		MainLoop.initAppropriateSystemField (system, "listOfCampaigns", listOfCampaigns);
		MainLoop.initAppropriateSystemField (system, "listOfLevels", listOfLevels);
		MainLoop.initAppropriateSystemField (system, "loadingScenarioContent", loadingScenarioContent);
		MainLoop.initAppropriateSystemField (system, "scenarioContent", scenarioContent);
		MainLoop.initAppropriateSystemField (system, "quitButton", quitButton);
	}

	public void importScenario(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "importScenario", content);
	}

	public void updateScenarioList()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateScenarioList", null);
	}

	public void launchLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", null);
	}

	public void testLevel(TMPro.TMP_Text levelToLoad)
	{
		MainLoop.callAppropriateSystemMethod (system, "testLevel", levelToLoad);
	}

	public void askToLoadLevel(System.String levelToLoad)
	{
		MainLoop.callAppropriateSystemMethod (system, "askToLoadLevel", levelToLoad);
	}

	public void quitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitGame", null);
	}

	public void displayLoadingPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "displayLoadingPanel", null);
	}

	public void onScenarioSelected(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "onScenarioSelected", go);
	}

	public void loadScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "loadScenario", null);
	}

}
