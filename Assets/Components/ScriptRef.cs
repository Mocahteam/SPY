using UnityEngine;

public class ScriptRef : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public GameObject executableScript;
	public GameObject executablePanel; //container to show/hide - root of Container prefab
	public bool scriptFinished;
	public int nbOfInactions;
}