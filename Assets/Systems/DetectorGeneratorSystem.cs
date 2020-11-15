using UnityEngine;
using FYFY;

public class DetectorGeneratorSystem : FSystem {

	private Family detectRangeGO = FamilyManager.getFamily(new AnyOfComponents(typeof(DetectRange)));
	private Family detectorGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Detector)));
	private Family entityGO = FamilyManager.getFamily(new AllOfComponents(typeof(Entity)));
	private GameData gameData;

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	public DetectorGeneratorSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		
		if(gameData.generateStep || gameData.initialize){
			//Destroy detection cells
			foreach( GameObject detector in detectorGO){
				GameObjectManager.unbind(detector);
				Object.Destroy(detector);
			}

			bool stop = false;
			//Generate detection cells
			foreach( GameObject detect in detectRangeGO){
				switch(detect.GetComponent<DetectRange>().type){
					//Line type
					case DetectRange.Type.Line:
						switch(detect.GetComponent<Direction>().direction){
							case Direction.Dir.North:
								stop = false;
								for(int i = 0; i < detect.GetComponent<DetectRange>().range; i++){
									int x = detect.GetComponent<Position>().x;
									int z = detect.GetComponent<Position>().z + i + 1;
									foreach(GameObject wall in entityGO){
										if(wall.GetComponent<Entity>().type == Entity.Type.Wall && wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z){
											stop = true;
										}
									}
									if(stop){
										break;
									}
									else{
										GameObject obj = Object.Instantiate (Resources.Load ("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x*3,1.5f,z*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
										obj.GetComponent<Position>().x = x;
										obj.GetComponent<Position>().z = z;
										obj.GetComponent<Detector>().owner = detect;
										GameObjectManager.bind(obj);
									}
								}
								break;
							case Direction.Dir.West:
								stop = false;
								for(int i = 0; i < detect.GetComponent<DetectRange>().range; i++){
									int x = detect.GetComponent<Position>().x - i - 1;
									int z = detect.GetComponent<Position>().z;
									foreach(GameObject wall in entityGO){
										if(wall.GetComponent<Entity>().type == Entity.Type.Wall && wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z){
											stop = true;
										}
									}
									if(stop){
										break;
									}
									else{
										GameObject obj = Object.Instantiate (Resources.Load ("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x*3,1.5f,z*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
										obj.GetComponent<Position>().x = x;
										obj.GetComponent<Position>().z = z;
										obj.GetComponent<Detector>().owner = detect;
										GameObjectManager.bind(obj);
									}
								}
								break;
							case Direction.Dir.South:
								stop = false;
								for(int i = 0; i < detect.GetComponent<DetectRange>().range; i++){
									int x = detect.GetComponent<Position>().x;
									int z = detect.GetComponent<Position>().z - i - 1;
									foreach(GameObject wall in entityGO){
										if(wall.GetComponent<Entity>().type == Entity.Type.Wall && wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z){
											stop = true;
										}
									}
									if(stop){
										break;
									}
									else{
										GameObject obj = Object.Instantiate (Resources.Load ("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x*3,1.5f,z*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
										obj.GetComponent<Position>().x = x;
										obj.GetComponent<Position>().z = z;
										obj.GetComponent<Detector>().owner = detect;
										GameObjectManager.bind(obj);
									}
								}
								break;
							case Direction.Dir.East:
								stop = false;
								for(int i = 0; i < detect.GetComponent<DetectRange>().range; i++){
									int x = detect.GetComponent<Position>().x + i + 1;
									int z = detect.GetComponent<Position>().z;
									foreach(GameObject wall in entityGO){
										if(wall.GetComponent<Entity>().type == Entity.Type.Wall && wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z){
											stop = true;
										}
									}
									if(stop){
										break;
									}
									else{
										GameObject obj = Object.Instantiate (Resources.Load ("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x*3,1.5f,z*3), Quaternion.Euler(0,0,0), gameData.Level.transform);
										obj.GetComponent<Position>().x = x;
										obj.GetComponent<Position>().z = z;
										obj.GetComponent<Detector>().owner = detect;
										GameObjectManager.bind(obj);
									}
								}
								break;
							}
						break;
					case DetectRange.Type.Cone:
						break;
					case DetectRange.Type.Around:
						break;
				}
			}
		}
	}
}