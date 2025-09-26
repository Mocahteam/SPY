using FYFY;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InitScenarioEditorManager : FSystem
{
	public Button downloadLevelBt;
	public GameObject menuCanvas;

	public GameObject menuEscape;
	public GameObject closePanelButton;
	public CanvasGroup[] canvasGroups;

	private UnityAction localCallback;
	private GameData gameData;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.loadScene("TitleScreen");
		else
		{
			gameData = gameDataGO.GetComponent<GameData>();

			if (Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser())
			{
				localCallback = null;
				Localization loc = gameData.GetComponent<Localization>();
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = loc.localization[9], OkButton = loc.localization[0], CancelButton = loc.localization[1], call = localCallback });
			} 

			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "opened",
				objectType = "scenarioEditor"
			});

			if (Application.platform != RuntimePlatform.WebGLPlayer)
				GameObjectManager.setGameObjectState(downloadLevelBt.gameObject, false);
		}
	}

	public void reloadEditor()
    {
		gameData.selectedScenario = "";
		GameObjectManager.loadScene("ScenarioEditor");
	}

	public void returnToLobby()
	{
		GameObjectManager.loadScene("TitleScreen");
	}

	public void toggleMainMenu()
	{
		// si le menu n'est pas affiché, on l'affiche
		if (!menuCanvas.activeInHierarchy)
		{
			menuCanvas.SetActive(true);
			EventSystem.current.SetSelectedGameObject(closePanelButton);
			foreach (CanvasGroup g in canvasGroups)
				g.interactable = false;
		}
		// sinon faire l'inverse
		else
		{
			menuCanvas.SetActive(false);
			EventSystem.current.SetSelectedGameObject(menuEscape);
			foreach (CanvasGroup g in canvasGroups)
				g.interactable = true;
		}
	}
}
