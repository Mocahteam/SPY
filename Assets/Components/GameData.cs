using UnityEngine;

public class GameData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public bool scriptRunning;

	public GameObject ButtonExec;
	public GameObject ButtonReset;

	public GameObject CasePrefab;
	public GameObject WallPrefab;
	public GameObject Level;
	public GameObject SpawnPrefab;
	public GameObject ExitPrefab;
	public GameObject PlayerPrefab;
}