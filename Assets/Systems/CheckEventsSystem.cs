using UnityEngine;
using FYFY;
using System.Collections;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;
public class CheckEventsSystem : FSystem {

	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script),typeof(Position),typeof(HighLight),typeof(Direction), typeof(Animator), typeof(AudioSource), typeof(TriggerSensitive3D), typeof(CapsuleCollider)), new AnyOfTags("Player"));
	private Family scriptedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script), typeof(Position), typeof(Direction), typeof(HighLight)));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(MeshRenderer), typeof(MeshFilter), typeof(AudioSource)), new AnyOfTags("Exit"));
	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(BoxCollider), typeof(MeshRenderer)), new AnyOfTags("Wall"));
	private Family activableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position), typeof(MeshRenderer), typeof(AudioSource)));
	private Family activationSlotGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position),typeof(Direction), typeof(BoxCollider)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family endpanel_f = FamilyManager.getFamily(new AllOfComponents(typeof(Image), typeof(AudioSource)), new AnyOfTags("endpanel"));

    private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));
    private GameData gameData;
    private GameObject endPanel;

    public CheckEventsSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
        endPanel = endpanel_f.First();
        GameObjectManager.setGameObjectState(endPanel, false);
        newStep_f.addEntryCallback(onNewStep);
        robotcollision_f.addEntryCallback(onNewCollision);
    }

    private void onNewStep(GameObject unused)
    {
        //Check if the player is on the end of the level
        int nbEnd = 0;
        foreach (GameObject player in playerGO)
        {
            foreach (GameObject exit in exitGO)
            {
                if (gameData.nbStep == 1 && player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().z == exit.GetComponent<Position>().z)
                {
                    nbEnd++;
                    //end level
                    if (nbEnd >= playerGO.Count)
                    {
                        Debug.Log("Fin du niveau");
                        GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Win });
                    }
                }
            }
        }

        //Check Activations
        foreach (GameObject activable in activableGO)
        {
            if (activable.GetComponent<Activable>().isActivated && !activable.GetComponent<Activable>().isFullyActivated)
            {
                activate(activable);
            }
        }

        //Check if If actions are valid
        int nbStepToAdd = 0;
        foreach (GameObject scripted in scriptedGO)
        {
            int nbStepPlayer = 0;
            ActionManipulator.invalidAllIf(scripted.GetComponent<Script>());
            Action nextIf = ActionManipulator.getCurrentIf(scripted);

            while (nextIf != null && !ActionManipulator.endOfScript(scripted))
            {
                //Check if ok
                bool ifok = nextIf.ifNot;
                Vector2 vec = new Vector2();
                switch (ActionManipulator.getDirection(scripted.GetComponent<Direction>().direction, nextIf.ifDirection))
                {
                    case Direction.Dir.North:
                        vec = new Vector2(0, 1);
                        break;
                    case Direction.Dir.South:
                        vec = new Vector2(0, -1);
                        break;
                    case Direction.Dir.East:
                        vec = new Vector2(1, 0);
                        break;
                    case Direction.Dir.West:
                        vec = new Vector2(-1, 0);
                        break;
                }

                switch (nextIf.ifEntityType)
                {
                    case 0:
                        for (int i = 1; i <= nextIf.range; i++)
                        {
                            foreach (GameObject go in wallGO)
                            {
                                if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i)
                                {
                                    ifok = !nextIf.ifNot;
                                }
                            }
                        }
                        break;
                    case 1:
                        for (int i = 1; i <= nextIf.range; i++)
                        {
                            foreach (GameObject go in scriptedGO)
                            {
                                if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i
                                 && go.tag == "Ennemy")
                                {
                                    ifok = !nextIf.ifNot;
                                }
                            }
                        }
                        break;
                    case 2:
                        for (int i = 1; i <= nextIf.range; i++)
                        {
                            foreach (GameObject go in scriptedGO)
                            {
                                if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i
                                 && go.tag == "Player")
                                {
                                    ifok = !nextIf.ifNot;
                                }
                            }
                        }
                        break;
                }

                if (ifok)
                {
                    nextIf.ifValid = true;
                    if (scripted.tag == "Player")
                        nbStepPlayer += ActionManipulator.getNbStep(nextIf, true);
                }
                else
                {
                    nextIf.currentAction = nextIf.actions.Count - 1;
                    ActionManipulator.incrementActionScript(scripted.GetComponent<Script>());
                }
                nextIf = ActionManipulator.getCurrentIf(scripted);
            }

            if (nbStepPlayer > nbStepToAdd)
            {
                nbStepToAdd = nbStepPlayer;
            }
        }
        gameData.nbStep += nbStepToAdd;

    }

    private void onNewCollision(GameObject robot){
        Triggered3D trigger = robot.GetComponent<Triggered3D>();
        foreach(GameObject target in trigger.Targets){
            //Check if the player collide with a detection cell
            if (target.GetComponent<Detector>() != null){
                //end level
                Debug.Log("Repéré !");
                GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Detected });
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
