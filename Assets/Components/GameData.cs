using UnityEngine;

public class GameData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	public GameObject ButtonExec;
	public GameObject ButtonReset;
	public GameObject Level;
	public bool step;
	public bool checkStep;
	public bool generateStep;
	public int nbStep;
	public bool initialize;
}