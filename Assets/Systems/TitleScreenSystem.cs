using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using System.Linq;
public class TitleScreenSystem : FSystem {
	private GameData gameData;
	private GameObject campagneMenu;
	private GameObject campagneButton;
	private GameObject quitButton;


	public TitleScreenSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.levelList = new List<string>();
		campagneMenu = GameObject.Find("CampagneMenu");
		campagneButton = GameObject.Find("Campagne");
		quitButton = GameObject.Find("Quitter");
		GameObjectManager.dontDestroyOnLoadAndRebind(GameObject.Find("GameData"));

		GameObject cList = GameObject.Find("CampagneList");

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
		gameData.levelList = new List<string>(Directory.GetFiles(@"Assets\Levels\Campagne","*.xml"));
		
		//order by number of level
		gameData.levelList = gameData.levelList.OrderBy(levelName => int.Parse(Regex.Match(levelName, @"\d+").Value)).ToList();

		for(int i = 0; i < gameData.levelList.Count; i++){
			GameObject button = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Button") as GameObject, cList.transform);
			string[] texts = gameData.levelList[i].Split('\\');
			button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = texts[texts.Length-1].Split('.')[0];
			int indice = i;
			button.GetComponent<Button>().onClick.AddListener(delegate{launchLevel(indice);});
		}

		cList.transform.GetChild(0).SetSiblingIndex(cList.transform.childCount-1);

	}
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if(Input.GetButtonDown("Cancel")){
			Application.Quit();
		}
	}

	public void showCampagneMenu(){
		GameObjectManager.setGameObjectState(campagneMenu, true);
		GameObjectManager.setGameObjectState(campagneButton, false);
		GameObjectManager.setGameObjectState(quitButton, false);
	}

	public void launchLevel(int level){
		gameData.levelToLoad = level;
		GameObjectManager.loadScene("MainScene");
	}

	public void backFromCampagneMenu(){
		GameObjectManager.setGameObjectState(campagneMenu, false);
		GameObjectManager.setGameObjectState(campagneButton, true);
		GameObjectManager.setGameObjectState(quitButton, true);
	}

	public void quitGame(){
		Application.Quit();
	}
}