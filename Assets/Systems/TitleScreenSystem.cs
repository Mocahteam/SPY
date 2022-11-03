using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Xml;
using Object = UnityEngine.Object;

using UnityEngine.SceneManagement;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private GameData gameData;
	public GameData prefabGameData;
	public GameObject mainMenu;
	public GameObject skinMenu;
	public GameObject skins;
	public GameObject campagneMenu;
	public GameObject compLevelButton;
	public GameObject cList;

	public GameObject robotKyle;

	private List<Material> textures = new List<Material>();

	public string pathFileParamFunct = "/StreamingAssets/ParamCompFunc/FunctionConstraint.csv"; // Chemin d'acces pour la chargement des paramètres des functions
	public string pathFileParamRequiermentLibrary = "/StreamingAssets/ParamCompFunc/FunctionalityRequiermentLibrairy.xml"; // Chemin d'acces pour la chargement des paramètres des functions

	private Dictionary<GameObject, List<GameObject>> levelButtons; //key = directory button,  value = list of level buttons

	private bool skinActive = false;

	private int current_skin_index = 0;

	/*
		Ajout projet
	*/
	public string[] textures_available = null;

	Material texture = null;

	/*
		Fin ajout projet
	*/


	protected override void onStart()
	{
		/*
			Ajout Projet
		*/

		skinMenu = GameObject.Find("SkinMenu");
		skinMenu.SetActive(false);
		skins = GameObject.Find("skins");
		skins.SetActive(false);
		robotKyle = GameObject.Find("Robot2");

		// Initialisation des skins dispos
		this.textures_available = new string[]{
			"Robot_Color",
			"Robot_Color_skin1",
			"Robot_Color_skin2",
			"Robot_Color_skin3"
		};
		
		// Debug.Log("y : "+this.bifules[3].ToString());
		int value_index_skin = get_current_skin_index_from_file();
		Debug.Log(this.textures_available[0]);
		Debug.Log(this.textures_available);
		Debug.Log("value : "+value_index_skin.ToString());




		this.texture = (Material)Resources.Load(textures_available[value_index_skin]);
		robotKyle.GetComponent<Renderer>().material = this.texture;
		/*
			Fin Ajout projet
		*/

		//Material texture0 = (Material)Resources.Load("Models\Robot Kyle\Materials\Robot_Color.mat");
		//Material texture1 = (Material)Resources.Load("Models\Robot Kyle\Materials\Robot_Color_Skin1.mat");

		

		/*
		Material texture0 = (Material)Resources.Load("Robot_Color");
		Material texture1 = (Material)Resources.Load("Robot_Color_skin1");
		Material texture2 = (Material)Resources.Load("Robot_Color_skin2");

		Debug.Log(texture0);

		//Texture2D texture3 = (Texture2D)Resources.Load("Robot_Color_skin 3");

		// il faut ajouter 4 textures
		textures.Add(texture0);
		textures.Add(texture1);
		textures.Add(texture2);
		textures.Add(texture0);
		*/
		
		if (!GameObject.Find("GameData"))
		{
			gameData = UnityEngine.Object.Instantiate(prefabGameData);
			gameData.name = "GameData";
			GameObjectManager.dontDestroyOnLoadAndRebind(gameData.gameObject);
		}
		else
		{
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
		}

		gameData.levelList = new Dictionary<string, List<string>>();

		levelButtons = new Dictionary<GameObject, List<GameObject>>();

		GameObjectManager.setGameObjectState(campagneMenu, false);
		string levelsPath;
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			//paramFunction();
			gameData.levelList["Campagne infiltration"] = new List<string>();
			for (int i = 1; i <= 20; i++)
				gameData.levelList["Campagne infiltration"].Add(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			Path.DirectorySeparatorChar + "Campagne infiltration" + Path.DirectorySeparatorChar +"Niveau" + i + ".xml");
			// Hide Competence button
			GameObjectManager.setGameObjectState(compLevelButton, false);
			ParamCompetenceSystem.instance.Pause = true;
		}
		else
		{
			levelsPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels";
			List<string> levels;
			foreach (string directory in Directory.GetDirectories(levelsPath))
			{
				levels = readScenario(directory);
				if (levels != null)
					gameData.levelList[Path.GetFileName(directory)] = levels; //key = directory name
			}
		}

		//create level directory buttons
		foreach (string key in gameData.levelList.Keys)
		{
			// Campagne infiltration
			GameObject directoryButton = Object.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, cList.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = key;
			levelButtons[directoryButton] = new List<GameObject>();
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showLevels(directoryButton); });
			// create level buttons
			for (int i = 0; i < gameData.levelList[key].Count; i++)
			{
				GameObject button = Object.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, cList.transform);
				button.transform.Find("Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(gameData.levelList[key][i]);
				int delegateIndice = i; // need to use local variable instead all buttons launch the last
				button.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { launchLevel(key, delegateIndice); });
				levelButtons[directoryButton].Add(button);
				GameObjectManager.bind(button);
				GameObjectManager.setGameObjectState(button, false);
			}
		}
	}

	private List<string> readScenario(string repositoryPath) {
		if (File.Exists(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml")) {
			List<string> levelList = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.Load(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml");
			XmlNode root = doc.ChildNodes[1]; //root = <scenario/>
			foreach (XmlNode child in root.ChildNodes) {
				if (child.Name.Equals("level")) {
					levelList.Add(repositoryPath + Path.DirectorySeparatorChar + (child.Attributes.GetNamedItem("name").Value));
				}
			}
			return levelList;
		}
		return null;
	}

	protected override void onProcess(int familiesUpdateCount) {

		



		if (Input.GetButtonDown("Cancel")) {
			Application.Quit();
		}
	}

	// See Jouer button in editor
	public void showCampagneMenu() {
		GameObjectManager.setGameObjectState(campagneMenu, true);
		GameObjectManager.setGameObjectState(mainMenu, false);
		foreach (GameObject directory in levelButtons.Keys) {
			//show directory buttons
			GameObjectManager.setGameObjectState(directory, true);
			//hide level buttons
			foreach (GameObject level in levelButtons[directory]) {
				GameObjectManager.setGameObjectState(level, false);
			}
		}
	}

	private void showLevels(GameObject levelDirectory) {
		//show/hide levels
		foreach (GameObject directory in levelButtons.Keys) {
			//hide level directories
			GameObjectManager.setGameObjectState(directory, false);
			//show levels
			if (directory.Equals(levelDirectory)) {
				for (int i = 0; i < levelButtons[directory].Count; i++) {
					GameObjectManager.setGameObjectState(levelButtons[directory][i], true);

					string directoryName = levelDirectory.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
					//locked levels
					if (i <= PlayerPrefs.GetInt(directoryName, 0)) //by default first level of directory is the only unlocked level of directory
						levelButtons[directory][i].transform.Find("Button").GetComponent<Button>().interactable = true;
					//unlocked levels
					else 
						levelButtons[directory][i].transform.Find("Button").GetComponent<Button>().interactable = false;
					//scores
					int scoredStars = PlayerPrefs.GetInt(directoryName + Path.DirectorySeparatorChar + i + gameData.scoreKey, 0); //0 star by default
					Transform scoreCanvas = levelButtons[directory][i].transform.Find("ScoreCanvas");
					for (int nbStar = 0; nbStar < 4; nbStar++) {
						if (nbStar == scoredStars)
							GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
						else
							GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
					}
				}
			}
			//hide other levels
			else {
				foreach (GameObject go in levelButtons[directory]) {
					GameObjectManager.setGameObjectState(go, false);
				}
			}
		}
	}

	public void launchLevel(string levelDirectory, int level) {
		gameData.levelToLoad = (levelDirectory, level);
		GameObjectManager.loadScene("MainScene");
	}

	// See Retour button in editor
	public void backFromCampagneMenu() {
		foreach (GameObject directory in levelButtons.Keys) {
			if (directory.activeSelf) {
				//main menu
				GameObjectManager.setGameObjectState(mainMenu, true);
				GameObjectManager.setGameObjectState(campagneMenu, false);
				break;
			}
			else {
				//show directory buttons
				GameObjectManager.setGameObjectState(directory, true);
				//hide level buttons
				foreach (GameObject go in levelButtons[directory]) {
					GameObjectManager.setGameObjectState(go, false);
				}
			}
		}
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}

	/**
	* Elements added for project
	**/

	// See Jouer button in editor
	public void showSkinMenu() {
		mainMenu.SetActive(false);
		//SceneManager.LoadScene("SkinSelection", LoadSceneMode.Single);
		
		skins.SetActive(true);
		skinMenu.SetActive(true);
		skinActive = true;
	}

	public void backToMain() {
		Debug.Log("Click retour");
		if(this.skinActive){
			skins.SetActive(false);
			skinMenu.SetActive(false);
			this.skinActive = false;
		}else{
			campagneMenu.SetActive(false);
		}
		mainMenu.SetActive(true);
		// TODO : Fix competence button disapear ...
	}


	/*
		Ajout project function
	*/
	public void LogName(int skin_index){
		this.current_skin_index = skin_index;

		this.texture = (Material)Resources.Load(textures_available[skin_index]);

		robotKyle.GetComponent<Renderer>().material = this.texture;
		
		write_current_skin_index();
	}

	public void write_current_skin_index()
	{
		string path = "Assets/Resources/current_skin_value.txt";
		if(File.Exists(path)){
			File.WriteAllText(path,string.Empty); // efface les dernieres valeurs enregistrée pour mettre la plus récente
		}
		StreamWriter writer = new StreamWriter(path, true);
		writer.WriteLine(this.current_skin_index.ToString());
        writer.Close();
		Debug.Log("Enregistrement fini du skin : "+this.current_skin_index.ToString());
		get_current_skin_index_from_file();
	}

	public int get_current_skin_index_from_file()
	{
		int skin_index = 0;
		string path = "Assets/Resources/current_skin_value.txt";
		if(File.Exists(path)){
			// si le fichier existe, on prend la derniere valeur enregistrée
			string[] lines = File.ReadAllLines(path);
			skin_index =  int.Parse(lines[0]);
		}
		// sinon on prend le skin par défaut
		return skin_index;
	}

}