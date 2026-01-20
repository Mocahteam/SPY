using UnityEngine;
using FYFY;

/// <summary>
/// Manage detector areas
/// </summary>
public class DetectorManager : FSystem {

	private Family f_enemy = FamilyManager.getFamily(new AllOfComponents(typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));
	private Family f_detector = FamilyManager.getFamily(new AllOfComponents(typeof(Detector), typeof(Position), typeof(Rigidbody)));
	private Family f_viewBlocker = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall", "Door"));
    private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded), typeof(MainLoop)));
    private Family f_enemyMoved = FamilyManager.getFamily(new AllOfComponents(typeof(Moved)), new AnyOfComponents(typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

    public GameObject LevelGO;

    protected override void onStart()
    {
        f_gameLoaded.addEntryCallback(delegate { updateDetectors(); });
        f_enemyMoved.addEntryCallback(updateDetector);

        f_playingMode.addEntryCallback(delegate {
            updateDetectors();
        });
        f_editingMode.addEntryCallback(delegate {
            updateDetectors();
        });
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

        //Generate detection cells
        DetectRange dr = drone.GetComponent<DetectRange>();
        Position drone_pos = drone.GetComponent<Position>();
        // Create detector under drone
        if (dr.selfRange)
        {
            GameObject newRedArea = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, LevelGO.transform.position + new Vector3(drone_pos.y * 3, 1.5f, drone_pos.x * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);
            newRedArea.GetComponent<Position>().x = drone_pos.x;
            newRedArea.GetComponent<Position>().y = drone_pos.y;
            newRedArea.GetComponent<Detector>().owner = drone;
            GameObjectManager.bind(newRedArea);
        }
        switch (dr.type)
        {
            //Line type
            case DetectRange.Type.Line:
                switch (drone.GetComponent<Direction>().direction)
                {
                    case Direction.Dir.North:
                        generateDetector(drone, 0, -1);
                        break;
                    case Direction.Dir.West:
                        generateDetector(drone, -1, 0);
                        break;
                    case Direction.Dir.South:
                        generateDetector(drone, 0, 1);
                        break;
                    case Direction.Dir.East:
                        generateDetector(drone, 1, 0);
                        break;
                }
                break;
            case DetectRange.Type.Cross:
                generateDetector(drone, 0, -1);
                generateDetector(drone, -1, 0);
                generateDetector(drone, 0, 1);
                generateDetector(drone, 1, 0);
                break;
            case DetectRange.Type.Cone:
                break;
            case DetectRange.Type.Around:
                break;
        }       
    }

    private void generateDetector(GameObject drone, int xStep, int yStep)
    {
        DetectRange dr = drone.GetComponent<DetectRange>();
        Position drone_pos = drone.GetComponent<Position>();
        bool stop = false;
        for (int i = 0; i < dr.range; i++)
        {
            float x = drone_pos.x + i*xStep + 1*xStep;
            float y = drone_pos.y + i*yStep + 1*yStep;
            foreach (GameObject blocker in f_viewBlocker)
                if (blocker.GetComponent<Position>().x == x && blocker.GetComponent<Position>().y == y && (blocker.tag == "Wall" || (blocker.tag == "Door" && !blocker.GetComponent<ActivationSlot>().state)))
                    stop = true;
            if (stop)
                break;
            else
            {
                GameObject obj = Object.Instantiate(Resources.Load("Prefabs/RedDetector") as GameObject, LevelGO.transform.position + new Vector3(y * 3, 1.5f, x * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);
                obj.GetComponent<Position>().x = x;
                obj.GetComponent<Position>().y = y;
                obj.GetComponent<Detector>().owner = drone;
                GameObjectManager.bind(obj);
            }
        }
    }
}