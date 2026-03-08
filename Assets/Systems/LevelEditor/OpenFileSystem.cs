using System;
using System.Collections.Generic;
using FYFY;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

public class OpenFileSystem : FSystem {

	public static OpenFileSystem instance;

	private Family f_newLoading = FamilyManager.getFamily(new AllOfComponents(typeof(NewLevelToLoad)));

	public GameObject loadingLevelContent;
	public DataLevelBehaviour dataLevel;
	private GameObject selectedLevelGO;

	public Button closeBriefing;
	public Button mapEditorTab;

	private GameData gameData;

	public OpenFileSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();

			refreshListOfLevels("");
		}

		f_newLoading.addEntryCallback(levelLoaded);

		Pause = true;
	}

	// see Load menu in MenuPanel and filter field in loading panel
	public void refreshListOfLevels(string filter)
	{
		// sync filter with the content of the input field (in the case of input field is not empty and loading panel is called from the menu)
		TMP_InputField input = loadingLevelContent.transform.parent.parent.parent.GetComponentInChildren<TMP_InputField>();
		if (input.text != filter)
		{
			input.text = filter;
			return;
		}

		// remove all old buttons
		for (int i = loadingLevelContent.transform.childCount - 1; i >= 0; i--)
        {
			Transform child = loadingLevelContent.transform.GetChild(i);
			GameObjectManager.unbind(child.gameObject);
			child.SetParent(null); // because destroying is not immediate
			GameObject.Destroy(child.gameObject);
		}

		//create levels buttons
		List<string> buttonsName = new List<string>();
		foreach (string key in gameData.levels.Keys)
			if (key != UtilityLobby.testFromLevelEditor && Utility.extractFileName(key).ToLower().Contains(filter.ToLower())) // // we don't create a button for tested level
				buttonsName.Add(Utility.extractFileName(key));
		buttonsName.Sort();
		foreach (string key in buttonsName)
		{
			GameObject levelItem = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/LevelEditor/LevelAvailable") as GameObject, loadingLevelContent.transform);
			levelItem.GetComponentInChildren<TextMeshProUGUI>().text = key;
			GameObjectManager.bind(levelItem);
		}
		selectedLevelGO = null;

		UtilityEditor.buildLoadingPanelNavigation(loadingLevelContent.transform.parent.parent.parent);
	}

	public void onLevelSelected(GameObject go)
	{
		selectedLevelGO = go;
	}

	// See New GO in MenuCanvas
	public void resetFileData()
    {
		dataLevel.data.missionName = "";
		dataLevel.data.filePath = "";
		dataLevel.data.overridedDialogs = new List<Dialog>();
	}

	// See LoadButton GameObject
	public void loadLevel()
    {
		if (selectedLevelGO != null)
		{
			// reset UI
			if (closeBriefing.gameObject.activeInHierarchy)
				closeBriefing.onClick.Invoke();
			if (mapEditorTab.interactable)
				mapEditorTab.onClick.Invoke();

			dataLevel.data.missionName = selectedLevelGO.GetComponentInChildren<TMP_Text>().text;
			if (gameData.levels.ContainsKey(new Uri(Application.streamingAssetsPath + "/" + dataLevel.data.missionName).AbsoluteUri))
				dataLevel.data.filePath = new Uri(Application.streamingAssetsPath + "/" + dataLevel.data.missionName).AbsoluteUri;
			else if (gameData.levels.ContainsKey(new Uri(Application.persistentDataPath + "/" + dataLevel.data.missionName).AbsoluteUri))
				dataLevel.data.filePath = new Uri(Application.persistentDataPath + "/" + dataLevel.data.missionName).AbsoluteUri;
			else
				dataLevel.data.filePath = dataLevel.data.missionName;
			// reset dialog
			dataLevel.data.overridedDialogs = new List<Dialog>();
			GameObjectManager.addComponent<NewLevelToLoad>(gameData.gameObject, new { levelKey = dataLevel.data.filePath });
		}
	}

	private void levelLoaded(GameObject go)
	{
		GameObjectManager.removeComponent<NewLevelToLoad>(go);
	}
}