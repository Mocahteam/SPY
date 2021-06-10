using UnityEngine;

public class ScriptRef : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public GameObject scriptContainer;
	public GameObject uiContainer; //container to show/hide - root of Container prefab
	public int currentAction;
	public bool scriptFinished;
}