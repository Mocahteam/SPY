using System;
using System.Collections.Generic;
using FYFY;
using TMPro;
using UnityEngine;
using Application = UnityEngine.Application;

public class OpenFileSystem : FSystem {

	public static OpenFileSystem instance;

	private Family f_newLoading = FamilyManager.getFamily(new AllOfComponents(typeof(NewLevelToLoad)));

	public GameObject loadingLevelContent;
	public LevelData levelData;
	public TMP_InputField savingInputField;
	private GameObject selectedLevelGO;

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
		levelData.levelName = "";
		levelData.filePath = "";
	}

	// See LoadButton GameObject
	public void loadLevel()
    {
		if (selectedLevelGO != null)
        {
			levelData.levelName = selectedLevelGO.GetComponentInChildren<TMP_Text>().text;
			savingInputField.text = levelData.levelName;
			if (gameData.levels.ContainsKey(new Uri(Application.streamingAssetsPath + "/" + selectedLevelGO.GetComponentInChildren<TMP_Text>().text).AbsoluteUri))
				levelData.filePath = new Uri(Application.streamingAssetsPath + "/" + levelData.levelName).AbsoluteUri;
			else if (gameData.levels.ContainsKey(new Uri(Application.persistentDataPath + "/" + selectedLevelGO.GetComponentInChildren<TMP_Text>().text).AbsoluteUri))
				levelData.filePath = new Uri(Application.persistentDataPath + "/" + levelData.levelName).AbsoluteUri;
			else
				levelData.filePath = levelData.levelName;
			GameObjectManager.addComponent<NewLevelToLoad>(gameData.gameObject, new { levelKey = levelData.filePath });
		}
	}

	private void levelLoaded(GameObject go)
	{
		GameObjectManager.removeComponent<NewLevelToLoad>(go);
	}
}