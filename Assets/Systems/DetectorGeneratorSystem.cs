using UnityEngine;
using FYFY;

public class DetectorGeneratorSystem : FSystem {

	private Family detectRangeGO = FamilyManager.getFamily(new AllOfComponents(typeof(DetectRange)));
	private Family detectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Detector), typeof(Position)));
	private Family entityGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));

    private Family gameLoaded_f = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private GameData gameData;

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	public DetectorGeneratorSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
        gameLoaded_f.addEntryCallback(generateStep);
        newStep_f.addEntryCallback(generateStep);
    }

    private void generateStep(GameObject unused)
    {
        //Destroy detection cells
        foreach (GameObject detector in detectorGO)
        {
            // Remove position (because GameObject is not destroyed immediate)
            Position pos = detector.GetComponent<Position>();
            pos.x = -1;
            pos.z = -1;
            GameObjectManager.unbind(detector);
            Object.Destroy(detector);
        }

        bool stop = false;
        //Generate detection cells
        foreach (GameObject detect in detectRangeGO)
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
                                foreach (GameObject wall in entityGO)
                                {
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                    {
                                        stop = true;
                                    }
                                }
                                if (stop)
                                {
                                    break;
                                }
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
                                foreach (GameObject wall in entityGO)
                                {
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                    {
                                        stop = true;
                                    }
                                }
                                if (stop)
                                {
                                    break;
                                }
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
                                foreach (GameObject wall in entityGO)
                                {
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                    {
                                        stop = true;
                                    }
                                }
                                if (stop)
                                {
                                    break;
                                }
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
                                foreach (GameObject wall in entityGO)
                                {
                                    if (wall.GetComponent<Position>().x == x && wall.GetComponent<Position>().z == z)
                                    {
                                        stop = true;
                                    }
                                }
                                if (stop)
                                {
                                    break;
                                }
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