using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

/// <summary>
/// Manage detector areas
/// </summary>
public class DetectorManager : FSystem {

	private Family f_enemy = FamilyManager.getFamily(new AllOfComponents(typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));
	private Family f_detector = FamilyManager.getFamily(new AllOfComponents(typeof(Detector), typeof(Position), typeof(Rigidbody)));
	private Family f_wall = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
    private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded), typeof(MainLoop)));
    private Family f_enemyMoved = FamilyManager.getFamily(new AllOfComponents(typeof(Moved)), new AnyOfComponents(typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));
    private Family f_robotcollision = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

    private GameData gameData;
    private bool activeRedDetector;

    protected override void onStart()
    {
        activeRedDetector = false;
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();
        f_gameLoaded.addEntryCallback(delegate { updateDetectors(); });
        f_enemyMoved.addEntryCallback(updateDetector);
        f_robotcollision.addEntryCallback(onNewCollision);

        f_playingMode.addEntryCallback(delegate {
            activeRedDetector = true;
            updateDetectors();
        });
        f_editingMode.addEntryCallback(delegate {
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
					GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Detected });
				}
			}			
		}
    }

    // Used by ReloadState button in inspector
    public void updateDetectors()
    {
        foreach (GameObject detect in f_enemy)
            updateDetector(detect);
    }

    // Reset detector positions depending on drone properties (position, orientation, range...)
    private void updateDetector(GameObject drone)
    {
        foreach (Moved moved in drone.GetComponents<Moved>())
            GameObjectManager.removeComponent(moved);

        //Destroy detection cells
        foreach (GameObject detector in f_detector)
        {
            if (detector.GetComponent<Detector>().owner == drone)
            {
                // Reset positions (because GameObject is not destroyed immediate)
                Position pos = detector.GetComponent<Position>();
                pos.x = -1;
                pos.y = -1;
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
                    GameObject newRedArea = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.LevelGO.transform.position + new Vector3(drone_pos.y * 3, 1.5f, drone_pos.x * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
                    newRedArea.GetComponent<Position>().x = drone_pos.x;
                    newRedArea.GetComponent<Position>().y = drone_pos.y;
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
                            int y = drone_pos.y - i - 1;
                            foreach (GameObject wall in f_wall)
                                if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().y == y)
                                    stop = true;
                            if (stop)
                                break;
                            else
                            {
                                GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.LevelGO.transform.position + new Vector3(y * 3, 1.5f, x * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
                                obj.GetComponent<Position>().x = x;
                                obj.GetComponent<Position>().y = y;
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
                            int y = drone_pos.y;
                            foreach (GameObject wall in f_wall)
                                if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().y == y)
                                    stop = true;
                            if (stop)
                                break;
                            else
                            {
                                GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.LevelGO.transform.position + new Vector3(y * 3, 1.5f, x * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
                                obj.GetComponent<Position>().x = x;
                                obj.GetComponent<Position>().y = y;
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
                            int y = drone_pos.y + i + 1;
                            foreach (GameObject wall in f_wall)
                                if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().y == y)
                                    stop = true;
                            if (stop)
                                break;
                            else
                            {
                                GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.LevelGO.transform.position + new Vector3(y * 3, 1.5f, x * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
                                obj.GetComponent<Position>().x = x;
                                obj.GetComponent<Position>().y = y;
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
                            int y = drone_pos.y;
                            foreach (GameObject wall in f_wall)
                                if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().y == y)
                                    stop = true;
                            if (stop)
                                break;
                            else
                            {
                                GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, gameData.LevelGO.transform.position + new Vector3(y * 3, 1.5f, x * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
                                obj.GetComponent<Position>().x = x;
                                obj.GetComponent<Position>().y = y;
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