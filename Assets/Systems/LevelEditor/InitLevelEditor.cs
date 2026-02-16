using FYFY;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InitLevelEditor : FSystem
{
	public Button mapTab;
	public Button scriptTab;
	public Button paramTab;
	public GameObject mapContent;
	public GameObject scriptContent;
	public GameObject paramContent;

	private GameData gameData;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern void HideHtmlButtons(); // call javascript

	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.loadScene("ConnexionScene");
		else
		{
			gameData = gameDataGO.GetComponent<GameData>();
			
			// config default UI
			mapTab.interactable = false;
			scriptTab.interactable = true;
			paramTab.interactable = true;
			mapContent.SetActive(true);
			GameObjectManager.refresh(mapContent);
			GameObjectManager.setGameObjectState(scriptContent, false);
			GameObjectManager.setGameObjectState(paramContent, false);

			if (Application.platform == RuntimePlatform.WebGLPlayer)
				HideHtmlButtons();

			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "opened",
				objectType = "levelEditor"
			});

			// reload edited level
			if (gameData.selectedScenario == UtilityLobby.testFromLevelEditor)
			{
				gameData.selectedScenario = "";
				GameObjectManager.addComponent<NewLevelToLoad>(gameData.gameObject, new { levelKey = UtilityLobby.testFromLevelEditor });
			}
		}
	}

	public void reloadEditor()
	{
		gameData.selectedScenario = "";
		GameObjectManager.loadScene("MissionEditor");
	}

	public void returnToLobby()
	{
		GameObjectManager.loadScene("TitleScreen");
	}
}
