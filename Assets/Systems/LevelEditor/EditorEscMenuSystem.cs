using UnityEngine;
using FYFY;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.Events;

public class EditorEscMenu : FSystem
{
	private Family f_activePopups = FamilyManager.getFamily(new AllOfComponents(typeof(Popup)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	public GameObject buttonMenu;
	public GameObject menuCanvas;

	public static EditorEscMenu instance;
	
	private UnityAction localCallback;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern void HideHtmlButtons(); // call javascript

	public EditorEscMenu()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.loadScene("TitleScreen");
		else
		{
			GameData gameData = gameDataGO.GetComponent<GameData>();
			GameObjectManager.setGameObjectState(menuCanvas, false);

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
		}
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		//Active/désactive le menu echap si on appuit sur echap et que le focus n'est pas sur un input field et qu'il n'y a pas de popup ouverte
		if (Input.GetKeyDown(KeyCode.Escape) && (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null) && f_activePopups.Count == 0)
			toggleMenu();
	}

	public void toggleMenu()
	{
		var newState = !menuCanvas.activeSelf;
		menuCanvas.SetActive(newState);

		// Si le menu est désactivé, mettre le focus sur le bouton du menu
		if (!newState)
			EventSystem.current.SetSelectedGameObject(buttonMenu);
		// Si le menu est activé, mettre le focus sur le premier bouton du panel du menu
		else
			EventSystem.current.SetSelectedGameObject(menuCanvas.GetComponentInChildren<Button>().gameObject);
	}

	// See Quit button in editor scene
	public void closeEditor()
	{
		GameObjectManager.loadScene("TitleScreen");
	}
}