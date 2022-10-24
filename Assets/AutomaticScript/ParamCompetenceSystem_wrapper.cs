using UnityEngine;
using FYFY;

public class ParamCompetenceSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject panelInfoComp;
	public UnityEngine.GameObject panelInfoUser;
	public UnityEngine.GameObject prefabComp;
	public UnityEngine.GameObject ContentCompMenu;
	public TMPro.TMP_Text messageForUser;
	public UnityEngine.GameObject compatibleLevelsPanel;
	public UnityEngine.GameObject levelCompatiblePrefab;
	public UnityEngine.GameObject contentListOfCompatibleLevel;
	public UnityEngine.GameObject contentInfoCompatibleLevel;
	public UnityEngine.GameObject deletableElement;
	public UnityEngine.GameObject contentScenario;
	public UnityEngine.UI.Button addToScenario;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "panelInfoComp", panelInfoComp);
		MainLoop.initAppropriateSystemField (system, "panelInfoUser", panelInfoUser);
		MainLoop.initAppropriateSystemField (system, "prefabComp", prefabComp);
		MainLoop.initAppropriateSystemField (system, "ContentCompMenu", ContentCompMenu);
		MainLoop.initAppropriateSystemField (system, "messageForUser", messageForUser);
		MainLoop.initAppropriateSystemField (system, "compatibleLevelsPanel", compatibleLevelsPanel);
		MainLoop.initAppropriateSystemField (system, "levelCompatiblePrefab", levelCompatiblePrefab);
		MainLoop.initAppropriateSystemField (system, "contentListOfCompatibleLevel", contentListOfCompatibleLevel);
		MainLoop.initAppropriateSystemField (system, "contentInfoCompatibleLevel", contentInfoCompatibleLevel);
		MainLoop.initAppropriateSystemField (system, "deletableElement", deletableElement);
		MainLoop.initAppropriateSystemField (system, "contentScenario", contentScenario);
		MainLoop.initAppropriateSystemField (system, "addToScenario", addToScenario);
	}

	public void openPanelSelectComp()
	{
		MainLoop.callAppropriateSystemMethod (system, "openPanelSelectComp", null);
	}

	public void cleanCompPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "cleanCompPanel", null);
	}

	public void showCompatibleLevels()
	{
		MainLoop.callAppropriateSystemMethod (system, "showCompatibleLevels", null);
	}

	public void showLevelInfo(System.String path)
	{
		MainLoop.callAppropriateSystemMethod (system, "showLevelInfo", path);
	}

	public void addCurrentLevelToScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "addCurrentLevelToScenario", null);
	}

	public void infoCompetence(Competency comp)
	{
		MainLoop.callAppropriateSystemMethod (system, "infoCompetence", comp);
	}

	public void removeLevelFromScenario(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeLevelFromScenario", go);
	}

	public void moveLevelInScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "moveLevelInScenario", null);
	}

	public void testLevel(TMPro.TMP_Text levelToLoad)
	{
		MainLoop.callAppropriateSystemMethod (system, "testLevel", levelToLoad);
	}

	public void displayMessageUser()
	{
		MainLoop.callAppropriateSystemMethod (system, "displayMessageUser", null);
	}

	public void refreshUI(UnityEngine.RectTransform competency)
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshUI", competency);
	}

	public void saveScenario(TMPro.TMP_InputField scenarioName)
	{
		MainLoop.callAppropriateSystemMethod (system, "saveScenario", scenarioName);
	}

}
