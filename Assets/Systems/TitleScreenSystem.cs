using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using System.Linq;
using System.Xml;
public class TitleScreenSystem : FSystem {
	private GameData gameData;
	private GameObject campagneMenu;
	private GameObject playButton;
	private GameObject quitButton;
	private GameObject levelName;
	private GameObject cList;
	//private List<GameObject> levelDirectories;
	private Dictionary<GameObject, List<GameObject>> levelButtons; //key = directory button,  value = list of level buttons
	//private Dictionary<string, GameObject> levelButtonRefs;

	public TitleScreenSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.levelList = new Dictionary<string, List<string>>();
		campagneMenu = GameObject.Find("CampagneMenu");
		playButton = GameObject.Find("Jouer");
		quitButton = GameObject.Find("Quitter");
		GameObjectManager.dontDestroyOnLoadAndRebind(GameObject.Find("GameData"));

		cList = GameObject.Find("CampagneList");
		levelButtons = new Dictionary<GameObject, List<GameObject>>();

		/*GameObject button = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
		button.transform.GetChild(0).GetComponent<Text>().text = "Level 1";
		button.GetComponent<Button>().onClick.AddListener(delegate{launchLevel(0);});

		button = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
		button.transform.GetChild(0).GetComponent<Text>().text = "Level 2";
		button.GetComponent<Button>().onClick.AddListener(delegate{launchLevel(1);});

		button = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
		button.transform.GetChild(0).GetComponent<Text>().text = "Level 3";
		button.GetComponent<Button>().onClick.AddListener(delegate{launchLevel(2);});

		button = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
		button.transform.GetChild(0).GetComponent<Text>().text = "Level 4";
		button.GetComponent<Button>().onClick.AddListener(delegate{launchLevel(3);});

		button = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
		button.transform.GetChild(0).GetComponent<Text>().text = "Level 5";
		button.GetComponent<Button>().onClick.AddListener(delegate{launchLevel(4);});*/
		GameObjectManager.setGameObjectState(campagneMenu, false);
		string levelsPath = Application.streamingAssetsPath+Path.DirectorySeparatorChar+"Levels";
		List<string> levels;
		foreach(string directory in Directory.GetDirectories(levelsPath)){
			levels = readScenario(directory);
			if(levels != null){
				gameData.levelList[Path.GetFileName(directory)] = levels; //key = directory name
				Debug.Log("key = "+Path.GetFileName(directory));
				//PlayerPrefs initialization
				/*
				if(!PlayerPrefs.HasKey(Path.GetFileName(directory))){
					Debug.Log("not key = "+Path.GetFileName(directory)+" count = "+Path.GetFileName(directory).Length);
					PlayerPrefs.SetInt(Path.GetFileName(directory), 0);
					PlayerPrefs.Save();
				}*/
			}
				
		}
		
		//create level directory buttons
		foreach(string key in gameData.levelList.Keys){
			GameObject directoryButton = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = key;
			levelButtons[directoryButton] = new List<GameObject>();
			GameObjectManager.bind(directoryButton);
			//add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate{showLevels(directoryButton);});
			//create level buttons
			for(int i = 0; i < gameData.levelList[key].Count; i++){
				GameObject button = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
				//string[] texts = gameData.levelList[i].Split(Path.DirectorySeparatorChar);
				//button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = texts[texts.Length-1].Split('.')[0];
				button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(gameData.levelList[key][i]);
				//save ref to level, key = path from Levels directory, value = gameobject button ref 
				//levelButtonRefs["Campagne"+Path.DirectorySeparatorChar+gameData.levelList[i]] = button;
				int indice = i;
				button.GetComponent<Button>().onClick.AddListener(delegate{launchLevel(key, indice);});
				levelButtons[directoryButton].Add(button);
				GameObjectManager.bind(button);
				GameObjectManager.setGameObjectState(button, false);
			}
		}

		cList.transform.GetChild(0).SetSiblingIndex(cList.transform.childCount-1); //back button		

	}

	private List<string> readScenario(string repositoryPath){
		if(File.Exists(repositoryPath+Path.DirectorySeparatorChar+"Scenario.xml")){
			List<string> levelList = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.Load(repositoryPath+Path.DirectorySeparatorChar+"Scenario.xml");
			XmlNode root = doc.ChildNodes[1]; //root = <scenario/>
			foreach(XmlNode child in root.ChildNodes){
				if (child.Name.Equals("level")){
					levelList.Add(repositoryPath + Path.DirectorySeparatorChar + (child.Attributes.GetNamedItem("name").Value));
				}
			}
			return levelList;			
		}
		return null;
	}

	protected override void onProcess(int familiesUpdateCount) {
		if(Input.GetButtonDown("Cancel")){
			Application.Quit();
		}
	}

	public void showCampagneMenu(){
		GameObjectManager.setGameObjectState(campagneMenu, true);
		foreach(GameObject directory in levelButtons.Keys){
			//show directory buttons
			GameObjectManager.setGameObjectState(directory, true);
			//hide level buttons
			foreach(GameObject level in levelButtons[directory]){
				GameObjectManager.setGameObjectState(level, false);
			}
		}
		GameObjectManager.setGameObjectState(playButton, false);
		GameObjectManager.setGameObjectState(quitButton, false);
	}

	private void showLevels(GameObject levelDirectory){
		//show/hide levels
		foreach(GameObject directory in levelButtons.Keys){
			//hide level directories
			GameObjectManager.setGameObjectState(directory, false);
			//show levels
			if(directory.Equals(levelDirectory)){
				//foreach(GameObject go in levelButtons[directory]){
				for(int i = 0 ; i < levelButtons[directory].Count ; i ++){
					GameObjectManager.setGameObjectState(levelButtons[directory][i], true);
					//lock levels
					string directoryName = levelDirectory.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
					if(i > PlayerPrefs.GetInt(directoryName, 0))
						levelButtons[directory][i].GetComponent<Button>().interactable = false;
					else
						levelButtons[directory][i].GetComponent<Button>().interactable = true;
				}
			}
			//hide other levels
			else{
				foreach(GameObject go in levelButtons[directory]){
					GameObjectManager.setGameObjectState(go, false);
				}
			}
		}
	}

	public void launchLevel(string levelDirectory, int level){
		gameData.levelToLoad = (levelDirectory,level);
		GameObjectManager.loadScene("MainScene");
	}

	public void backFromCampagneMenu(){
		foreach(GameObject directory in levelButtons.Keys){
			if(directory.activeSelf){
				//main menu
				GameObjectManager.setGameObjectState(campagneMenu, false);
				GameObjectManager.setGameObjectState(playButton, true);
				GameObjectManager.setGameObjectState(quitButton, true);
				break;
			}
			else{
				//show directory buttons
				GameObjectManager.setGameObjectState(directory, true);
				//hide level buttons
				foreach(GameObject go in levelButtons[directory]){
					GameObjectManager.setGameObjectState(go, false);
				}
			}
		}

	}

	public void quitGame(){
		Application.Quit();
	}
}