using UnityEngine;
using FYFY;
using System.Collections;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;

/// <summary>
/// Manage detector areas
/// </summary>
public class DetectorManager : FSystem {

	private Family enemyGO = FamilyManager.getFamily(new AllOfComponents(typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));
	private Family detectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Detector), typeof(Position), typeof(Rigidbody)));
	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
    private Family gameLoaded_f = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded), typeof(MainLoop)));
    private Family enemyMoved_f = FamilyManager.getFamily(new AnyOfComponents(typeof(Moved), typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));
    private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));

    private Family playingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family editingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

    private GameData gameData;
    private bool activeRedDetector;

    public GameObject endPanel;

    protected override void onStart()
    {
        activeRedDetector = false;
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();
        gameLoaded_f.addEntryCallback(delegate { updateDetectors(); });
        enemyMoved_f.addEntryCallback(updateDetector);
        robotcollision_f.addEntryCallback(onNewCollision);

        playingMode_f.addEntryCallback(delegate {
            activeRedDetector = true;
            updateDetectors();
        });
        editingMode_f.addEntryCallback(delegate {
            activeRedDetector = false;
            updateDetectors();
        });
    }

    private void onNewCollision(GameObject robot){
		if(activeRedDetector){
			Triggered3D trigger = robot.GetComponent<Triggered3D>();
			foreach(GameObject target in trigger.Targets){
				//Check if the player collide with a detection cell
				if (target.GetComponent<Detector>() != null){
					//end level
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Detected });
				}
			}			
		}
    }

    private void updateDetectors()
    {
        foreach (GameObject detect in enemyGO)
            updateDetector(detect);
    }

    private void updateDetector(GameObject drone)
    {
        foreach (Moved moved in drone.GetComponents<Moved>())
            GameObjectManager.removeComponent(moved);

        //Destroy detection cells
        foreach (GameObject detector in detectorGO)
        {
            if (detector.GetComponent<Detector>().owner == drone)
            {
                // Reset positions (because GameObject is not destroyed immediate)
                Position pos = detector.GetComponent<Position>();
                pos.x = -1;
                pos.z = -1;
                GameObjectManager.unbind(detector);
                Object.Destroy(detector);
            }
        }

        bool stop = false;
        //Generate detection cells
        DetectRange dr = drone.GetComponent<DetectRange>();
        Position drone_pos = drone.GetComponent<Position>();
        switch (dr.type)
        {
            //Line type
            case DetectRange.Type.Line:
                if (dr.selfRange)
                {
                    GameObject newRedArea = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.Level.transform.position + new Vector3(drone_pos.x * 3, 1.5f, drone_pos.z * 3), Quaternion.Euler(0, 0, 0), gameData.Level.transform);
                    newRedArea.GetComponent<Position>().x = drone_pos.x;
                    newRedArea.GetComponent<Position>().z = drone_pos.z;
                    newRedArea.GetComponent<Detector>().owner = drone;
                    GameObjectManager.bind(newRedArea);
                }
                switch (drone.GetComponent<Direction>().direction)
                {
                    case Direction.Dir.North:
                        stop = false;
                        for (int i = 0; i < dr.range; i++)
                        {
                            int x = drone_pos.x;
                            int z = drone_pos.z + i + 1;
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
                                obj.GetComponent<Detector>().owner = drone;
                                GameObjectManager.bind(obj);
                            }
                        }
                        break;
                    case Direction.Dir.West:
                        stop = false;
                        for (int i = 0; i < dr.range; i++)
                        {
                            int x = drone_pos.x - i - 1;
                            int z = drone_pos.z;
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
                                obj.GetComponent<Detector>().owner = drone;
                                GameObjectManager.bind(obj);
                            }
                        }
                        break;
                    case Direction.Dir.South:
                        stop = false;
                        for (int i = 0; i < dr.range; i++)
                        {
                            int x = drone_pos.x;
                            int z = drone_pos.z - i - 1;
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
                                obj.GetComponent<Detector>().owner = drone;
                                GameObjectManager.bind(obj);
                            }
                        }
                        break;
                    case Direction.Dir.East:
                        stop = false;
                        for (int i = 0; i < dr.range; i++)
                        {
                            int x = drone_pos.x + i + 1;
                            int z = drone_pos.z;
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
                                obj.GetComponent<Detector>().owner = drone;
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