using UnityEngine;

[RequireComponent(typeof(Highlightable))]
public class UIActionType : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	//public GameObject prefab;
	public GameObject linkedTo;
	// Savoir si l'item est un container (for; if etc.)
	public bool container = false;
}