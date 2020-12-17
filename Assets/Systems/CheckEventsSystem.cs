using UnityEngine;
using FYFY;
using System.Collections;

public class CheckEventsSystem : FSystem {

	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)), new AnyOfTags("Player"));
	private Family scriptedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)));
	private Family noPlayerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)), new NoneOfTags("Player"));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Exit"));
	private Family entityGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(Entity)));
	private Family detectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Detector)));
	private Family activableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable)));
	private Family activationSlotGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot)));
	private GameData gameData;

	public CheckEventsSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	}

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if(gameData.checkStep){
			//Check if the player is on the end of the level
			int nbEnd = 0;
			foreach( GameObject player in playerGO){
				foreach( GameObject exit in exitGO){
					if(gameData.nbStep == 1 && player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().z == exit.GetComponent<Position>().z){
						nbEnd++;
						//end level
						if(nbEnd >= playerGO.Count){
							Debug.Log("Fin du niveau");
							gameData.endLevel = 2;
						}
					}
				}
			}

			//Check Activations
			foreach(GameObject activable in activableGO){
				if(activable.GetComponent<Activable>().isActivated && !activable.GetComponent<Activable>().isFullyActivated){
					activate(activable);
				}
			}

			//Check if If actions are valid
			int nbStepToAdd = 0;
			foreach( GameObject scripted in scriptedGO){
				int nbStepPlayer = 0;
				ActionManipulator.invalidAllIf(scripted.GetComponent<Script>());
				Action nextIf = ActionManipulator.getCurrentIf(scripted);

				while(nextIf != null && !ActionManipulator.endOfScript(scripted)){
					//Check if ok
					bool ifok = nextIf.ifNot;
					Vector2 vec = new Vector2();
					switch(ActionManipulator.getDirection(scripted.GetComponent<Direction>().direction,nextIf.ifDirection)){
						case Direction.Dir.North:
							vec = new Vector2(0,1);
							break;
						case Direction.Dir.South:
							vec = new Vector2(0,-1);
							break;
						case Direction.Dir.East:
							vec = new Vector2(1,0);
							break;
						case Direction.Dir.West:
							vec = new Vector2(-1,0);
							break;
					}

					switch(nextIf.ifEntityType){
						case 0:
							for(int i = 1; i <= nextIf.range; i++){
								foreach( GameObject go in entityGO){
									if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i
									 && go.GetComponent<Entity>().type == Entity.Type.Wall){
										ifok = !nextIf.ifNot;
									}
								}
							}
							break;
						case 1:
							for(int i = 1; i <= nextIf.range; i++){
								foreach( GameObject go in scriptedGO){
									if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i
									 && go.tag == "Ennemy"){
										ifok = !nextIf.ifNot;
									}
								}
							}
							break;
						case 2:
							for(int i = 1; i <= nextIf.range; i++){
								foreach( GameObject go in scriptedGO){
									if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i
									 && go.tag == "Player"){
										ifok = !nextIf.ifNot;
									}
								}
							}
							break;
					}

					if(ifok){
						nextIf.ifValid = true;
						if(scripted.tag == "Player")
							nbStepPlayer += ActionManipulator.getNbStep(nextIf, true);
					}
					else{
						nextIf.currentAction = nextIf.actions.Count-1;
						ActionManipulator.incrementActionScript(scripted.GetComponent<Script>());
					}
					nextIf = ActionManipulator.getCurrentIf(scripted);
				}

				if(nbStepPlayer > nbStepToAdd){
					nbStepToAdd = nbStepPlayer;
				}
			}
			gameData.nbStep += nbStepToAdd;
			

			foreach( GameObject player in playerGO){
				//Check if the player collide with a non-player
				/*foreach( GameObject noPlayer in noPlayerGO){
					if(player.GetComponent<Position>().x == noPlayer.GetComponent<Position>().x && player.GetComponent<Position>().z == noPlayer.GetComponent<Position>().z){
						//end level
						Debug.Log("Repéré !");
						gameData.endLevel = 1;
					}
				}*/

				//Check if the player collide with a detection cell
				foreach(GameObject detector in detectorGO){
					if(player.GetComponent<Position>().x == detector.GetComponent<Position>().x && player.GetComponent<Position>().z == detector.GetComponent<Position>().z){
						//end level
						Debug.Log("Repéré !");
						gameData.endLevel = 1;
					}
				}
			}
		}
	}

	private void activate(GameObject go){
		go.GetComponent<Activable>().isFullyActivated = true;
		foreach(int id in go.GetComponent<Activable>().slotID){
			foreach(GameObject slotGo in activationSlotGO){
				if(slotGo.GetComponent<ActivationSlot>().slotID == id){
					switch(slotGo.GetComponent<ActivationSlot>().type){
						case ActivationSlot.ActivationType.Destroy:
							MainLoop.instance.StartCoroutine(doorDestroy(slotGo));
							break;
					}
				}
			}
		}
	}

	private IEnumerator doorDestroy(GameObject go){

		yield return new WaitForSeconds(0.3f);

		go.GetComponent<Renderer>().enabled = false;
		go.GetComponent<AudioSource>().Play();
		
		yield return new WaitForSeconds(0.5f);
		GameObjectManager.unbind(go);
		Object.Destroy(go);
	}
}
