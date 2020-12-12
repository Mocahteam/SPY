using UnityEngine;
using FYFY;
using System.Collections.Generic;

public class LevelGeneratorSystem : FSystem {

	private Family levelGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Position), typeof(HighLight)));
	private List<List<int>> map;
	private GameData gameData;

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	public LevelGeneratorSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.Level = GameObject.Find("Level");

		switch(gameData.levelToLoad){
			case 0:
				generateLevel1();
				break;
			case 1:
				generateLevel2();
				break;
			case 2:
				generateLevel3();
				break;
			case 3:
				generateLevel4();
				break;
			case 4:
				generateLevel5();
				break;
		}
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
		
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}

	
	private void generateLevel1(){
		eraseMap();
		map = new List<List<int>> {new List<int>{1,1,1,1,1},
									new List<int>{1,0,0,0,1},
									new List<int>{1,0,1,0,1},
									new List<int>{1,2,1,3,1},
									new List<int>{1,1,1,1,1}};
		generateMap();
		
		createEntity(3,1, Direction.Dir.West,0);

		

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
		gameData.dialogMessage.Add("Utilise l'action 'For', tu pourras y mettre d'autres actions dans cet ordre qui seront répétés le nombre de fois indiqué !\nMet y l'action Avancer et règle le For sur 12.");

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

	private GameObject createEntity(int i, int j, Direction.Dir direction, int type, List<Action> script = null, bool repeat = false){
		GameObject entity = null;
		switch(type){
			case 0:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Player") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
			case 1:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Ennemy") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
			case 2:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/SecurityCamera") as GameObject, gameData.Level.transform.position + new Vector3(i*3,5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
		}
		
		entity.GetComponent<Position>().x = i;
		entity.GetComponent<Position>().z = j;
		entity.GetComponent<Direction>().direction = direction;

		ActionManipulator.resetScript(entity.GetComponent<Script>());
		if(script != null){
			entity.GetComponent<Script>().actions = script;
		}

		entity.GetComponent<Script>().repeat = repeat;

		GameObjectManager.bind(entity);

		return entity;
	}

	private void createSpawnExit(int i, int j, bool type){
		GameObject spawnExit;
		if(type)
			spawnExit = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Spawn") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
		else
			spawnExit = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Exit") as GameObject, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);

		spawnExit.GetComponent<Position>().x = i;
		spawnExit.GetComponent<Position>().z = j;
		GameObjectManager.bind(spawnExit);
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

	public void reloadScene(){
		gameData.step = false;
		gameData.checkStep = false;
		gameData.generateStep = false;
		gameData.nbStep = 0;
		gameData.endLevel = 0;
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		GameObjectManager.loadScene("MainScene");
	}

	public void returnToTitleScreen(){
		GameObjectManager.loadScene("TitleScreen");
	}

	public void nextLevel(){
		gameData.levelToLoad++;
		reloadScene();
	}
}