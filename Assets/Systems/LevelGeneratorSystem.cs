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
		if(gameData.levelToLoad == 0)
			generateLevel1();
		else if(gameData.levelToLoad == 1)
			generateLevel2();
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
		


	}

	private void generateLevel2(){
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
	}

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

	private void createEntity(int i, int j, Direction.Dir direction, int type, List<Action> script = null, bool repeat = false){
		GameObject entity = null;
		switch(type){
			case 0:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Player") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
				break;
			case 1:
				entity = Object.Instantiate<GameObject>(Resources.Load ("Prefabs/Ennemy") as GameObject, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
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