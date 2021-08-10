using UnityEngine;
using FYFY;
using System.Collections;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;

/// <summary>
/// Manage detector areas
/// </summary>
public class DetectorManager : FSystem {

	private Family ennemyGO = FamilyManager.getFamily(new AllOfComponents(typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));
	private Family detectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Detector), typeof(Position), typeof(Rigidbody)));
	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
    private Family gameLoaded_f = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded), typeof(MainLoop)));
    private Family newStep_f = FamilyManager.getFamily(new AnyOfComponents(typeof(NewStep), typeof(FirstStep)));
    private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));
    private Family endpanel_f = FamilyManager.getFamily(new AllOfComponents(typeof(Image), typeof(AudioSource)), new AnyOfTags("endpanel"));
    private GameData gameData;
    private bool activeRedDetector;

	public DetectorManager()
    {
        if (Application.isPlaying)
        {
            activeRedDetector = true;
            gameData = GameObject.Find("GameData").GetComponent<GameData>();
            gameLoaded_f.addEntryCallback(delegate { updateDetector(); });
            newStep_f.addEntryCallback(delegate { updateDetector(); });
            robotcollision_f.addEntryCallback(onNewCollision);
        }
    }
    private void onNewCollision(GameObject robot){
		if(activeRedDetector){
			Triggered3D trigger = robot.GetComponent<Triggered3D>();
			foreach(GameObject target in trigger.Targets){
				//Check if the player collide with a detection cell
				if (target.GetComponent<Detector>() != null){
					//end level
					GameObjectManager.addComponent<NewEnd>(endpanel_f.First(), new { endType = NewEnd.Detected });
				}
			}			
		}
    }

    // See ExecuteButton, StopButton and ReloadState buttons in editor
	public void detectCollision(bool on){
		activeRedDetector = on;
    }

    // See StopButton and ReloadState buttons in editor
    public void updateDetector()
    {
        MainLoop.instance.StartCoroutine(delayUpdateDetector());
    }

    private IEnumerator delayUpdateDetector(){
        yield return null; // On NewStep currentAction is moved to the next action
        yield return null; // Following frame CurrentAction is available in families and action is executed (for exemple drone rotation)
        // then we can update dectetors

        //Destroy detection cells
        foreach (GameObject detector in detectorGO)
        {
            // Reset positions (because GameObject is not destroyed immediate)
            Position pos = detector.GetComponent<Position>();
            pos.x = -1;
            pos.z = -1;
            GameObjectManager.unbind(detector);
            Object.Destroy(detector);
        }

        bool stop = false;
        //Generate detection cells
        foreach (GameObject detect in ennemyGO)
        {
            switch (detect.GetComponent<DetectRange>().type)
            {
                //Line type
                case DetectRange.Type.Line:
                    if (detect.GetComponent<DetectRange>().selfRange)
                    {
                        GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(detect.GetComponent<Position>().x * 3, 1.5f, detect.GetComponent<Position>().z * 3), Quaternion.Euler(0, 0, 0), gameData.Level.transform);
                        obj.GetComponent<Position>().x = detect.GetComponent<Position>().x;
                        obj.GetComponent<Position>().z = detect.GetComponent<Position>().z;
                        obj.GetComponent<Detector>().owner = detect;
                        GameObjectManager.bind(obj);
                    }
                    switch (detect.GetComponent<Direction>().direction)
                    {
                        case Direction.Dir.North:
                            stop = false;
                            for (int i = 0; i < detect.GetComponent<DetectRange>().range; i++)
                            {
                                int x = detect.GetComponent<Position>().x;
                                int z = detect.GetComponent<Position>().z + i + 1;
                                foreach (GameObject wall in wallGO)
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                        stop = true;
                                if (stop)
                                    break;
                                else
                                {
                                    GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x * 3, 1.5f, z * 3), Quaternion.Euler(0, 0, 0), gameData.Level.transform);
                                    obj.GetComponent<Position>().x = x;
                                    obj.GetComponent<Position>().z = z;
                                    obj.GetComponent<Detector>().owner = detect;
                                    GameObjectManager.bind(obj);
                                }
                            }
                            break;
                        case Direction.Dir.West:
                            stop = false;
                            for (int i = 0; i < detect.GetComponent<DetectRange>().range; i++)
                            {
                                int x = detect.GetComponent<Position>().x - i - 1;
                                int z = detect.GetComponent<Position>().z;
                                foreach (GameObject wall in wallGO)
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                        stop = true;
                                if (stop)
                                    break;
                                else
                                {
                                    GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x * 3, 1.5f, z * 3), Quaternion.Euler(0, 0, 0), gameData.Level.transform);
                                    obj.GetComponent<Position>().x = x;
                                    obj.GetComponent<Position>().z = z;
                                    obj.GetComponent<Detector>().owner = detect;
                                    GameObjectManager.bind(obj);
                                }
                            }
                            break;
                        case Direction.Dir.South:
                            stop = false;
                            for (int i = 0; i < detect.GetComponent<DetectRange>().range; i++)
                            {
                                int x = detect.GetComponent<Position>().x;
                                int z = detect.GetComponent<Position>().z - i - 1;
                                foreach (GameObject wall in wallGO)
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                        stop = true;
                                if (stop)
                                    break;
                                else
                                {
                                    GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x * 3, 1.5f, z * 3), Quaternion.Euler(0, 0, 0), gameData.Level.transform);
                                    obj.GetComponent<Position>().x = x;
                                    obj.GetComponent<Position>().z = z;
                                    obj.GetComponent<Detector>().owner = detect;
                                    GameObjectManager.bind(obj);
                                }
                            }
                            break;
                        case Direction.Dir.East:
                            stop = false;
                            for (int i = 0; i < detect.GetComponent<DetectRange>().range; i++)
                            {
                                int x = detect.GetComponent<Position>().x + i + 1;
                                int z = detect.GetComponent<Position>().z;
                                foreach (GameObject wall in wallGO)
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                        stop = true;
                                if (stop)
                                    break;
                                else
                                {
                                    GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(x * 3, 1.5f, z * 3), Quaternion.Euler(0, 0, 0), gameData.Level.transform);
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