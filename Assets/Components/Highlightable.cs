using UnityEngine;

public class Highlightable : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Color baseColor = Color.white;
    public Color highlightedColor =  Color.yellow;
    public GameObject dropZoneChild; // Drop zone général de l'objet qui permetra d'activer la red bar ou l'outline lors du survole de l'élément (selon condition)
}