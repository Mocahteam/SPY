using UnityEngine;
using FYFY;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Read XML file and load level
/// Need to be binded before UISystem
/// </summary>
public class LevelGenerator : FSystem {

	public static LevelGenerator instance;

	// Famille contenant les agents editables
	private Family f_level = FamilyManager.getFamily(new AnyOfComponents(typeof(Position), typeof(CurrentAction)));

	private List<List<int>> map;
	private GameData gameData;
	private int nbAgentCreate = 0; // Nombre d'agents créés

	public GameObject camera;
	public GameObject editableCanvas;// Le container qui contient les Viewport/script containers
	public GameObject scriptContainer;
	public GameObject library; // Le viewport qui contient la librairie
	public GameObject EditableContenair; // Le container qui contient les séquences éditables
	public TMP_Text levelName;
	public GameObject canvas;
	public GameObject buttonExecute;

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
			gameData.Level = GameObject.Find("Level");
			XmlDocument doc = new XmlDocument();
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				MainLoop.instance.StartCoroutine(GetLevelWebRequest(doc));
			}
			else
			{
				doc.Load(gameData.levelList[gameData.levelToLoad.Item1][gameData.levelToLoad.Item2]);
				XmlToLevel(doc, gameData.levelList[gameData.levelToLoad.Item1][gameData.levelToLoad.Item2]);
			}
			levelName.text = Path.GetFileNameWithoutExtension(gameData.levelList[gameData.levelToLoad.Item1][gameData.levelToLoad.Item2]);
		}
	}

	IEnumerator GetLevelWebRequest(XmlDocument doc)
	{
		UnityWebRequest www = UnityWebRequest.Get(gameData.levelList[gameData.levelToLoad.Item1][gameData.levelToLoad.Item2]);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
			Debug.Log(www.error);
		else
		{
			doc.LoadXml(www.downloadHandler.text);
			XmlToLevel(doc, gameData.levelList[gameData.levelToLoad.Item1][gameData.levelToLoad.Item2]);
		}
	}

	// read the map and create wall, ground, spawn and exit
	private void generateMap(){
		for(int i = 0; i< map.Count; i++){
			for(int j = 0; j < map[i].Count; j++){
				switch (map[i][j]){
					case -1: // void
						break;
					case 0: // Path
						createCell(i,j);
						break;
					case 1: // Wall
						createCell(i,j);
						createWall(i,j);
						break;
					case 2: // Spawn
						createCell(i,j);
						createSpawnExit(i,j,true);
						break;
					case 3: // Exit
						createCell(i,j);
						createSpawnExit(i,j,false);
						break;
				}
			}
		}
	}

	// Créer une entité agent ou robot et y associer un panel container
	private GameObject createEntity(string nameAgent, int i, int j, Direction.Dir direction, string type, List<GameObject> script = null){
		GameObject entity = null;
		switch(type){
			case "player": // Robot
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Robot Kyle") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
			case "enemy": // Enemy
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Drone") as GameObject, gameData.Level.transform.position + new Vector3(i*3,5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
		}
        // Si la function permettant de changer le nom de l'agent est activée
        if (gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains("F9"))
        {
			entity.GetComponent<AgentEdit>().editState = AgentEdit.EditMode.Editable;
		}

		// Charger l'agent aux bonnes coordonées dans la bonne direction
		entity.GetComponent<Position>().x = i;
		entity.GetComponent<Position>().z = j;
		entity.GetComponent<Direction>().direction = direction;
		
		//add new container to entity
		ScriptRef scriptref = entity.GetComponent<ScriptRef>();
		GameObject executablePanel = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/ExecutablePanel") as GameObject, scriptContainer.gameObject.transform, false);
		// Associer à l'agent l'UI container
		scriptref.executablePanel = executablePanel;
		// Associer à l'agent le script container
		scriptref.executableScript = executablePanel.transform.Find("Scroll View").Find("Viewport").Find("ScriptContainer").gameObject;
		// Association de l'agent au script de gestion des fonctions
		executablePanel.GetComponentInChildren<LinkedWith>().target = entity;

		// On va charger l'image et le nom de l'agent selon l'agent (robot, enemie etc...)
		if (type == "player")
		{
			nbAgentCreate++;
			// On nomme l'agent
			AgentEdit agentEdit = entity.GetComponent<AgentEdit>();
            if (nameAgent != "")
            {
				agentEdit.agentName = nameAgent;
			}
            else
            {
				if (gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains("F9"))
				{
					agentEdit.agentName = "Agent" + nbAgentCreate;
				}
				else
				{
					agentEdit.agentName = "Script " + nbAgentCreate;
				}
			}

			// Si l'agent est en mode Locked ou Synchro ou qu'un script est défini, on crée une zone de programmation dédiée
			if (agentEdit.editState == AgentEdit.EditMode.Locked || agentEdit.editState == AgentEdit.EditMode.Synch || script != null)
				GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { name = agentEdit.agentName, editState = agentEdit.editState, script = script });

			// Chargement de l'icône de l'agent sur la localisation
			executablePanel.transform.Find("Header").Find("locateButton").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/robotIcon", typeof(Sprite)) as Sprite;
			// Affichage du nom de l'agent
			executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = entity.GetComponent<AgentEdit>().agentName;
			// Si on autorise le changement de nom on dévérouille la possibilité d'écrire dans la zone de nom du robot
			if (entity.GetComponent<AgentEdit>().editState != AgentEdit.EditMode.Locked)
			{
				executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().interactable = true;
			}
		}
		else if (type == "enemy")
		{
			// Chargement de l'icône de l'agent sur la localisation
			executablePanel.transform.Find("Header").Find("locateButton").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/droneIcon", typeof(Sprite)) as Sprite;
			// Affichage du nom de l'agent
			if(nameAgent != "")
            {
				executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = nameAgent;
			}
            else
            {
				executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = "Drone";
			}

			if (script != null)
			{
				GameObject tmpContainer = GameObject.Instantiate(scriptref.executableScript);
				foreach (GameObject go in script)
					go.transform.SetParent(tmpContainer.transform, false); //add actions to container
				EditingUtility.fillExecutablePanel(tmpContainer, scriptref.executableScript, entity.tag);
				// On fait apparaitre le panneau du robot
				scriptref.executablePanel.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;
				Object.Destroy(tmpContainer);
			}
		}

		AgentColor ac = MainLoop.instance.GetComponent<AgentColor>();
		scriptref.executablePanel.transform.Find("Scroll View").GetComponent<Image>().color = (type == "player" ? ac.playerBackground : ac.droneBackground);

		executablePanel.SetActive(false);
		GameObjectManager.bind(executablePanel);
		GameObjectManager.bind(entity);
		return entity;
	}

	private void createDoor(int i, int j, Direction.Dir orientation, int slotID){
		GameObject door = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Door") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);

		door.GetComponent<ActivationSlot>().slotID = slotID;
		door.GetComponent<Position>().x = i;
		door.GetComponent<Position>().z = j;
		door.GetComponent<Direction>().direction = orientation;
		GameObjectManager.bind(door);
	}

	private void createActivable(int i, int j, List<int> slotIDs, Direction.Dir orientation)
	{
		GameObject activable = Object.Instantiate<GameObject>(Resources.Load("Prefabs/ActivableConsole") as GameObject, gameData.Level.transform.position + new Vector3(i * 3, 3, j * 3), Quaternion.Euler(0, 0, 0), gameData.Level.transform);

		activable.GetComponent<Activable>().slotID = slotIDs;
		activable.GetComponent<Position>().x = i;
		activable.GetComponent<Position>().z = j;
		activable.GetComponent<Direction>().direction = orientation;
		GameObjectManager.bind(activable);
	}

	private void createSpawnExit(int i, int j, bool type){
		GameObject spawnExit;
		if(type)
			spawnExit = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/TeleporterSpawn") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(-90,0,0), gameData.Level.transform);
		else
			spawnExit = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/TeleporterExit") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(-90,0,0), gameData.Level.transform);

		spawnExit.GetComponent<Position>().x = i;
		spawnExit.GetComponent<Position>().z = j;
		GameObjectManager.bind(spawnExit);
	}

	private void createCoin(int i, int j){
		GameObject coin = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Coin") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(90,0,0), gameData.Level.transform);
		coin.GetComponent<Position>().x = i;
		coin.GetComponent<Position>().z = j;
		GameObjectManager.bind(coin);
	}

	private void createCell(int i, int j){
		GameObject cell = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Cell") as GameObject, gameData.Level.transform.position + new Vector3(i*3,0,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
		GameObjectManager.bind(cell);
	}

	private void createWall(int i, int j){
		GameObject wall = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Wall") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
		wall.GetComponent<Position>().x = i;
		wall.GetComponent<Position>().z = j;
		GameObjectManager.bind(wall);
	}

	private void eraseMap(){
		foreach( GameObject go in f_level){
			GameObjectManager.unbind(go.gameObject);
			Object.Destroy(go.gameObject);
		}
	}

	// Read xml document and create all game objects
	public void XmlToLevel(XmlDocument doc, string nameLevel)
	{

		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
		gameData.dialogMessage = new List<(string, string)>();
		gameData.actionBlocLimit = new Dictionary<string, int>();
		map = new List<List<int>>();

		XmlNode root = doc.ChildNodes[1];
		foreach(XmlNode child in root.ChildNodes){
			switch(child.Name){
				case "info":
					readXMLInfos(child);
					break;
				case "map":
					readXMLMap(child);
					break;
				case "dialogs":
					string src = null;
					//optional xml attribute
					if(child.Attributes["img"] !=null)
						src = child.Attributes.GetNamedItem("img").Value;
					gameData.dialogMessage.Add((child.Attributes.GetNamedItem("dialog").Value, src));
					break;
				case "actionBlocLimit":
					readXMLLimits(child);
					break;
				case "coin":
					createCoin(int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posZ").Value));
					break;
				case "activable":
					readXMLActivable(child);
					break;
				case "door":
					createDoor(int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posZ").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value), int.Parse(child.Attributes.GetNamedItem("slot").Value));
					break;
				case "player":
					string nameAgentByUser = "";
					if(child.Attributes.GetNamedItem("name").Value != "")
                    {
						nameAgentByUser = child.Attributes.GetNamedItem("name").Value;
					}
					createEntity(nameAgentByUser, int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posZ").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value),"player", readXMLScript(child.ChildNodes[0]));
					break;		
				case "enemy":
					string nameSentinelleByUser = "";
					if (child.Attributes.GetNamedItem("name").Value != "")
					{
						nameSentinelleByUser = child.Attributes.GetNamedItem("name").Value;
					}
					GameObject enemy = createEntity(nameSentinelleByUser, int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posZ").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value),"enemy", readXMLScript(child.ChildNodes[0]));
					enemy.GetComponent<DetectRange>().range = int.Parse(child.Attributes.GetNamedItem("range").Value);
					enemy.GetComponent<DetectRange>().selfRange = bool.Parse(child.Attributes.GetNamedItem("selfRange").Value);
					enemy.GetComponent<DetectRange>().type = (DetectRange.Type)int.Parse(child.Attributes.GetNamedItem("typeRange").Value);
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
		activeDesactiveFunctionality();
        GameObjectManager.addComponent<GameLoaded>(MainLoop.instance.gameObject);
	}

	// Si le niveau n'a pas été lancé par la selection de compétence,
	// Alors on va noter les fonctionnalité (hors level design) qui doivent être activées dans le niveau
	private void readXMLInfos(XmlNode infoNode){
        if (!gameData.GetComponent<GameData>().executeLvlByComp){
			foreach (XmlNode child in infoNode){
				switch(child.Name)
                {
					case "func":
						readFunc(child);
						break;
					default:
						break;
                }
			}
		}
	}

	// Ajoute la fonction parametrée dans le niveau dans la liste des fonctions selectionnées dans le gameData si celle ci ne s'y trouve pas déjà
	private void readFunc(XmlNode FuncNode)
    {
        if (!gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(FuncNode.Attributes.GetNamedItem("name").Value))
        {
			gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Add(FuncNode.Attributes.GetNamedItem("name").Value);
		}
		 // tester si func associé et réitéré
		foreach(string funcName in gameData.GetComponent<FunctionalityParam>().activeFunc[FuncNode.Attributes.GetNamedItem("name").Value])
        {
			if(funcName != "")
            {
				selectFuncAssociate(funcName);
			}
        }
    }

	private void selectFuncAssociate(string functionnalityName)
    {
		if (!gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(functionnalityName))
        {
			gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Add(functionnalityName);
			foreach (string funcName in gameData.GetComponent<FunctionalityParam>().activeFunc[functionnalityName])
			{
				if (funcName != "")
				{
					selectFuncAssociate(funcName);
				}
			}
		}
	}

	// Load the data of the map from XML
	private void readXMLMap(XmlNode mapNode){
		foreach(XmlNode lineNode in mapNode.ChildNodes){
			List<int> line = new List<int>();
			foreach(XmlNode rowNode in lineNode.ChildNodes){
				line.Add(int.Parse(rowNode.Attributes.GetNamedItem("value").Value));
			}
			map.Add(line);
		}
	}

	private void readXMLLimits(XmlNode limitsNode){
		List<string> listFuncGD = gameData.GetComponent<FunctionalityParam>().funcActiveInLevel;
		// Si l'option F2 est activée
		if (listFuncGD.Contains("F2"))
        {
			string actionName = null;
			foreach (XmlNode limitNode in limitsNode.ChildNodes)
			{
				actionName = limitNode.Attributes.GetNamedItem("actionType").Value;
				// check if a GameObject exists with the same name
				if (GameObject.Find(actionName) && !gameData.actionBlocLimit.ContainsKey(actionName)){
					gameData.actionBlocLimit[actionName] = int.Parse(limitNode.Attributes.GetNamedItem("limit").Value);
				}
			}
			tcheckRequierement();
		} // Sinon on met toutes les limites de bloc à 0
        else
        {
			foreach(string nameFunc in gameData.actionBlocLimit.Keys)
            {
				gameData.actionBlocLimit[nameFunc] = 0;
			}
		}
		
	}

	private void tcheckRequierement()
    {
		// Besoin de regarder si les capteurs sont correctement parametrés
		bool prgOkCaptor = tcheckCaptorProgramationValide();

		foreach (string funcName in gameData.GetComponent<FunctionalityParam>().elementRequiermentLibrary.Keys)
        {
			if (gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains(funcName))
            {
				bool errorPrgFunc = true;
				foreach (string element in gameData.GetComponent<FunctionalityParam>().elementRequiermentLibrary[funcName])
				{
					//On regarde si c'est un capteur ou non
					if (element != "Captor")
					{
						if (gameData.actionBlocLimit[element] != 0)
						{
							errorPrgFunc = false;
						}
					}
				}

                if (errorPrgFunc || !prgOkCaptor)
                {
					foreach (string element in gameData.GetComponent<FunctionalityParam>().elementRequiermentLibrary[funcName])
					{
						//On regarde si c'est une erreur de programation capteur (et si l'element est un capteur)
						// On vérifie aussi que l'élément à besoin de capteur
						if (!prgOkCaptor && gameData.GetComponent<FunctionalityParam>().elementRequiermentLibrary[funcName].Contains("Captor"))
						{
							// On passe tous les capteurs à -1
							foreach (string captor in gameData.GetComponent<FunctionalityParam>().listCaptor)
                            {
								gameData.actionBlocLimit[captor] = -1;
							}
						}

						if(errorPrgFunc && element != "Captor")
						{
							gameData.actionBlocLimit[element] = -1;
						}
					}
				}
			}
        }
	}

	private bool tcheckCaptorProgramationValide()
    {
		bool tcheck = false;
		foreach(string ele in gameData.actionBlocLimit.Keys)
        {
            if (gameData.GetComponent<FunctionalityParam>().listCaptor.Contains(ele))
            {
				// Si au moins un des capteurs est présent dans les limitations (autre que 0)
				// On considére qu'il n'y a pas d'erreur de programation du niveau pour les capteurs
				if(gameData.actionBlocLimit[ele] != 0)
                {
					tcheck = true;
				}
            }
        }
		return tcheck;
	}

	private void readXMLActivable(XmlNode activableNode){
		List<int> slotsID = new List<int>();

		foreach(XmlNode child in activableNode.ChildNodes){
			slotsID.Add(int.Parse(child.Attributes.GetNamedItem("slot").Value));
		}

		createActivable(int.Parse(activableNode.Attributes.GetNamedItem("posX").Value), int.Parse(activableNode.Attributes.GetNamedItem("posZ").Value),
		 slotsID, (Direction.Dir)int.Parse(activableNode.Attributes.GetNamedItem("direction").Value));
	}

	// Lit le XML d'un script est génère les game objects des instructions
	private List<GameObject> readXMLScript(XmlNode scriptNode)
	{
		if(scriptNode != null){
			List<GameObject> script = new List<GameObject>();
			foreach(XmlNode actionNode in scriptNode.ChildNodes){
				script.Add(readXMLInstruction(actionNode));
			}
			return script;			
		}
		return null;
	}

	// Transforme le noeud d'action XML en gameObject
	private GameObject readXMLInstruction(XmlNode actionNode){
		GameObject obj = null;
		BaseElement action = null;
		Transform conditionContainer = null;
		Transform firstContainerBloc = null;
		Transform secondContainerBloc = null;
        switch (actionNode.Name)
        {
			case "control":
				switch (actionNode.Attributes.GetNamedItem("type").Value)
                {
					case "If":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("If"), canvas);

						conditionContainer = obj.transform.Find("ConditionContainer");
						firstContainerBloc = obj.transform.Find("Container");
						action = obj.GetComponent<IfControl>();

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

					case "IfElse":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("IfElse"), canvas);
						conditionContainer = obj.transform.Find("ConditionContainer");
						firstContainerBloc = obj.transform.Find("Container");
						secondContainerBloc = obj.transform.Find("ElseContainer");
						action = obj.GetComponent<IfControl>();

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
							else if (containerNode.Name == "container" && containerNode.Attributes.GetNamedItem("type").Value == "ThenContainer")
							{
								if (containerNode.HasChildNodes)
									processXMLInstruction(firstContainerBloc, containerNode);
							}
							else if (containerNode.Name == "container" && containerNode.Attributes.GetNamedItem("type").Value == "ElseContainer")
							{
								if (containerNode.HasChildNodes)
									processXMLInstruction(secondContainerBloc, containerNode);
							}
						}
						break;

					case "For":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("For"), canvas);
						firstContainerBloc = obj.transform.Find("Container");
						action = obj.GetComponent<ForControl>();

						((ForControl)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
						obj.transform.GetComponentInChildren<TMP_InputField>().text = ((ForControl)action).nbFor.ToString();

						if (actionNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, actionNode);
						break;

					case "While":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("While"), canvas);
						firstContainerBloc = obj.transform.Find("Container");
						conditionContainer = obj.transform.Find("ConditionContainer");
						action = obj.GetComponent<WhileControl>();

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

					case "Forever":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("Forever"), canvas);
						firstContainerBloc = obj.transform.Find("Container");

						if (actionNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, actionNode);
						break;
				}
				break;
			case "action":
				obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find(actionNode.Attributes.GetNamedItem("type").Value), canvas);
				break;
        }

		return obj;
	}

	private void processXMLInstruction(Transform gameContainer, XmlNode xmlContainer)
	{
		// The first child of a control container is a drop zone and the second an emptySolt
		GameObject dropZone = gameContainer.GetChild(0).gameObject;
		GameObject emptySlot = gameContainer.GetChild(1).gameObject;
		bool firstchild = true;
		foreach (XmlNode eleNode in xmlContainer.ChildNodes)
		{
			GameObject child = readXMLInstruction(eleNode);
			if (firstchild) // add the first child to the emptySlot
			{
				EditingUtility.addItemOnDropArea(child, emptySlot);
				firstchild = false;
			}
			else // add next childs to the dropZone
				EditingUtility.addItemOnDropArea(child, dropZone);
		}
	}

	// Transforme le noeud d'action XML en gameObject élément/opérator
	private GameObject readXMLCondition(XmlNode conditionNode) {
		GameObject obj = null;
		ReplacementSlot[] slots = null;
		switch (conditionNode.Name)
        {
			case "operator":
				switch (conditionNode.Attributes.GetNamedItem("type").Value)
                {
					case "AndOperator":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("AndOperator"), canvas);
						slots = obj.GetComponentsInChildren<ReplacementSlot>();
						if (conditionNode.HasChildNodes)
						{
							GameObject emptyZone = null;
							foreach (XmlNode andNode in conditionNode.ChildNodes)
							{
								if (andNode.Attributes.GetNamedItem("type").Value == "Left")
									// The Left slot is the second ReplacementSlot (first is the And operator)
									emptyZone = slots[1].gameObject;
								if (andNode.Attributes.GetNamedItem("type").Value == "Right")
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

					case "OrOperator":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("OrOperator"), canvas);
						slots = obj.GetComponentsInChildren<ReplacementSlot>();
						if (conditionNode.HasChildNodes)
						{
							GameObject emptyZone = null;
							foreach (XmlNode orNode in conditionNode.ChildNodes)
							{
								if (orNode.Attributes.GetNamedItem("type").Value == "Left")
									// The Left slot is the second ReplacementSlot (first is the And operator)
									emptyZone = slots[1].gameObject;
								if (orNode.Attributes.GetNamedItem("type").Value == "Right")
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

					case "NotOperator":
						obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find("NotOperator"), canvas);
						if (conditionNode.HasChildNodes)
						{
							GameObject emptyZone = obj.transform.Find("Container").GetChild(1).gameObject;
							GameObject child = readXMLCondition(conditionNode.FirstChild);
							// Add child to empty zone
							EditingUtility.addItemOnDropArea(child, emptyZone);
						}
						break;
				}
				break;
			case "captor":
				obj = EditingUtility.createEditableBlockFromLibrary(GameObject.Find(conditionNode.Attributes.GetNamedItem("type").Value), canvas);
				break;
        }

		return obj;
	}

	private void activeDesactiveFunctionality()
    {
        // Si F1 désactivé, on désactive la librairie
        if (!gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains("F1"))
        {
			library.SetActive(false);
		}
		// Si F3 désactivé, on désactive le systéme DragDropSystem
		if (!gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains("F3"))
        {
			DragDropSystem.instance.Pause = true;
		}
		// Si F8 est désactivé, le bouton pour que le robot effectue une sequence d'action n'est plus disponible
		if (!gameData.GetComponent<FunctionalityParam>().funcActiveInLevel.Contains("F8"))
		{
			buttonExecute.SetActive(false);
		}
	}
}
