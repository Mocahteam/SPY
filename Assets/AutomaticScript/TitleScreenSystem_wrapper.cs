using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject prefabGameData;
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.GameObject campagneMenu;
	public UnityEngine.GameObject compLevelButton;
	public UnityEngine.GameObject listOfCampaigns;
	public UnityEngine.GameObject listOfLevels;
	public UnityEngine.GameObject loadingScenarioContent;
	public UnityEngine.GameObject scenarioContent;
	public UnityEngine.GameObject quitButton;
	public UnityEngine.GameObject loadingScreen;
	public UnityEngine.GameObject sessionIdPanel;
	public UnityEngine.GameObject deletableElement;
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
		MainLoop.initAppropriateSystemField (system, "loadingScreen", loadingScreen);
		MainLoop.initAppropriateSystemField (system, "sessionIdPanel", sessionIdPanel);
		MainLoop.initAppropriateSystemField (system, "deletableElement", deletableElement);
	}

	public void initGBLXAPI()
	{
		MainLoop.callAppropriateSystemMethod (system, "initGBLXAPI", null);
	}

	public void resetProgression(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "resetProgression", go);
	}

	public void GetProgression(TMPro.TMP_InputField idSession)
	{
		MainLoop.callAppropriateSystemMethod (system, "GetProgression", idSession);
	}

	public void updateScenarioContent()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateScenarioContent", null);
	}

	public void importScenario(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "importScenario", content);
	}

	public void displayScenarioList()
	{
		MainLoop.callAppropriateSystemMethod (system, "displayScenarioList", null);
	}

	public void launchLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", null);
	}

	public void testLevel(DataLevelBehaviour dlb)
	{
		MainLoop.callAppropriateSystemMethod (system, "testLevel", dlb);
	}

	public void askToLoadLevel(System.String levelToLoad)
	{
		MainLoop.callAppropriateSystemMethod (system, "askToLoadLevel", levelToLoad);
	}

	public void enableSendStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "enableSendStatement", null);
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
