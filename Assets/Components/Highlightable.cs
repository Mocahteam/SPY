using UnityEngine;

public class Highlightable : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    [HideInInspector]
    public Color baseColor = Color.white;
    public Color highlightedColor =  Color.yellow;
}