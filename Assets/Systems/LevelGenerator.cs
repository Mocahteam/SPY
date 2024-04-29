using UnityEngine;
using FYFY;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// Read XML file and load level
/// Need to be binded before UISystem
/// </summary>
public class LevelGenerator : FSystem {

	public static LevelGenerator instance;

	// Famille contenant les agents editables
	private Family f_level = FamilyManager.getFamily(new AnyOfComponents(typeof(Position), typeof(CurrentAction)));
	private Family f_draggableElement = FamilyManager.getFamily(new AnyOfComponents(typeof(ElementToDrag)));

	public GameObject LevelGO;
	private List<List<int>> map;
	private GameData gameData;
	private int nbAgentCreate = 0; // Nombre d'agents créés
	private int nbDroneCreate = 0; // Nombre de drones créés
	private GameObject lastAgentCreated = null;

	public GameObject editableCanvas;// Le container qui contient les Viewport/script containers
	public GameObject scriptContainer;
	public GameObject library; // Le viewport qui contient la librairie
	public TMP_Text levelName;
	public GameObject buttonExecute;

	[DllImport("__Internal")]
	private static extern void HideHtmlButtons(); // call javascript

	public LevelGenerator()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.loadScene("TitleScreen");
		else
		{
			gameData = gameDataGO.GetComponent<GameData>();
			DataLevel levelToLoad = gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad];
			if (gameData.levels.ContainsKey(levelToLoad.src))
				XmlToLevel(gameData.levels[levelToLoad.src].OwnerDocument);
			else
				GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Error });
			levelName.text = Utility.extractLocale(levelToLoad.name);
			if (Application.platform == RuntimePlatform.WebGLPlayer)
				HideHtmlButtons();
            GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
			{
				verb = "launched",
				objectType = "level",
				activityExtensions = new Dictionary<string, string>() {
					{ "value", Utility.extractFileName(levelToLoad.src) },
					{ "context", gameData.selectedScenario },
					{ "progress", levelToLoad.name }
				}
			});
		}
	}

	// Read xml document and create all game objects
	public void XmlToLevel(XmlDocument doc)
	{
		gameData.totalActionBlocUsed = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
		// check if dialogs are defined in the scenario
		bool dialogsOverrided = true;
		DataLevel levelToLoad = gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad];
		if (levelToLoad.overridedDialogs == null)
		{
			dialogsOverrided = false;
			levelToLoad.overridedDialogs = new List<Dialog>();
		}
		gameData.actionBlockLimit = new Dictionary<string, int>();
		map = new List<List<int>>();

		// remove comments
		Utility.removeComments(doc);

		XmlNode root = doc.ChildNodes[1];

		// check if dragdropDisabled node exists and set gamedata accordingly
		gameData.dragDropEnabled = doc.GetElementsByTagName("dragdropDisabled").Count == 0;

		foreach (XmlNode child in root.ChildNodes)
		{
			switch (child.Name)
			{
				case "map":
					readXMLMap(child);
					break;
				case "dialogs":
					if (!dialogsOverrided)
						Utility.readXMLDialogs(child, levelToLoad.overridedDialogs);
					break;
				case "executionLimit":
					int amount = int.Parse(child.Attributes.GetNamedItem("amount").Value);
					if (amount > 0)
					{
						GameObject amountGO = buttonExecute.transform.GetChild(1).gameObject;
						GameObjectManager.setGameObjectState(amountGO, true);
						amountGO.GetComponentInChildren<TMP_Text>(true).text = "" + amount;
					}
					break;
				case "fog":
					// fog has to be enabled after agents
					MainLoop.instance.StartCoroutine(delayEnableFog());
					break;
				case "blockLimits":
					readXMLLimits(child);
					break;
				case "coin":
					createCoin(int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posY").Value));
					break;
				case "console":
					readXMLConsole(child);
					break;
				case "door":
					createDoor(int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posY").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value), int.Parse(child.Attributes.GetNamedItem("slotId").Value));
					break;
				case "robot":
				case "guard":
				case "player": // backward compatibility
				case "enemy": // backward compatibility
					string nameAgentByUser = "";
					XmlNode agentName = child.Attributes.GetNamedItem("inputLine"); 
					if (agentName == null)
						agentName = child.Attributes.GetNamedItem("associatedScriptName"); // for backward compatibility
					if (agentName != null && agentName.Value != "")
						nameAgentByUser = agentName.Value;
					GameObject agent = createEntity(Utility.extractLocale(nameAgentByUser), int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posY").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value), child.Name);
					if (child.Name == "enemy" || child.Name == "guard")
					{
						agent.GetComponent<DetectRange>().range = int.Parse(child.Attributes.GetNamedItem("range").Value);
						agent.GetComponent<DetectRange>().selfRange = bool.Parse(child.Attributes.GetNamedItem("selfRange").Value);
						agent.GetComponent<DetectRange>().type = (DetectRange.Type)int.Parse(child.Attributes.GetNamedItem("typeRange").Value);
					}
					else
						lastAgentCreated = agent;
					break;
				case "decoration":
					createDecoration(child.Attributes.GetNamedItem("name").Value, int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posY").Value), (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value));
					break;
				case "script":
					UIRootContainer.EditMode editModeByUser = UIRootContainer.EditMode.Locked;
					XmlNode editMode = child.Attributes.GetNamedItem("editMode");
					int tmpValue;
					if (editMode != null && int.TryParse(editMode.Value, out tmpValue))
						editModeByUser = (UIRootContainer.EditMode)tmpValue;
					UIRootContainer.SolutionType typeByUser = UIRootContainer.SolutionType.Undefined;
					XmlNode typeNode = child.Attributes.GetNamedItem("type");
					if (typeNode != null && int.TryParse(typeNode.Value, out tmpValue))
						typeByUser = (UIRootContainer.SolutionType)tmpValue;
					XmlNode name = child.Attributes.GetNamedItem("outputLine");
					if (name == null)
						name = child.Attributes.GetNamedItem("name"); // for retrocompatibility

					// Ask to create script
					/* We can't used :
					GameObjectManager.addComponent<ScriptToLoad>(MainLoop.instance.gameObject, new
					{
						scriptNode = child,
						scriptName = Utility.extractLocale(name.Value),
						editMode = editModeByUser,
						type = typeByUser
					});
					because we get exception "not IConvertible" when we try to get the component !!! something wrong with XmlNode attribute of ScriptToLoad component...
					Solution: we add ScriptToLoad component to the MainLoop immediately, we init each of its attributes and we ask to refresh the MainLoop to update families*/
					ScriptToLoad stl = MainLoop.instance.gameObject.AddComponent<ScriptToLoad>();
					stl.scriptNode = child;
					stl.scriptName = Utility.extractLocale(name.Value);
					stl.editMode = editModeByUser;
					stl.type = typeByUser;
					MainLoop.instance.StartCoroutine(delayRefreshMainLoop());
					break;
				case "score":
					gameData.levelToLoadScore = new int[2];
					gameData.levelToLoadScore[0] = int.Parse(child.Attributes.GetNamedItem("threeStars").Value);
					gameData.levelToLoadScore[1] = int.Parse(child.Attributes.GetNamedItem("twoStars").Value);
					break;
			}
		}
		eraseMap();
		generateMap(doc.GetElementsByTagName("hideExits").Count > 0);
		MainLoop.instance.StartCoroutine(delayGameLoaded());
	}

	private IEnumerator delayRefreshMainLoop()
	{
		yield return null;
		GameObjectManager.refresh(MainLoop.instance.gameObject);
	}

	private IEnumerator delayGameLoaded()
	{
		yield return null;
		yield return null;
		GameObjectManager.addComponent<GameLoaded>(MainLoop.instance.gameObject);
	}

	private IEnumerator delayEnableFog()
	{
		yield return null;
		if (lastAgentCreated != null)
		{
			Transform fog = lastAgentCreated.transform.Find("Fog");
			if (fog != null)
				GameObjectManager.setGameObjectState(fog.gameObject, true);
		}
	}

	// read the map and create wall, ground, spawn and exit
	private void generateMap(bool hideExits){
		for (int y = 0; y< map.Count; y++){
			for(int x = 0; x < map[y].Count; x++){
				switch (map[y][x]){
					case -1: // void
						createWall(x, y, false);
						break;
					case 0: // Path
						createCell(x,y);
						break;
					case 1: // Wall
						createCell(x,y);
						createWall(x,y, true);
						break;
					case 2: // Spawn
						createCell(x,y);
						createSpawnExit(x,y,true);
						break;
					case 3: // Exit
						createCell(x,y);
						createSpawnExit(x,y,false, hideExits);
						break;
				}
			}
		}
	}

	// Créer une entité agent ou robot et y associer un panel container
	private GameObject createEntity(string nameAgent, int gridX, int gridY, Direction.Dir direction, string type){
		GameObject entity = null;
		switch(type){
			case "robot":
			case "player": // backward compatibility
				entity = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/Robot Kyle") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,1.5f,gridX*3), Quaternion.Euler(0,0,0), LevelGO.transform);
				break;
			case "guard":
			case "enemy": // backward compatibility
				entity = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/Drone") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,3.8f,gridX*3), Quaternion.Euler(0,0,0), LevelGO.transform);
				break;
		}

		// Charger l'agent aux bonnes coordonées dans la bonne direction
		entity.GetComponent<Position>().x = gridX;
		entity.GetComponent<Position>().y = gridY;
		entity.GetComponent<Position>().targetX = -1;
		entity.GetComponent<Position>().targetY = -1;
		entity.GetComponent<Direction>().direction = direction;
		
		//add new container to entity
		ScriptRef scriptref = entity.GetComponent<ScriptRef>();
		GameObject executablePanel = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/ExecutablePanel") as GameObject, scriptContainer.gameObject.transform, false);
		// Associer à l'agent l'UI container
		scriptref.executablePanel = executablePanel;
		// Associer à l'agent le script container
		scriptref.executableScript = executablePanel.transform.Find("Scroll View").Find("Viewport").Find("ScriptContainer").gameObject;
		// Association de l'agent au script de gestion des fonctions
		executablePanel.GetComponentInChildren<LinkedWith>(true).target = entity;

		// On va charger l'image et le nom de l'agent selon l'agent (robot, ennemi etc...)
		if (type == "robot" || type == "player")
		{
			nbAgentCreate++;
			// On nomme l'agent
			AgentEdit agentEdit = entity.GetComponent<AgentEdit>();
			if (nameAgent != "")
				agentEdit.associatedScriptName = nameAgent;
			else
				agentEdit.associatedScriptName = "Agent" + nbAgentCreate;

			// Chargement de l'icône de l'agent sur la localisation
			executablePanel.transform.Find("Header").Find("locateButton").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/robotIcon", typeof(Sprite)) as Sprite;
			// Affichage du nom de l'agent
			executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = entity.GetComponent<AgentEdit>().associatedScriptName;
		}
		else if (type == "guard" || type == "enemy")
		{
			nbDroneCreate++;
			// Chargement de l'icône de l'agent sur la localisation
			executablePanel.transform.Find("Header").Find("locateButton").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/droneIcon", typeof(Sprite)) as Sprite;
			// Affichage du nom de l'agent
			if(nameAgent != "")
				executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = nameAgent;
            else
				executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = "Drone "+nbDroneCreate;
		}

		AgentColor ac = MainLoop.instance.GetComponent<AgentColor>();
		scriptref.executablePanel.transform.Find("Scroll View").GetComponent<Image>().color = ((type == "robot" || type == "player") ? ac.playerBackground : ac.droneBackground);

		executablePanel.SetActive(false);
		GameObjectManager.bind(executablePanel);
		GameObjectManager.bind(entity);
		return entity;
	}

	private void createDoor(int gridX, int gridY, Direction.Dir orientation, int slotID){
		GameObject door = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/Door") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,3,gridX*3), Quaternion.Euler(0,0,0), LevelGO.transform);

		door.GetComponentInChildren<ActivationSlot>().slotID = slotID;
		door.GetComponentInChildren<Position>().x = gridX;
		door.GetComponentInChildren<Position>().y = gridY;
		door.GetComponentInChildren<Direction>().direction = orientation;
		GameObjectManager.bind(door);
	}

	private void createDecoration(string name, int gridX, int gridY, Direction.Dir orientation)
	{
		GameObject decoration = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/"+name) as GameObject, LevelGO.transform.position + new Vector3(gridY * 3, 3, gridX * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);

		decoration.GetComponent<Position>().x = gridX;
		decoration.GetComponent<Position>().y = gridY;
		decoration.GetComponent<Direction>().direction = orientation;
		GameObjectManager.bind(decoration);
	}

	private void createConsole(int state, int gridX, int gridY, List<int> slotIDs, Direction.Dir orientation)
	{
		GameObject activable = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/ActivableConsole") as GameObject, LevelGO.transform.position + new Vector3(gridY * 3, 3, gridX * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);

		activable.GetComponent<Activable>().slotID = slotIDs;
		DoorPath path = activable.GetComponentInChildren<DoorPath>();
		if (slotIDs.Count > 0)
			path.slotId = slotIDs[0];
		else
			path.slotId = -1;
		activable.GetComponent<Position>().x = gridX;
		activable.GetComponent<Position>().y = gridY;
		activable.GetComponent<Direction>().direction = orientation;
		if (state == 1)
			activable.AddComponent<TurnedOn>();
		GameObjectManager.bind(activable);
	}

	private void createSpawnExit(int gridX, int gridY, bool type, bool hideExit = false){
		GameObject spawnExit;
		if(type)
			spawnExit = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/TeleporterSpawn") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,1.5f,gridX*3), Quaternion.Euler(-90,0,0), LevelGO.transform);
		else
			spawnExit = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/TeleporterExit") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,1.5f,gridX*3), Quaternion.Euler(-90,0,0), LevelGO.transform);

		if (hideExit)
        {
			Component.Destroy(spawnExit.GetComponent<Renderer>());
			Component.Destroy(spawnExit.GetComponent<ParticleSystem>());
        }
			
		spawnExit.GetComponent<Position>().x = gridX;
		spawnExit.GetComponent<Position>().y = gridY;
		GameObjectManager.bind(spawnExit);
	}

	private void createCoin(int gridX, int gridY){
		GameObject coin = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/Coin") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,3,gridX*3), Quaternion.Euler(90,0,0), LevelGO.transform);
		coin.GetComponent<Position>().x = gridX;
		coin.GetComponent<Position>().y = gridY;
		GameObjectManager.bind(coin);
	}

	private void createCell(int gridX, int gridY){
		GameObject cell = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/Cell") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,0,gridX*3), Quaternion.Euler(0,0,0), LevelGO.transform);
		GameObjectManager.bind(cell);
	}

	private void createWall(int gridX, int gridY, bool visible = true){
		GameObject wall = GameObject.Instantiate<GameObject>(Resources.Load ("Prefabs/Wall") as GameObject, LevelGO.transform.position + new Vector3(gridY*3,3,gridX*3), Quaternion.Euler(0,0,0), LevelGO.transform);
		wall.GetComponent<Position>().x = gridX;
		wall.GetComponent<Position>().y = gridY;
		if (!visible)
			wall.GetComponent<Renderer>().enabled = false;
		GameObjectManager.bind(wall);
	}

	private void eraseMap(){
		foreach( GameObject go in f_level){
			GameObjectManager.unbind(go.gameObject);
			GameObject.Destroy(go.gameObject);
		}
	}

	// Load the data of the map from XML
	private void readXMLMap(XmlNode mapNode){
		foreach(XmlNode lineNode in mapNode.ChildNodes){
			List<int> line = new List<int>();
			foreach(XmlNode cellNode in lineNode.ChildNodes){
				line.Add(int.Parse(cellNode.Attributes.GetNamedItem("value").Value));
			}
			map.Add(line);
		}
	}
	private GameObject getLibraryItemByName(string itemName)
	{
		foreach (GameObject item in f_draggableElement)
			if (item.name == itemName)
				return item;
		return null;
	}

	private void readXMLLimits(XmlNode limitsNode){
		string actionName = null;
		foreach (XmlNode limitNode in limitsNode.ChildNodes)
		{
			actionName = limitNode.Attributes.GetNamedItem("blockType").Value;
			// check if a GameObject exists with the same name
			if (getLibraryItemByName(actionName) && !gameData.actionBlockLimit.ContainsKey(actionName)){
				gameData.actionBlockLimit[actionName] = int.Parse(limitNode.Attributes.GetNamedItem("limit").Value);
			}
		}
	}

	private void readXMLConsole(XmlNode activableNode){
		List<int> slotsID = new List<int>();

		foreach(XmlNode child in activableNode.ChildNodes){
			slotsID.Add(int.Parse(child.Attributes.GetNamedItem("slotId").Value));
		}

		createConsole(int.Parse(activableNode.Attributes.GetNamedItem("state").Value), int.Parse(activableNode.Attributes.GetNamedItem("posX").Value), int.Parse(activableNode.Attributes.GetNamedItem("posY").Value),
		 slotsID, (Direction.Dir)int.Parse(activableNode.Attributes.GetNamedItem("direction").Value));
	}
}
