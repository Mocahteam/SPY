using UnityEngine;

[RequireComponent(typeof(Highlightable))]
public class LibraryItemRef : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public GameObject linkedTo; // keep a reference to an item in the library
}