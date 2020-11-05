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
		generateLevel1();
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
									new List<int>{1,2,1,0,0,0,0,0,0,1},
									new List<int>{1,0,1,0,1,0,0,0,0,1},
									new List<int>{1,0,1,0,1,0,0,1,0,1},
									new List<int>{1,0,0,0,1,0,0,0,0,1},
									new List<int>{1,0,1,1,1,0,0,0,0,1},
									new List<int>{1,0,0,0,1,0,0,1,0,1},
									new List<int>{1,0,1,0,1,0,0,1,3,1},
									new List<int>{1,1,1,1,1,1,1,1,1,1}};

		generateMap();

		createPlayer(1,1, Direction.Dir.North);
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

	private void createPlayer(int i, int j, Direction.Dir direction){
		GameObject player = Object.Instantiate<GameObject>(gameData.PlayerPrefab, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
		player.GetComponent<Position>().x = i;
		player.GetComponent<Position>().z = j;
		player.GetComponent<Direction>().direction = direction;
		player.GetComponent<MoveTarget>().x = i;
		player.GetComponent<MoveTarget>().z = j;
		GameObjectManager.bind(player);
	}

	private void createSpawnExit(int i, int j, bool type){
		GameObject spawnExit;
		if(type)
			spawnExit = Object.Instantiate<GameObject>(gameData.SpawnPrefab, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
		else
			spawnExit = Object.Instantiate<GameObject>(gameData.ExitPrefab, gameData.Level.transform.position + new Vector3(i*3,1.5f,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);

		spawnExit.GetComponent<Position>().x = i;
		spawnExit.GetComponent<Position>().z = j;
		GameObjectManager.bind(spawnExit);
	}

	private void createCell(int i, int j){
		GameObject cell = Object.Instantiate<GameObject>(gameData.CasePrefab, gameData.Level.transform.position + new Vector3(i*3,0,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
		GameObjectManager.bind(cell);
	}

	private void createWall(int i, int j){
		GameObject wall = Object.Instantiate<GameObject>(gameData.WallPrefab, gameData.Level.transform.position + new Vector3(i*3,3,j*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
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
}