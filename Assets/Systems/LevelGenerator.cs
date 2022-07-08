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
/// </summary>
public class LevelGenerator : FSystem {

	// Famille contenant les agents editables
	private Family levelGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Position), typeof(CurrentAction)));
	private List<List<int>> map;
	private GameData gameData;
	private int nbAgent = 0; // Nombre d'agent créer
	public GameObject camera;
	public GameObject editableCanvas;// Le container qui contient les Viewport/script container
	public GameObject scriptContainer;
	public TMP_Text levelName;
	public GameObject canvas;

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
				doc.LoadXml(gameData.levelList[gameData.levelToLoad.Item1][gameData.levelToLoad.Item2]);
				XmlToLevel(doc, gameData.levelList[gameData.levelToLoad.Item1][gameData.levelToLoad.Item2]);
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

	// Creer une entité agent ou robot et y associe un panel container
	private GameObject createEntity(int i, int j, Direction.Dir direction, string type, List<GameObject> script = null){
		GameObject entity = null;
		switch(type){
			case "player": // Robot
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Robot Kyle") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
			case "enemy": // Enemy
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Drone") as GameObject, gameData.Level.transform.position + new Vector3(i*3,5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
		}
		// Charger l'agent aux bonnes coordonées dans la bonne direction
		entity.GetComponent<Position>().x = i;
		entity.GetComponent<Position>().z = j;
		entity.GetComponent<Direction>().direction = direction;
		
		//add new container to entity
		ScriptRef scriptref = entity.GetComponent<ScriptRef>();
		GameObject containerParent = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/ExecutablePanel") as GameObject, scriptContainer.gameObject.transform, false);
		// Associer à l'agent l'UI container
		scriptref.uiContainer = containerParent;
		// Associer à l'agent le script container
		scriptref.scriptContainer = containerParent.transform.Find("Scroll View").Find("Viewport").Find("ScriptContainer").gameObject;
		// Association de l'agent au script de gestion des fonctions
		containerParent.GetComponentInChildren<EditAgentSystemBridge>().agent = entity;

		// On va charger l'image et le nom de l'agent selon l'agent (robot, enemie etc...)
		if (type == "player")
		{
			nbAgent++;
			// On nomme l'agent
			AgentEdit agentEdit = entity.GetComponent<AgentEdit>();
			agentEdit.agentName = "Script" + nbAgent;

			// Si l'agent est en mode Locked ou Synchro ou qu'un script est défini, on crée une zone de programmation dédiée
			if (agentEdit.editState == AgentEdit.EditMode.Locked || agentEdit.editState == AgentEdit.EditMode.Synch || script != null)
				UISystem.instance.addSpecificContainer(agentEdit.agentName, agentEdit.editState, script);

			// Chargement de l'icône de l'agent sur la localisation
			containerParent.transform.Find("Header").Find("locateButton").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/robotIcon", typeof(Sprite)) as Sprite;
			// Affichage du nom de l'agent
			containerParent.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = entity.GetComponent<AgentEdit>().agentName;
			// Si on autorise le changement de nom on dévérouille la possibilité d'écrire dans la zone de nom du robot
			if (entity.GetComponent<AgentEdit>().editState != AgentEdit.EditMode.Locked)
			{
				containerParent.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().interactable = true;
			}
		}
		else if (type == "enemy")
		{
			// Chargement de l'icône de l'agent sur la localisation
			containerParent.transform.Find("Header").Find("locateButton").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/droneIcon", typeof(Sprite)) as Sprite;
			// Affichage du nom de l'agent
			containerParent.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text = "Drone";

			if (script != null)
			{
				GameObject targetContainer = entity.GetComponent<ScriptRef>().scriptContainer;
				foreach (GameObject go in script)
				{
					go.transform.SetParent(targetContainer.transform); //add actions to container
					List<GameObject> basicActionGO = getBasicActionGO(go);
					foreach (GameObject baGO in basicActionGO)
					{
						baGO.GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().droneAction;
						if (baGO.GetComponent<Selectable>() != null)
							baGO.GetComponent<Selectable>().interactable = false;
					}
				}
				//TODO : calculer les firstchild, stringifier les conditions...
				computeNext(scriptref.scriptContainer);
			}
		}

		AgentColor ac = MainLoop.instance.GetComponent<AgentColor>();
		scriptref.uiContainer.transform.Find("Scroll View").GetComponent<Image>().color = (type == "player" ? ac.playerBackground : ac.droneBackground);

		containerParent.SetActive(false);
		GameObjectManager.bind(containerParent);
		GameObjectManager.bind(entity);
		return entity;
	}

	private List<GameObject> getBasicActionGO(GameObject go){
		List<GameObject> res = new List<GameObject>();
		if(go.GetComponent<BasicAction>())
			res.Add(go);
		foreach(Transform child in go.transform){
			if(child.GetComponent<BasicAction>())
				res.Add(child.gameObject);
			else {
					List<GameObject> childGO = getBasicActionGO(child.gameObject); 
					foreach(GameObject cgo in childGO){
						res.Add(cgo);
					}
				}		
		}
		return res;
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
		foreach( GameObject go in levelGO){
			GameObjectManager.unbind(go.gameObject);
			Object.Destroy(go.gameObject);
		}
	}

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
					createEntity(int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posZ").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value),"player", readXMLScript(child.ChildNodes[0]));
					break;
				
				case "enemy":
					GameObject enemy = createEntity(int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posZ").Value),
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
        GameObjectManager.addComponent<GameLoaded>(MainLoop.instance.gameObject);
	}

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
		string actionName = null;
		foreach(XmlNode limitNode in limitsNode.ChildNodes){
			//gameData.actionBlocLimit.Add(int.Parse(limitNode.Attributes.GetNamedItem("limit").Value));
			actionName = limitNode.Attributes.GetNamedItem("actionType").Value;
			if (!gameData.actionBlocLimit.ContainsKey(actionName)){
				gameData.actionBlocLimit[actionName] = int.Parse(limitNode.Attributes.GetNamedItem("limit").Value);
			}
		}
	}

	private void readXMLActivable(XmlNode activableNode){
		List<int> slotsID = new List<int>();

		foreach(XmlNode child in activableNode.ChildNodes){
			slotsID.Add(int.Parse(child.Attributes.GetNamedItem("slot").Value));
		}

		createActivable(int.Parse(activableNode.Attributes.GetNamedItem("posX").Value), int.Parse(activableNode.Attributes.GetNamedItem("posZ").Value),
		 slotsID, (Direction.Dir)int.Parse(activableNode.Attributes.GetNamedItem("direction").Value));
	}

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

	// Transforme le noeux d'action XML en gameObject
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
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("If"));

						conditionContainer = obj.transform.Find("ConditionContainer");
						firstContainerBloc = obj.transform.Find("Container");
						action = obj.GetComponent<IfControl>();

						// On ajoute les éléments enfant dans les bons container
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
									DragDropSystem.instance.addItemOnDropArea(child, emptyZone);
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
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("IfElse"));
						conditionContainer = obj.transform.Find("ConditionContainer");
						firstContainerBloc = obj.transform.Find("Container");
						secondContainerBloc = obj.transform.Find("ElseContainer");
						action = obj.GetComponent<IfControl>();

						// On ajoute les éléments enfant dans les bons container
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
									DragDropSystem.instance.addItemOnDropArea(child, emptyZone);
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
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("For"));
						firstContainerBloc = obj.transform.Find("Container");
						action = obj.GetComponent<ForControl>();

						((ForControl)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
						obj.transform.GetComponentInChildren<TMP_InputField>().text = ((ForControl)action).nbFor.ToString();

						if (actionNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, actionNode);
						break;

					case "While":
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("While"));
						firstContainerBloc = obj.transform.Find("Container");
						conditionContainer = obj.transform.Find("ConditionContainer");
						action = obj.GetComponent<WhileControl>();

						// On ajoute les éléments enfant dans les bons container
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
									DragDropSystem.instance.addItemOnDropArea(child, emptyZone);
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
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("Forever"));
						firstContainerBloc = obj.transform.Find("Container");

						if (actionNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, actionNode);
						break;
				}
				break;
			case "action":
				obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find(actionNode.Attributes.GetNamedItem("type").Value));
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
				DragDropSystem.instance.addItemOnDropArea(child, emptySlot);
				firstchild = false;
			}
			else // add next childs to the dropZone
				DragDropSystem.instance.addItemOnDropArea(child, dropZone);
		}
	}

	// Transforme le noeux d'action XML en gameObject élément/opérator
	private GameObject readXMLCondition(XmlNode conditionNode) {
		GameObject obj = null;
		ReplacementSlot[] slots = null;
		switch (conditionNode.Name)
        {
			case "operator":
				switch (conditionNode.Attributes.GetNamedItem("type").Value)
                {
					case "AndOperator":
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("AndOperator"));
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
									DragDropSystem.instance.addItemOnDropArea(child, emptyZone);
								}
								emptyZone = null;
							}
						}
						break;

					case "OrOperator":
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("OrOperator"));
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
									DragDropSystem.instance.addItemOnDropArea(child, emptyZone);
								}
								emptyZone = null;
							}
						}
						break;

					case "NotOperator":
						obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find("NotOperator"));
						if (conditionNode.HasChildNodes)
						{
							GameObject emptyZone = obj.transform.Find("Container").GetChild(1).gameObject;
							GameObject child = readXMLCondition(conditionNode.FirstChild);
							// Add child to empty zone
							DragDropSystem.instance.addItemOnDropArea(child, emptyZone);
						}
						break;
				}
				break;
			case "captor":
				obj = DragDropSystem.instance.createEditableBlockFromLibrary(GameObject.Find(conditionNode.Attributes.GetNamedItem("type").Value));
				break;
        }

		return obj;
	}

	// link actions together => define next property
	// Associe à chaque bloc le bloc qui sera executé aprés
	public static void computeNext(GameObject container){
		for (int i = 0 ; i < container.transform.childCount ; i++){
			Transform child = container.transform.GetChild(i);
			// Si l'action est une action basique et n'est pas la dernière
			if (i < container.transform.childCount && child.GetComponent<BaseElement>()){
				// Si le bloc appartient à un for, il faut que le dernier élément ait comme next le block for
				if ((container.transform.parent.GetComponent<ForeverControl>() || container.transform.parent.GetComponent<ForControl>()) && i == container.transform.childCount - 1)
				{
					child.GetComponent<BaseElement>().next = container.transform.parent.gameObject;
					i = container.transform.childCount;
				}// Si le bloc appartient à un if et qu'il est le dernier block de la partie action
				else if (container.transform.parent.GetComponent<IfControl>() && i == container.transform.childCount - 1) {
					// On regarde si il reste des éléments dans le container parent
					// Si oui on met l'élément suivant en next
					// Sinon on ne fait rien et fin de la sequence
					if(container.transform.parent.parent.childCount - 1 > container.transform.parent.GetSiblingIndex())
                    {
						child.GetComponent<BaseElement>().next = container.transform.parent.parent.GetChild(container.transform.parent.GetSiblingIndex() + 1).gameObject;
					}
                    else
                    {
						// Exception, si le container parent parent est un for, on le met en next
						if (container.transform.parent.parent.parent.GetComponent<ForControl>() || container.transform.parent.parent.parent.GetComponent<ForeverControl>())
                        {
							child.GetComponent<BaseElement>().next = container.transform.parent.parent.parent.gameObject;
						}
                    }
				}// Sinon l'associer au block suivant
				else if (i != container.transform.childCount - 1)
				{
					child.GetComponent<BaseElement>().next = container.transform.GetChild(i + 1).gameObject;
				}
			}// Sinon si c'est la derniére et une action basique
			else if(i == container.transform.childCount-1 && child.GetComponent<BaseElement>() && container.GetComponent<BaseElement>()){
				if(container.GetComponent<ForControl>() || container.GetComponent<ForeverControl>())
					child.GetComponent<BaseElement>().next = container;
				else if(container.GetComponent<IfControl>())
					child.GetComponent<BaseElement>().next = container.GetComponent<BaseElement>().next;
			}
			// Si autre action que les actions basique
			// Alors récursive de la fonction sur leur container
			if(child.GetComponent<IfControl>() || child.GetComponent<ForControl>())
            {
				computeNext(child.transform.Find("Container").gameObject);
                // Si c'est un else il ne faut pas oublier le container else
                if (child.GetComponent<IfElseControl>())
                {
					computeNext(child.transform.Find("ElseContainer").gameObject);
				}
			}
			else if (child.GetComponent<ForeverControl>())
            {
				computeNext(child.transform.Find("Container").gameObject);
			}
		}
	}
}
