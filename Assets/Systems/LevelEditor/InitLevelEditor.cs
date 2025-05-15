using FYFY;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InitLevelEditor : FSystem
{
	public GameObject menuCanvas;
	public Button mapTab;
	public Button scriptTab;
	public Button paramTab;
	public GameObject mapContent;
	public GameObject scriptContent;
	public GameObject paramContent;
	public GameObject initFocused;

	private UnityAction localCallback;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern void HideHtmlButtons(); // call javascript

	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.loadScene("TitleScreen");
		else
		{
			GameData gameData = gameDataGO.GetComponent<GameData>();
			
			// config default UI
			GameObjectManager.setGameObjectState(menuCanvas, false);
			mapTab.interactable = false;
			scriptTab.interactable = true;
			paramTab.interactable = true;
			mapContent.SetActive(true);
			EventSystem.current.SetSelectedGameObject(initFocused);
			GameObjectManager.refresh(mapContent);
			GameObjectManager.setGameObjectState(scriptContent, false);
			GameObjectManager.setGameObjectState(paramContent, false);

			if (Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser())
			{
				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = gameData.localization[9], OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			}
			if (Application.platform == RuntimePlatform.WebGLPlayer)
				HideHtmlButtons();

			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "opened",
				objectType = "levelEditor"
			});

			// reload edited level
			if (gameData.selectedScenario == Utility.testFromLevelEditor)
			{
				gameData.selectedScenario = "";
				GameObjectManager.addComponent<NewLevelToLoad>(gameData.gameObject, new { levelKey = Utility.testFromLevelEditor });
			}
		}
	}

}
