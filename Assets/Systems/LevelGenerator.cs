using UnityEngine;
using FYFY;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

/// <summary>
/// Read XML file and load level
/// Need to be binded before UISystem
/// </summary>
public class LevelGenerator : FSystem {

	public static LevelGenerator instance;

	// Famille contenant les agents editables
	private Family f_level = FamilyManager.getFamily(new AnyOfComponents(typeof(Position), typeof(CurrentAction)));
	private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)), new AnyOfTags("Drone")); // On récupére les agents pouvant être édités
	private Family f_draggableElement = FamilyManager.getFamily(new AnyOfComponents(typeof(ElementToDrag)));

	private List<List<int>> map;
	private GameData gameData;
	private int nbAgentCreate = 0; // Nombre d'agents créés
	private int nbDroneCreate = 0; // Nombre de drones créés
	private HashSet<string> scriptNameUsed = new HashSet<string>();
	private GameObject lastAgentCreated = null;

	public GameObject editableCanvas;// Le container qui contient les Viewport/script containers
	public GameObject scriptContainer;
	public GameObject library; // Le viewport qui contient la librairie
	public GameObject EditableContenair; // Le container qui contient les séquences éditables
	public TMP_Text levelName;
	public GameObject canvas;
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
			gameData.LevelGO = GameObject.Find("Level");
			if (gameData.levels.ContainsKey(gameData.levelToLoad))
				XmlToLevel(gameData.levels[gameData.levelToLoad].OwnerDocument);
			else
				GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Error });
			levelName.text = Path.GetFileNameWithoutExtension(gameData.levelToLoad);
			if (Application.platform == RuntimePlatform.WebGLPlayer)
				HideHtmlButtons();
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
		gameData.dialogMessage = new List<(string, string, float, int, int)>();
		gameData.actionBlockLimit = new Dictionary<string, int>();
		map = new List<List<int>>();

		// remove comments
		EditingUtility.removeComments(doc);

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
					readXMLDialogs(child);
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
				case "player":
				case "enemy":
					string nameAgentByUser = "";
					XmlNode agentName = child.Attributes.GetNamedItem("associatedScriptName");
					if (agentName != null && agentName.Value != "")
						nameAgentByUser = agentName.Value;
					GameObject agent = createEntity(nameAgentByUser, int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posY").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value), child.Name);
					if (child.Name == "enemy")
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
					// Script has to be created after agents
					MainLoop.instance.StartCoroutine(delayReadXMLScript(child, child.Attributes.GetNamedItem("name").Value, editModeByUser, typeByUser));
					break;
				case "score":
					gameData.levelToLoadScore = new int[2];
					gameData.levelToLoadScore[0] = int.Parse(child.Attributes.GetNamedItem("threeStars").Value);
					gameData.levelToLoadScore[1] = int.Parse(child.Attributes.GetNamedItem("twoStars").Value);
					break;
			}
		}
		eraseMap();
		generateMap();
		MainLoop.instance.StartCoroutine(delayGameLoaded());
	}

	IEnumerator delayGameLoaded()
	{
		yield return null;
		yield return null;
		GameObjectManager.addComponent<GameLoaded>(MainLoop.instance.gameObject);
	}

	IEnumerator delayEnableFog()
	{
		yield return null;
		if (lastAgentCreated != null)
		{
			Transform fog = lastAgentCreated.transform.Find("Fog");
			if (fog != null)
				GameObjectManager.setGameObjectState(fog.gameObject, true);
		}
	}

	IEnumerator delayReadXMLScript(XmlNode scriptNode, string name, UIRootContainer.EditMode editMode, UIRootContainer.SolutionType type)
    {
		yield return null;
		readXMLScript(scriptNode, name, editMode, type);
	}

	// read the map and create wall, ground, spawn and exit
	private void generateMap(){
		for(int y = 0; y< map.Count; y++){
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
						createSpawnExit(x,y,false);
						break;
				}
			}
		}
	}

	// Créer une entité agent ou robot et y associer un panel container
	private GameObject createEntity(string nameAgent, int gridX, int gridY, Direction.Dir direction, string type){
		GameObject entity = null;
		switch(type){
			case "player": // Robot
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Robot Kyle") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,1.5f,gridX*3), Quaternion.Euler(0,0,0), gameData.LevelGO.transform);
				break;
			case "enemy": // Enemy
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Drone") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,5f,gridX*3), Quaternion.Euler(0,0,0), gameData.LevelGO.transform);
				break;
		}

		// Charger l'agent aux bonnes coordonées dans la bonne direction
		entity.GetComponent<Position>().x = gridX;
		entity.GetComponent<Position>().y = gridY;
		entity.GetComponent<Direction>().direction = direction;
		
		//add new container to entity
		ScriptRef scriptref = entity.GetComponent<ScriptRef>();
		GameObject executablePanel = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/ExecutablePanel") as GameObject, scriptContainer.gameObject.transform, false);
		// Associer à l'agent l'UI container
		scriptref.executablePanel = executablePanel;
		// Associer à l'agent le script container
		scriptref.executableScript = executablePanel.transform.Find("Scroll View").Find("Viewport").Find("ScriptContainer").gameObject;
		// Association de l'agent au script de gestion des fonctions
		executablePanel.GetComponentInChildren<LinkedWith>(true).target = entity;

		// On va charger l'image et le nom de l'agent selon l'agent (robot, ennemi etc...)
		if (type == "player")
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
		else if (type == "enemy")
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
		scriptref.executablePanel.transform.Find("Scroll View").GetComponent<Image>().color = (type == "player" ? ac.playerBackground : ac.droneBackground);

		executablePanel.SetActive(false);
		GameObjectManager.bind(executablePanel);
		GameObjectManager.bind(entity);
		return entity;
	}

	private void createDoor(int gridX, int gridY, Direction.Dir orientation, int slotID){
		GameObject door = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Door") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,3,gridX*3), Quaternion.Euler(0,0,0), gameData.LevelGO.transform);

		door.GetComponentInChildren<ActivationSlot>().slotID = slotID;
		door.GetComponentInChildren<Position>().x = gridX;
		door.GetComponentInChildren<Position>().y = gridY;
		door.GetComponentInChildren<Direction>().direction = orientation;
		GameObjectManager.bind(door);
	}

	private void createDecoration(string name, int gridX, int gridY, Direction.Dir orientation)
	{
		GameObject decoration = Object.Instantiate<GameObject>(Resources.Load("Prefabs/"+name) as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY * 3, 3, gridX * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);

		decoration.GetComponent<Position>().x = gridX;
		decoration.GetComponent<Position>().y = gridY;
		decoration.GetComponent<Direction>().direction = orientation;
		GameObjectManager.bind(decoration);
	}

	private void createConsole(int state, int gridX, int gridY, List<int> slotIDs, Direction.Dir orientation)
	{
		GameObject activable = Object.Instantiate<GameObject>(Resources.Load("Prefabs/ActivableConsole") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY * 3, 3, gridX * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);

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

	private void createSpawnExit(int gridX, int gridY, bool type){
		GameObject spawnExit;
		if(type)
			spawnExit = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/TeleporterSpawn") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,1.5f,gridX*3), Quaternion.Euler(-90,0,0), gameData.LevelGO.transform);
		else
			spawnExit = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/TeleporterExit") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,1.5f,gridX*3), Quaternion.Euler(-90,0,0), gameData.LevelGO.transform);

		spawnExit.GetComponent<Position>().x = gridX;
		spawnExit.GetComponent<Position>().y = gridY;
		GameObjectManager.bind(spawnExit);
	}

	private void createCoin(int gridX, int gridY){
		GameObject coin = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Coin") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,3,gridX*3), Quaternion.Euler(90,0,0), gameData.LevelGO.transform);
		coin.GetComponent<Position>().x = gridX;
		coin.GetComponent<Position>().y = gridY;
		GameObjectManager.bind(coin);
	}

	private void createCell(int gridX, int gridY){
		GameObject cell = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Cell") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,0,gridX*3), Quaternion.Euler(0,0,0), gameData.LevelGO.transform);
		GameObjectManager.bind(cell);
	}

	private void createWall(int gridX, int gridY, bool visible = true){
		GameObject wall = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Wall") as GameObject, gameData.LevelGO.transform.position + new Vector3(gridY*3,3,gridX*3), Quaternion.Euler(0,0,0), gameData.LevelGO.transform);
		wall.GetComponent<Position>().x = gridX;
		wall.GetComponent<Position>().y = gridY;
		if (!visible)
			wall.GetComponent<Renderer>().enabled = false;
		GameObjectManager.bind(wall);
	}

	private void eraseMap(){
		foreach( GameObject go in f_level){
			GameObjectManager.unbind(go.gameObject);
			Object.Destroy(go.gameObject);
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

	private void readXMLDialogs(XmlNode dialogs)
	{
		foreach (XmlNode dialog in dialogs.ChildNodes)
		{
			string text = null;
			if (dialog.Attributes.GetNamedItem("text") != null)
				text = dialog.Attributes.GetNamedItem("text").Value;
			string src = null;
			if (dialog.Attributes.GetNamedItem("img") != null)
				src = dialog.Attributes.GetNamedItem("img").Value;
			float imgHeight = -1;
			if (dialog.Attributes.GetNamedItem("imgHeight") != null)
				imgHeight = float.Parse(dialog.Attributes.GetNamedItem("imgHeight").Value);
			int camX = -1;
			if (dialog.Attributes.GetNamedItem("camX") != null)
				camX = int.Parse(dialog.Attributes.GetNamedItem("camX").Value);
			int camY = -1;
			if (dialog.Attributes.GetNamedItem("camY") != null)
				camY = int.Parse(dialog.Attributes.GetNamedItem("camY").Value);
			gameData.dialogMessage.Add((text, src, imgHeight, camX, camY));
		}
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

	// Lit le XML d'un script est génère les game objects des instructions
	private void readXMLScript(XmlNode scriptNode, string name, UIRootContainer.EditMode editMode, UIRootContainer.SolutionType type)
	{
		if(scriptNode != null){
			List<GameObject> script = new List<GameObject>();
			foreach(XmlNode actionNode in scriptNode.ChildNodes){
				script.Add(readXMLInstruction(actionNode));
			}

			// Look for another script with the same name. If one already exists, we don't create one more.
			if (!scriptNameUsed.Contains(name))
            {
				// Rechercher un drone associé à ce script
				bool droneFound = false;
				foreach (GameObject drone in f_drone)
				{
					ScriptRef scriptRef = drone.GetComponent<ScriptRef>();
					if (scriptRef.executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text == name)
					{
						GameObject tmpContainer = GameObject.Instantiate(scriptRef.executableScript);
						foreach (GameObject go in script)
							go.transform.SetParent(tmpContainer.transform, false); //add actions to container
						EditingUtility.fillExecutablePanel(tmpContainer, scriptRef.executableScript, drone.tag);
						// bind all child
						foreach (Transform child in scriptRef.executableScript.transform)
							GameObjectManager.bind(child.gameObject);
						// On fait apparaitre le panneau du robot
						scriptRef.executablePanel.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;
						GameObjectManager.setGameObjectState(scriptRef.executablePanel, true);
						Object.Destroy(tmpContainer);
						droneFound = true;
					}
				}
				if (!droneFound)
					GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { title = name, editState = editMode, typeState = type, script = script });
			}
			else
            {
				Debug.LogWarning("Script \"" + name + "\" not created because another one already exists. Only one script with the same name is possible.");
            }		
		}
	}

	private GameObject getLibraryItemByName(string itemName)
    {
		foreach (GameObject item in f_draggableElement)
			if (item.name == itemName)
				return item;
		return null;
	}

	// Transforme le noeud d'action XML en gameObject
	private GameObject readXMLInstruction(XmlNode actionNode){
		GameObject obj = null;
		Transform conditionContainer = null;
		Transform firstContainerBloc = null;
		Transform secondContainerBloc = null;
        switch (actionNode.Name)
        {
			case "if":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("IfThen"), canvas);

				conditionContainer = obj.transform.Find("ConditionContainer");
				firstContainerBloc = obj.transform.Find("Container");

				// On ajoute les éléments enfants dans les bons containers
				foreach (XmlNode containerNode in actionNode.ChildNodes)
				{
					// Ajout des conditions
					if (containerNode.Name == "condition")
					{
						if (containerNode.HasChildNodes)
						{
							// The first child of the conditional container of a If action contains the ReplacementSlot
							GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
							// Parse xml condition
							GameObject child = readXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							EditingUtility.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "container")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, containerNode);
					}
				}
				break;

			case "ifElse":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("IfElse"), canvas);
				conditionContainer = obj.transform.Find("ConditionContainer");
				firstContainerBloc = obj.transform.Find("Container");
				secondContainerBloc = obj.transform.Find("ElseContainer");

				// On ajoute les éléments enfants dans les bons containers
				foreach (XmlNode containerNode in actionNode.ChildNodes)
				{
					// Ajout des conditions
					if (containerNode.Name == "condition")
					{
						if (containerNode.HasChildNodes)
						{
							// The first child of the conditional container of a IfElse action contains the ReplacementSlot
							GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
							// Parse xml condition
							GameObject child = readXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							EditingUtility.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "thenContainer")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, containerNode);
					}
					else if (containerNode.Name == "elseContainer")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(secondContainerBloc, containerNode);
					}
				}
				break;

			case "for":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("ForLoop"), canvas);
				firstContainerBloc = obj.transform.Find("Container");
				BaseElement action = obj.GetComponent<ForControl>();

				((ForControl)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
				obj.transform.GetComponentInChildren<TMP_InputField>().text = ((ForControl)action).nbFor.ToString();

				if (actionNode.HasChildNodes)
					processXMLInstruction(firstContainerBloc, actionNode);
				break;

			case "while":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("While"), canvas);
				firstContainerBloc = obj.transform.Find("Container");
				conditionContainer = obj.transform.Find("ConditionContainer");

				// On ajoute les éléments enfants dans les bons containers
				foreach (XmlNode containerNode in actionNode.ChildNodes)
				{
					// Ajout des conditions
					if (containerNode.Name == "condition")
					{
						if (containerNode.HasChildNodes)
						{
							// The first child of the conditional container of a While action contains the ReplacementSlot
							GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
							// Parse xml condition
							GameObject child = readXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							EditingUtility.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "container")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, containerNode);
					}
				}
				break;

			case "forever":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("Forever"), canvas);
				firstContainerBloc = obj.transform.Find("Container");

				if (actionNode.HasChildNodes)
					processXMLInstruction(firstContainerBloc, actionNode);
				break;
			case "action":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName(actionNode.Attributes.GetNamedItem("type").Value), canvas);
				break;
        }

		return obj;
	}

	private void processXMLInstruction(Transform gameContainer, XmlNode xmlContainer)
	{
		// The first child of a control container is an emptySolt
		GameObject emptySlot = gameContainer.GetChild(0).gameObject;
		foreach (XmlNode eleNode in xmlContainer.ChildNodes)
			EditingUtility.addItemOnDropArea(readXMLInstruction(eleNode), emptySlot);
	}

	// Transforme le noeud d'action XML en gameObject élément/opérator
	private GameObject readXMLCondition(XmlNode conditionNode) {
		GameObject obj = null;
		ReplacementSlot[] slots = null;
		switch (conditionNode.Name)
        {
			case "and":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("AndOperator"), canvas);
				slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = null;
					foreach (XmlNode andNode in conditionNode.ChildNodes)
					{
						if (andNode.Name == "conditionLeft")
							// The Left slot is the second ReplacementSlot (first is the And operator)
							emptyZone = slots[1].gameObject;
						if (andNode.Name == "conditionRight")
							// The Right slot is the third ReplacementSlot
							emptyZone = slots[2].gameObject;
						if (emptyZone != null && andNode.HasChildNodes)
						{
							// Parse xml condition
							GameObject child = readXMLCondition(andNode.FirstChild);
							// Add child to empty zone
							EditingUtility.addItemOnDropArea(child, emptyZone);
						}
						emptyZone = null;
					}
				}
				break;

			case "or":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("OrOperator"), canvas);
				slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = null;
					foreach (XmlNode orNode in conditionNode.ChildNodes)
					{
						if (orNode.Name == "conditionLeft")
							// The Left slot is the second ReplacementSlot (first is the And operator)
							emptyZone = slots[1].gameObject;
						if (orNode.Name == "conditionRight")
							// The Right slot is the third ReplacementSlot
							emptyZone = slots[2].gameObject;
						if (emptyZone != null && orNode.HasChildNodes)
						{
							// Parse xml condition
							GameObject child = readXMLCondition(orNode.FirstChild);
							// Add child to empty zone
							EditingUtility.addItemOnDropArea(child, emptyZone);
						}
						emptyZone = null;
					}
				}
				break;

			case "not":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName("NotOperator"), canvas);
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = obj.transform.Find("Container").GetChild(1).gameObject;
					GameObject child = readXMLCondition(conditionNode.FirstChild);
					// Add child to empty zone
					EditingUtility.addItemOnDropArea(child, emptyZone);
				}
				break;
			case "captor":
				obj = EditingUtility.createEditableBlockFromLibrary(getLibraryItemByName(conditionNode.Attributes.GetNamedItem("type").Value), canvas);
				break;
        }

		return obj;
	}
}
