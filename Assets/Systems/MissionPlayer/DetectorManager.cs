using UnityEngine;
using FYFY;

/// <summary>
/// Manage detector areas 
/// </summary>
public class DetectorManager : FSystem {

	private readonly Family f_enemy = FamilyManager.getFamily(new AllOfComponents(typeof(DetectRange), typeof(Direction), typeof(Position)), new AnyOfTags("Drone"));
	private readonly Family f_detector = FamilyManager.getFamily(new AllOfComponents(typeof(Detector), typeof(Position), typeof(Rigidbody)));
	private readonly Family f_viewBlocker = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall", "Door"));
    private readonly Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));
    private readonly Family f_playMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private readonly Family f_editMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));
    private readonly Family f_positionCorrected = FamilyManager.getFamily(new AllOfComponents(typeof(PositionCorrected)));

    public GameObject LevelGO;

    public GameObject RedDetectorPrefab;

    public static DetectorManager instance;

    public DetectorManager()
    {
        instance = this;
    }

    protected override void onStart()
    {
        // On passe par 4 familles différentes et non une seule famille avec tous les composants car sinon si le GameObject a déjà un des composants et qu'on en ajoute un supplémentaire, la callback n'est pas appelé car le GO est déjà dans la famille
        f_gameLoaded.addEntryCallback(delegate { UpdateDetectors(); });
        f_playMode.addEntryCallback(delegate { UpdateDetectors(); });
        f_editMode.addEntryCallback(delegate { UpdateDetectors(); });
        f_positionCorrected.addEntryCallback(delegate { UpdateDetectors(); });

        Pause = true;
    }

    // Used by ReloadState and StopButton UIs
    public void UpdateDetectors()
    {
        Pause = false; // Pour ne faire le traitement qu'une fois dans le cas où plusieurs sources déclenchent en même temps
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        // On fait le traitement ici et pas dans la callback pour ne faire le traitement qu'une fois dans le cas où plusieurs sources déclenchent en même temps, sinon chaque callback ferait le traitement
        foreach (GameObject detect in f_enemy)
            UpdateDetector(detect);
        Pause = true;
    }

    // Reset detector positions depending on drone properties (position, orientation, range...)
    private void UpdateDetector(GameObject drone)
    {
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
        // Si la position que le drone cherche à atteindre est une porte fermée (par exemple) il va se planter dans ce cas on ne cherche même pas à générer les zones de détection
        if (!drone.GetComponent<ScriptRef>().isBroken && !ViewIsLocked(drone_pos.x, drone_pos.y))
        {
            // Create detector under drone
            if (dr.selfRange)
            {
                GameObject newRedArea = Object.Instantiate(RedDetectorPrefab, LevelGO.transform.position + new Vector3(drone_pos.x * 3, 1.5f, -drone_pos.y * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);
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
                            GenerateDetector(drone, 0, -1);
                            break;
                        case Direction.Dir.West:
                            GenerateDetector(drone, -1, 0);
                            break;
                        case Direction.Dir.South:
                            GenerateDetector(drone, 0, 1);
                            break;
                        case Direction.Dir.East:
                            GenerateDetector(drone, 1, 0);
                            break;
                    }
                    break;
                case DetectRange.Type.Cross:
                    GenerateDetector(drone, 0, -1);
                    GenerateDetector(drone, -1, 0);
                    GenerateDetector(drone, 0, 1);
                    GenerateDetector(drone, 1, 0);
                    break;
                case DetectRange.Type.Cone:
                    break;
                case DetectRange.Type.Around:
                    break;
            }
        }
    }

    private void GenerateDetector(GameObject drone, int xStep, int yStep)
    {
        DetectRange dr = drone.GetComponent<DetectRange>();
        Position drone_pos = drone.GetComponent<Position>();
        for (int i = 0; i < dr.range; i++)
        {
            int x = drone_pos.x + i*xStep + 1*xStep;
            int y = drone_pos.y + i*yStep + 1*yStep;
            if (ViewIsLocked(x, y))
                break;
            else
            {
                GameObject obj = Object.Instantiate(RedDetectorPrefab, LevelGO.transform.position + new Vector3(x * 3, 1.5f, -y * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);
                obj.GetComponent<Position>().x = x;
                obj.GetComponent<Position>().y = y;
                obj.GetComponent<Detector>().owner = drone;
                GameObjectManager.bind(obj);
            }
        }
    }

    private bool ViewIsLocked(int x, int y)
    {
        foreach (GameObject blocker in f_viewBlocker)
            if (blocker.GetComponent<Position>().x == x && blocker.GetComponent<Position>().y == y && (blocker.CompareTag("Wall") || (blocker.CompareTag("Door") && !blocker.GetComponent<ActivationSlot>().state)))
                return true;
        return false;
    }
}