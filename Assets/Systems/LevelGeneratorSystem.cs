using UnityEngine;
using FYFY;
using System.Collections.Generic;
using System.Xml;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;
using TMPro;
using FYFY_plugins.PointerManager;
//using System;

public class LevelGeneratorSystem : FSystem {

	private Family levelGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Position), typeof(CurrentAction)));
	//private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position),typeof(MeshRenderer), typeof(CapsuleCollider), typeof(AudioSource)), new AnyOfTags("Coin"));
	//private Family activableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(MeshRenderer), typeof(AudioSource), typeof(MeshFilter)));
	//private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Direction), typeof(MeshRenderer), typeof(BoxCollider), typeof(Position), typeof(AudioSource)), new AnyOfTags("Door"));
	//private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script),typeof(Position),typeof(HighLight),typeof(Direction), typeof(Animator), typeof(AudioSource), typeof(TriggerSensitive3D), typeof(CapsuleCollider)), new AnyOfTags("Player"));
	//private Family ennemyGO = FamilyManager.getFamily(new AllOfComponents(typeof(DetectRange), typeof(Script), typeof(Direction), typeof(Position), typeof(HighLight), typeof(AudioSource)), new AnyOfTags("Drone"));
	//private Family ennemyScript = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(Image)), new NoneOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));
	private Family ennemyScript = FamilyManager.getFamily(new AllOfComponents(typeof(HorizontalLayoutGroup), typeof(CanvasRenderer)), new NoneOfComponents(typeof(Image)));
	private List<List<int>> map;
	private GameData gameData;
	private GameObject scriptContainer;

	//private bool historyIsInScript = false;

	public LevelGeneratorSystem(){
		//Debug.Log("level constructor");
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.Level = GameObject.Find("Level");
		scriptContainer = ennemyScript.First();
		XmlToLevel(gameData.levelList[gameData.levelToLoad]);
		//gameData.currentLevelBlocLimits = gameData.actionBlocLimit;
		//generateLevel6();


	}

	/*
	public static void updateLimitFromActions(GameData gameData, Action action){
		ActionManipulator.updateActionBlocLimit(gameData, action.actionType, -1);
		if(action.actionType == Action.ActionType.For ||action.actionType == Action.ActionType.If){
			foreach(Action act in action.actions){
				//check children
				updateLimitFromActions(gameData, act);
			}
			
		}
	}
	public static void DisplayActionsInContainer(List<Action> actions, GameObject container){
		foreach(Action action in actions){
			GameObject go = ActionManipulator.ActionToContainer(action, false, false, true);
			GameObjectManager.bind(go);
			go.transform.SetParent(container.transform, false);
			GameData gameData = GameObject.Find("GameData").GetComponent<GameData>();
			updateLimitFromActions(gameData, action);

		}
	}
	*/

	// Use to process your families.
	/*
	protected override void onProcess(int familiesUpdateCount) {
		if (historyIsInScript == false){
			//Debug.Log("on process");
			Debug.Log("actionsHistory.count = "+gameData.actionsHistory.Count);
			//get back history in player script
			//
			Script scriptActions = new Script();
			scriptActions.actions = gameData.actionsHistory;
			scriptActions.currentAction = -1;
			scriptActions.repeat = false;
			//
			GameObject scriptComposer = playerScript.First();
			DisplayActionsInContainer(gameData.actionsHistory, scriptComposer);
			//remove from history
			gameData.actionsHistory.Clear();
			//history added to script ok
			historyIsInScript = true;
			}
	}*/

	/*
	private void generateLevel1(){
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1},
									new List<int>{1,0,0,0,1},
									new List<int>{1,0,1,0,1},
									new List<int>{1,2,1,3,1},
									new List<int>{1,1,1,1,1}};
		generateMap();

		//createCoin(1,1);
		createEntity(3,1, Direction.Dir.West,0);

		//createActivable(1,2,new List<int>(){0},Direction.Dir.North.GetHashCode());

		gameData.dialogMessage.Add("Bienvenu dans Spy !\n Votre objectif est de vous échapper en atteignant la sortie (cercle bleu)");
		gameData.dialogMessage.Add("Pour cela vous devez donner des ordres à votre agent en faisant glisser les actions en bas de l'écran jusqu'en haut à droite de la fenêtre de script");
		gameData.dialogMessage.Add("Vous pouvez utiliser le clique droit sur une action du script pour la supprimer, le bouton 'Reset' vous permet de vider la fenêtre de script d'un seul coup.");
		gameData.dialogMessage.Add("Vous pouvez déplacer la caméra avec ZQSD ou les fleches directionnelles");
		gameData.dialogMessage.Add("Essayez donc d'avancer 2 fois puis de tourner à droite pour commencer, cliquez ensuite sur 'Executer'. Essayez ensuite de terminer cette mission.");

		gameData.actionBlocLimit = new List<int>() {-1,-1,-1,-1,-1,-1};
	}

	private void generateLevel2(){
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1},
									new List<int>{1,0,1},
									new List<int>{1,3,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,0,1},
									new List<int>{1,2,1},
									new List<int>{1,1,1}};
		generateMap();
		
		createEntity(14,1, Direction.Dir.West,0);

		gameData.dialogMessage.Add("La sortie est au bout de ce couloir !");
		gameData.dialogMessage.Add("Evitons de de saturer la ligne de communication en donnant un ordre plus efficace");
		gameData.dialogMessage.Add("Utilise l'action 'For', tu pourras y mettre d'autres actions dans cet ordre qui seront répétés le nombre de fois indiqué !\nMet y l'action Avancer et règle le pour arriver sur la sortie.");

		gameData.actionBlocLimit = new List<int>() {1,-1,-1,-1,-1,-1};
	}

	private void generateLevel3(){
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1,1,1},
									new List<int>{1,1,1,3,1,1,1},
									new List<int>{1,1,0,0,0,1,1},
									new List<int>{1,0,0,0,0,1,1},
									new List<int>{1,1,0,0,0,1,1},
									new List<int>{1,1,0,0,0,0,1},
									new List<int>{1,1,0,0,0,1,1},
									new List<int>{1,1,1,2,1,1,1},
									new List<int>{1,1,1,1,1,1,1}};
		generateMap();
		
		createEntity(7,3, Direction.Dir.West,0);

		createEntity(3,1, Direction.Dir.North,2);
		createEntity(5,5, Direction.Dir.South,2);

		gameData.dialogMessage.Add("Attention il y a des caméras de sécurité ici, tu peux voir leur champ de vision en rouge. Faufile toi sans te faire repérer.");

		gameData.actionBlocLimit = new List<int>() {-1,-1,-1,-1,-1,-1};

	}

	private void generateLevel4(){
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1},
									new List<int>{1,0,0,3,1},
									new List<int>{1,0,1,1,1},
									new List<int>{1,2,1,1,1},
									new List<int>{1,1,1,1,1}};
		generateMap();
		
		createEntity(3,1, Direction.Dir.West,0);

		List<Action> script = new List<Action>();

		script.Add(ActionManipulator.createAction(Action.ActionType.Wait));
		script.Add(ActionManipulator.createAction(Action.ActionType.Wait));
		script.Add(ActionManipulator.createAction(Action.ActionType.TurnLeft));
		script.Add(ActionManipulator.createAction(Action.ActionType.Wait));
		script.Add(ActionManipulator.createAction(Action.ActionType.Wait));
		script.Add(ActionManipulator.createAction(Action.ActionType.TurnRight));

		GameObject camera = createEntity(1,1, Direction.Dir.East,2, script, true);
		camera.GetComponent<DetectRange>().range = 1;

		gameData.dialogMessage.Add("Attention il y a une caméra devant toi ! Par chance son champ de détection est très petit, mais elle te bloque tout de même le passage.");
		gameData.dialogMessage.Add("Il semblerait que cette caméra a une IA, clique dessus pour analyser son comportement pour y trouver une faille et passer. De plus ce modèle de caméra ne semble pas voir en dessous d'elle même.");

		gameData.actionBlocLimit = new List<int>() {-1,-1,-1,-1,-1,-1};
	}

	private void generateLevel5(){
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1,1,1},
									new List<int>{1,0,3,1,0,3,1},
									new List<int>{1,0,1,1,0,1,1},
									new List<int>{1,0,1,1,0,0,1},
									new List<int>{1,0,1,1,1,0,1},
									new List<int>{1,0,0,1,0,0,1},
									new List<int>{1,1,0,1,0,1,1},
									new List<int>{1,1,2,1,2,1,1},
									new List<int>{1,1,1,1,1,1,1}};
		generateMap();
		
		createEntity(7,2, Direction.Dir.West,0);
		createEntity(7,4, Direction.Dir.West,0);

		gameData.dialogMessage.Add("Dans cette mission vous avez deux agents que vous devez diriger vers la sortie. Malheureusement nous ne pouvons pas nous permettre d'utiliser plusieurs communications, ils recevront donc les même ordres.");
		gameData.dialogMessage.Add("Pour cela utilisez les particularités du terrain pour ammener les deux agents à la sortie.");

		gameData.actionBlocLimit = new List<int>() {-1,-1,-1,-1,-1,-1};
	}



	/*
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1,1,1,1,1,1},
									new List<int>{1,3,1,0,0,0,0,0,0,1},
									new List<int>{1,0,0,1,1,1,1,0,0,1},
									new List<int>{1,1,0,1,3,0,1,1,0,1},
									new List<int>{1,0,0,1,1,0,1,0,0,1},
									new List<int>{1,0,1,1,1,0,1,0,0,1},
									new List<int>{1,0,1,0,0,0,1,1,0,1},
									new List<int>{1,2,1,2,1,1,1,1,0,1},
									new List<int>{1,1,1,1,1,1,1,1,1,1}};

		generateMap();

		createEntity(7,1, Direction.Dir.West,0);
		createEntity(7,3, Direction.Dir.West,0);

		//////////////////////

		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1,1,1,1,1,1},
									new List<int>{1,2,0,0,0,0,0,0,0,1},
									new List<int>{1,0,1,0,1,0,0,0,0,1},
									new List<int>{1,0,1,0,1,0,0,1,0,1},
									new List<int>{1,0,0,0,1,0,0,0,0,1},
									new List<int>{1,0,1,1,1,0,0,0,0,1},
									new List<int>{1,0,0,0,1,0,0,1,0,1},
									new List<int>{1,0,1,0,1,0,0,1,3,1},
									new List<int>{1,1,1,1,1,1,1,1,1,1}};

		generateMap();

		createEntity(1,1, Direction.Dir.North,0);

		Action forAct = ActionManipulator.createAction(Action.ActionType.For,4);
		ActionManipulator.addAction(forAct,ActionManipulator.createAction(Action.ActionType.Forward));
		ActionManipulator.addAction(forAct,ActionManipulator.createAction(Action.ActionType.TurnLeft));
		List<Action> script = new List<Action> {forAct};
		createEntity(5,6,Direction.Dir.West,1, script, true);
	*/

	/*
	private void generateLevel6(){
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1,1,1,1},
									new List<int>{1,0,0,0,0,0,0,1},
									new List<int>{1,0,1,1,1,1,0,1},
									new List<int>{1,0,1,2,0,1,0,1},
									new List<int>{1,0,1,1,0,1,0,1},
									new List<int>{1,0,0,0,0,1,0,1},
									new List<int>{1,1,1,1,1,1,3,1},
									new List<int>{1,1,1,1,1,1,1,1}};
		generateMap();
		
		createEntity(3,3, Direction.Dir.North,0);

		//createCoin(1,6);

		//gameData.dialogMessage.Add("Attention il y a une caméra devant toi ! Par chance son champ de détection est très petit, mais elle te bloque tout de même le passage.");
		//gameData.dialogMessage.Add("Il semblerait que cette caméra a une IA, clique dessus pour analyser son comportement pour y trouver une faille et passer. De plus ce modèle de caméra ne semble pas voir en dessous d'elle même.");

		gameData.actionBlocLimit = new List<int>() {1, 0, 1, 0, 0, 1, 1};
	}
	*/

	private void generateMap(){
		for(int i = 0; i< map.Count; i++){
			for(int j = 0; j < map[i].Count; j++){
				switch (map[i][j]){
					case 0:
						createCell(i,j);
						break;
					case 1:
						createCell(i,j);
						createWall(i,j);
						break;
					case 2:
						createCell(i,j);
						createSpawnExit(i,j,true);
						break;
					case 3:
						createCell(i,j);
						createSpawnExit(i,j,false);
						break;
				}
			}
		}
	}

	private GameObject createEntity(int i, int j, Direction.Dir direction, int type, List<GameObject> script = null){
		GameObject entity = null;
		Sprite agentSpriteIcon = null;
		switch(type){
			case 0:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Robot Kyle") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				agentSpriteIcon =  Resources.Load("UI Images/robotIcon", typeof(Sprite)) as Sprite;
				break;
			/*
			case 1:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Ennemy") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
			*/
			case 2:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Drone") as GameObject, gameData.Level.transform.position + new Vector3(i*3,5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				agentSpriteIcon =  Resources.Load("UI Images/droneIcon", typeof(Sprite)) as Sprite;
				break;
		}
		entity.GetComponent<Position>().x = i;
		entity.GetComponent<Position>().z = j;
		entity.GetComponent<Direction>().direction = direction;
		/*
        Script entityScript = entity.GetComponent<Script>();
        if (script != null)
            entityScript.actions = script;
        else
            entityScript.actions = new List<Action>();
        entityScript.currentAction = 0;
        entityScript.repeat = repeat;
		*/

		//add new container to entity
		ScriptRef scriptref = entity.GetComponent<ScriptRef>();
		GameObject containerParent = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Container") as GameObject);
		scriptref.uiContainer = containerParent;
		scriptref.scriptContainer = containerParent.transform.Find("Container").Find("Viewport").Find("ScriptContainer").gameObject;
		containerParent.transform.SetParent(scriptContainer.gameObject.transform);
		containerParent.transform.Find("agent").GetComponent<Image>().sprite = agentSpriteIcon;

		if(script != null){
			//add actions to container
			foreach(GameObject go in script){
				go.transform.SetParent(scriptref.scriptContainer.transform);
			}
			addNext(scriptref.scriptContainer);
		}
		GameObjectManager.bind(containerParent);
		GameObjectManager.bind(entity);
		return entity;
	}

	private void createDoor(int i, int j, Direction.Dir orientation, int slotID){
		GameObject door = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Door") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
		if(orientation == Direction.Dir.West || orientation == Direction.Dir.East){
			door.transform.rotation = Quaternion.Euler(0,90,0);
		}

		door.GetComponent<ActivationSlot>().slotID = slotID;
		door.GetComponent<Position>().x = i;
		door.GetComponent<Position>().z = j;
		door.GetComponent<Direction>().direction = orientation;
		GameObjectManager.bind(door);
	}

	private void createActivable(int i, int j, List<int> slotIDs, int side){
		GameObject activable = null;
		switch(side){
			case 3:
				activable = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/ActivableConsole") as GameObject, gameData.Level.transform.position + new Vector3(i*3-0.8f,1.5f,j*3), Quaternion.Euler(0,90,0), gameData.Level.transform);
				break;
			case 2:
				activable = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/ActivableConsole") as GameObject, gameData.Level.transform.position + new Vector3(i*3+0.8f,1.5f,j*3), Quaternion.Euler(0,-90,0), gameData.Level.transform);
				break;
			case 1:
				activable = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/ActivableConsole") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3-0.8f), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
			case 0:
				activable = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/ActivableConsole") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3+0.8f), Quaternion.Euler(0,180,0), gameData.Level.transform);
				break;
		}

		activable.GetComponent<Activable>().slotID = slotIDs;
		activable.GetComponent<Activable>().isActivated = false;
		activable.GetComponent<Activable>().isFullyActivated = false;
		activable.GetComponent<Activable>().side = side;
		activable.GetComponent<Position>().x = i;
		activable.GetComponent<Position>().z = j;
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
			//go.transform.DetachChildren();
			GameObjectManager.unbind(go.gameObject);
			Object.Destroy(go.gameObject);
		}
	}

	/*
	public void levelToXML(string fileName){
		XmlDocument doc = new XmlDocument();

		XmlElement levelElement = doc.CreateElement("level");
		doc.AppendChild(levelElement);
		XmlElement mapElement = doc.CreateElement("map");

		foreach(List<int> line in map){
			XmlElement lineElement = doc.CreateElement("line");
			foreach(int row in line){
				XmlElement rowElement = doc.CreateElement("row");
				XmlAttribute rowAttribut = doc.CreateAttribute("value");
				rowAttribut.Value = row.ToString();
				rowElement.Attributes.Append(rowAttribut);
				lineElement.AppendChild(rowElement);
			}
			mapElement.AppendChild(lineElement);
		}
		levelElement.AppendChild(mapElement);

		
		foreach(string dialog in gameData.dialogMessage){
			XmlElement dialogElement = doc.CreateElement("dialogs");
			XmlAttribute dialogAttribut = doc.CreateAttribute("dialog");
			dialogAttribut.Value = dialog;
			dialogElement.Attributes.Append(dialogAttribut);
			levelElement.AppendChild(dialogElement);
		}

		XmlElement actionBlocLimitElement = doc.CreateElement("actionBlocLimit");
		foreach(int limit in gameData.actionBlocLimit){
			XmlElement limitElement = doc.CreateElement("limit");
			XmlAttribute limitAttribut = doc.CreateAttribute("limit");
			limitAttribut.Value = limit.ToString();
			limitElement.Attributes.Append(limitAttribut);
			actionBlocLimitElement.AppendChild(limitElement);
		}
		levelElement.AppendChild(actionBlocLimitElement);
		
		

		foreach(GameObject coin in coinGO){
			XmlElement coinElement = doc.CreateElement("coin");
			XmlAttribute posX = doc.CreateAttribute("posX");
			posX.Value = coin.GetComponent<Position>().x.ToString();
			coinElement.Attributes.Append(posX);
			XmlAttribute posZ = doc.CreateAttribute("posZ");
			posZ.Value = coin.GetComponent<Position>().z.ToString();
			coinElement.Attributes.Append(posZ);
			levelElement.AppendChild(coinElement);
		}

		foreach(GameObject activable in activableGO){
			XmlElement activableElement = doc.CreateElement("activable");
			XmlAttribute posX = doc.CreateAttribute("posX");
			posX.Value = activable.GetComponent<Position>().x.ToString();
			activableElement.Attributes.Append(posX);
			XmlAttribute posZ = doc.CreateAttribute("posZ");
			posZ.Value = activable.GetComponent<Position>().z.ToString();
			activableElement.Attributes.Append(posZ);
			XmlAttribute side = doc.CreateAttribute("side");
			side.Value = activable.GetComponent<Activable>().side.ToString();
			activableElement.Attributes.Append(side);
			foreach(int slotID in activable.GetComponent<Activable>().slotID){
				XmlElement slotElement = doc.CreateElement("slot");
				XmlAttribute slot = doc.CreateAttribute("slot");
				slot.Value = slotID.ToString();
				slotElement.Attributes.Append(slot);
				activableElement.AppendChild(slotElement);
			}
			levelElement.AppendChild(activableElement);
		}

		foreach(GameObject door in doorGO){
			XmlElement doorElement = doc.CreateElement("door");
			XmlAttribute posX = doc.CreateAttribute("posX");
			posX.Value = door.GetComponent<Position>().x.ToString();
			doorElement.Attributes.Append(posX);
			XmlAttribute posZ = doc.CreateAttribute("posZ");
			posZ.Value = door.GetComponent<Position>().z.ToString();
			doorElement.Attributes.Append(posZ);

			XmlAttribute slot = doc.CreateAttribute("slot");
			slot.Value = door.GetComponent<ActivationSlot>().slotID.ToString();
			doorElement.Attributes.Append(slot);

			XmlAttribute direction = doc.CreateAttribute("direction");
			direction.Value = door.GetComponent<Direction>().direction.GetHashCode().ToString();
			doorElement.Attributes.Append(direction);

			levelElement.AppendChild(doorElement);
		}

		foreach(GameObject player in playerGO){
			XmlElement playerElement = doc.CreateElement("player");
			XmlAttribute posX = doc.CreateAttribute("posX");
			posX.Value = player.GetComponent<Position>().x.ToString();
			playerElement.Attributes.Append(posX);
			XmlAttribute posZ = doc.CreateAttribute("posZ");
			posZ.Value = player.GetComponent<Position>().z.ToString();
			playerElement.Attributes.Append(posZ);
			XmlAttribute direction = doc.CreateAttribute("direction");
			direction.Value = player.GetComponent<Direction>().direction.GetHashCode().ToString();
			playerElement.Attributes.Append(direction);
			levelElement.AppendChild(playerElement);
		}

		foreach(GameObject ennemy in ennemyGO){
			XmlElement ennemyElement = doc.CreateElement("ennemy");
			XmlAttribute posX = doc.CreateAttribute("posX");
			posX.Value = ennemy.GetComponent<Position>().x.ToString();
			ennemyElement.Attributes.Append(posX);
			XmlAttribute posZ = doc.CreateAttribute("posZ");
			posZ.Value = ennemy.GetComponent<Position>().z.ToString();
			ennemyElement.Attributes.Append(posZ);
			XmlAttribute direction = doc.CreateAttribute("direction");
			direction.Value = ennemy.GetComponent<Direction>().direction.GetHashCode().ToString();
			ennemyElement.Attributes.Append(direction);

			XmlAttribute detectRange = doc.CreateAttribute("range");
			detectRange.Value = ennemy.GetComponent<DetectRange>().range.ToString();
			ennemyElement.Attributes.Append(detectRange);

			XmlAttribute selfRange = doc.CreateAttribute("selfRange");
			selfRange.Value = ennemy.GetComponent<DetectRange>().selfRange.ToString();
			ennemyElement.Attributes.Append(selfRange);

			XmlAttribute typeRange = doc.CreateAttribute("typeRange");
			typeRange.Value = ennemy.GetComponent<DetectRange>().type.GetHashCode().ToString();
			ennemyElement.Attributes.Append(typeRange);

			//Script
			ennemyElement.AppendChild(scriptToXML(doc, ennemy.GetComponent<Script>()));


			levelElement.AppendChild(ennemyElement);
		}



		XmlTextWriter writer = new XmlTextWriter(fileName + ".xml",null);
    	writer.Formatting = Formatting.Indented;
    	doc.Save(writer);

	}

	public XmlElement scriptToXML(XmlDocument doc, Script script){
		XmlElement scriptElement = doc.CreateElement("script");
		XmlAttribute repeat = doc.CreateAttribute("repeat");
		repeat.Value = script.repeat.ToString();
		scriptElement.Attributes.Append(repeat);

		foreach(Action act in script.actions){
			scriptElement.AppendChild(actionToXML(doc, act));
		}
		return scriptElement;
	}
	
	public XmlElement actionToXML(XmlDocument doc, Action action){
		XmlElement actionElement = doc.CreateElement("action");

		XmlAttribute actionType = doc.CreateAttribute("actionType");
		actionType.Value = action.actionType.GetHashCode().ToString();
		actionElement.Attributes.Append(actionType);

		XmlAttribute nbFor = doc.CreateAttribute("nbFor");
		nbFor.Value = action.nbFor.ToString();
		actionElement.Attributes.Append(nbFor);

		XmlAttribute ifDirection = doc.CreateAttribute("ifDirection");
		ifDirection.Value = action.ifDirection.ToString();
		actionElement.Attributes.Append(ifDirection);

		XmlAttribute ifEntityType = doc.CreateAttribute("ifEntityType");
		ifEntityType.Value = action.ifEntityType.ToString();
		actionElement.Attributes.Append(ifEntityType);

		XmlAttribute range = doc.CreateAttribute("range");
		range.Value = action.range.ToString();
		actionElement.Attributes.Append(range);

		XmlAttribute ifNot = doc.CreateAttribute("ifNot");
		ifNot.Value = action.ifNot.ToString();
		actionElement.Attributes.Append(ifNot);

		if(action.actions != null){
			foreach(Action act in action.actions){
				actionElement.AppendChild(actionToXML(doc, act));
			}
		}

		return actionElement;
	}
	*/

	public void XmlToLevel(string fileName){

		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.dialogMessage = new List<string>();
		gameData.actionBlocLimit = new Dictionary<string, int>();
		map = new List<List<int>>();

		XmlDocument doc = new XmlDocument();
		doc.Load(fileName);

		XmlNode root = doc.ChildNodes[1];
		foreach(XmlNode child in root.ChildNodes){
			switch(child.Name){
				case "map":
					readXMLMap(child);
					break;
				case "dialogs":
					gameData.dialogMessage.Add(child.Attributes.GetNamedItem("dialog").Value);
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
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value),0);
					break;
				
				case "ennemy":
					GameObject ennemy = createEntity(int.Parse(child.Attributes.GetNamedItem("posX").Value), int.Parse(child.Attributes.GetNamedItem("posZ").Value),
					(Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value),2, readXMLScript(child.ChildNodes[0]));
					ennemy.GetComponent<DetectRange>().range = int.Parse(child.Attributes.GetNamedItem("range").Value);
					ennemy.GetComponent<DetectRange>().selfRange = bool.Parse(child.Attributes.GetNamedItem("selfRange").Value);
					ennemy.GetComponent<DetectRange>().type = (DetectRange.Type)int.Parse(child.Attributes.GetNamedItem("typeRange").Value);
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
		 slotsID, int.Parse(activableNode.Attributes.GetNamedItem("side").Value));
	}

	private List<GameObject> readXMLScript(XmlNode scriptNode){
		List<GameObject> script = new List<GameObject>();
		foreach(XmlNode actionNode in scriptNode.ChildNodes){
			script.Add(readXMLAction(actionNode));
		}

		return script;
	}

	private GameObject readXMLAction(XmlNode actionNode){
		GameObject obj = null;
		BaseElement action = null;
		GameObject prefab = null;
		bool firstchild;

		string actionKey = actionNode.Attributes.GetNamedItem("actionType").Value;
		switch(actionKey){
			case "If" :
				prefab = Resources.Load ("Prefabs/IfDetectBloc") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("If");
				action = obj.GetComponent<IfAction>();
				//read xml
				((IfAction)action).ifDirection = int.Parse(actionNode.Attributes.GetNamedItem("ifDirection").Value);
				((IfAction)action).ifEntityType = int.Parse(actionNode.Attributes.GetNamedItem("ifEntityType").Value);
				((IfAction)action).range = int.Parse(actionNode.Attributes.GetNamedItem("range").Value);
				((IfAction)action).ifNot = bool.Parse(actionNode.Attributes.GetNamedItem("ifNot").Value);

				//add to gameobject
				obj.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().value = ((IfAction)action).ifEntityType;
				obj.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().value = ((IfAction)action).ifDirection;
				obj.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().text = ((IfAction)action).range.ToString();
				
				if(!((IfAction)action).ifNot)
					obj.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value = 0;
				else
					obj.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value = 1;

				//not interactable actions
				obj.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().interactable = false;
				obj.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().interactable = false;
				obj.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().interactable = false;
				obj.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().interactable = false;
				
				Object.Destroy(obj.GetComponent<UITypeContainer>());

				//add children
				firstchild = false;
				if(actionNode.HasChildNodes){
					foreach(XmlNode actNode in actionNode.ChildNodes){
						GameObject child = (readXMLAction(actNode));
						child.transform.SetParent(obj.transform);
						if(!firstchild){
							firstchild = true;
							((IfAction)action).firstChild = child;
						}
					}
				}
				break;
			
			case "For":
				prefab = Resources.Load ("Prefabs/ForBloc") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("For");
				action = obj.GetComponent<ForAction>();

				//read xml
				((ForAction)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
				
				//add to gameobject
				obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (((ForAction)action).currentFor).ToString() + " / " + ((ForAction)action).nbFor.ToString();
				obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().interactable = false;
				Object.Destroy(obj.GetComponent<UITypeContainer>());

				//add children
				firstchild = false;
				if(actionNode.HasChildNodes){
					foreach(XmlNode actNode in actionNode.ChildNodes){
						GameObject child = (readXMLAction(actNode));
						child.transform.SetParent(action.transform);
						if(!firstchild){
							firstchild = true;
							((ForAction)action).firstChild = child;
						}
					}	
				}
				break;
			
			default:

				prefab = Resources.Load ("Prefabs/"+actionKey+"ActionBloc") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find(actionKey);
				action = obj.GetComponent<BasicAction>();		

				break;
		}
		obj.GetComponent<UIActionType>().prefab = prefab;
		action.target = obj;
		Object.Destroy(obj.GetComponent<PointerSensitive>());
		return obj;
	}

	private void addNext(GameObject container){
		for(int i = 0 ; i < container.transform.childCount ; i++){
			Transform child = container.transform.GetChild(i);
			if(i < container.transform.childCount-1 && child.GetComponent<BaseElement>()){
				child.GetComponent<BaseElement>().next = container.transform.GetChild(i+1).gameObject;
			}
			else if(i == container.transform.childCount-1 && child.GetComponent<BaseElement>() && container.GetComponent<BaseElement>()){
				if(container.GetComponent<ForAction>()){
					child.GetComponent<BaseElement>().next = container;
				}
				else if(container.GetComponent<IfAction>()){
					child.GetComponent<BaseElement>().next = container.GetComponent<BaseElement>().next;
				}
				
			}
			//if or for action
			if(child.GetComponent<IfAction>() || child.GetComponent<ForAction>()){
				addNext(child.gameObject);
			}
		}
	}

}
