using FYFY;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class InitScenarioEditorManager : FSystem
{
	private GameData gameData;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "ConnexionScene" });
		else
		{
			gameData = gameDataGO.GetComponent<GameData>();

			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "opened",
				objectType = "scenarioEditor"
			});
		}

		Pause = true;
	}

	public void reloadEditor()
    {
		gameData.selectedScenario = "";
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "ScenarioEditor" });
	}

	public void returnToLobby()
	{
		gameData.selectedScenario = "";
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "TitleScreen" });
	}
}
