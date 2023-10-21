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
			gameData = go.GetComponent<GameData>();

		refreshListOfLevels("");

		f_newLoading.addEntryCallback(levelLoaded);

		// reload edited level
		if (gameData.selectedScenario == Utility.testFromLevelEditor)
        {
			gameData.selectedScenario = "";
			GameObjectManager.addComponent<NewLevelToLoad>(gameData.gameObject, new { levelKey = Utility.testFromLevelEditor });
		}
	}

	// see Open menu and filter field in loading panel
	public void refreshListOfLevels(string filter)
	{
		// remove all old buttons
		for(int i = loadingLevelContent.transform.childCount - 1; i >= 0; i--)
        {
			Transform child = loadingLevelContent.transform.GetChild(i);
			GameObjectManager.unbind(child.gameObject);
			child.SetParent(null);
			GameObject.Destroy(child.gameObject);
		}

		//create levels buttons
		List<string> buttonsName = new List<string>();
		foreach (string key in gameData.levels.Keys)
			if (key != Utility.testFromLevelEditor && Utility.extractFileName(key).Contains(filter)) // // we don't create a button for tested level
				buttonsName.Add(Utility.extractFileName(key));
		buttonsName.Sort();
		foreach (string key in buttonsName)
		{
			GameObject levelItem = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/LevelEditor/LevelAvailable") as GameObject, loadingLevelContent.transform);
			levelItem.GetComponent<TextMeshProUGUI>().text = key;
			GameObjectManager.bind(levelItem);
		}
		selectedLevelGO = null;
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