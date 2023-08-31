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

		// remove all old scenario
		foreach (Transform child in loadingLevelContent.transform)
		{
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
		}

		//create levels buttons
		List<string> buttonsName = new List<string>();
		foreach (string key in gameData.levels.Keys)
			buttonsName.Add(key.Replace(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri, ""));
		buttonsName.Sort();
		foreach (string key in buttonsName)
        {
			GameObject levelItem = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/LevelEditor/LevelAvailable") as GameObject, loadingLevelContent.transform);
			levelItem.GetComponent<TextMeshProUGUI>().text = key;
			GameObjectManager.bind(levelItem);
		}
		selectedLevelGO = null;

		f_newLoading.addEntryCallback(levelLoaded);
	}

	public void onLevelSelected(GameObject go)
	{
		selectedLevelGO = go;
	}

	public void resetFileData()
    {
		levelData.levelName = "";
		levelData.filePath = "";
	}

	// See LoadButton GameObject
	public void loadLevel()
    {
		if (selectedLevelGO != null && gameData.levels.ContainsKey(new Uri(Application.streamingAssetsPath + "/" + selectedLevelGO.GetComponentInChildren<TMP_Text>().text).AbsoluteUri))
		{
			levelData.levelName = selectedLevelGO.GetComponentInChildren<TMP_Text>().text;
			levelData.filePath = new Uri(Application.streamingAssetsPath + "/" + levelData.levelName).AbsoluteUri;
			GameObjectManager.addComponent<NewLevelToLoad>(gameData.gameObject, new { levelKey = levelData.filePath });
		}
    }

	private void levelLoaded(GameObject go)
	{
		GameObjectManager.removeComponent<NewLevelToLoad>(go);
	}
}