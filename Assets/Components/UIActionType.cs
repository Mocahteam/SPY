using UnityEngine;

public class UIActionType : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public Action.ActionType type;
	public GameObject prefab;
	public GameObject linkedTo;
}