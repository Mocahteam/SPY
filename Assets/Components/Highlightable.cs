using UnityEngine;

public class Highlightable : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Color baseColor = Color.white;
    public Color highlightedColor =  Color.yellow;
    // TODO : Virer ça de là => Highlightable est aussi utilisé pour tous les objets de la scène comme le sol ou le robot dropZoneChild est propre aux objets de programme
    public GameObject dropZoneChild; // Drop zone général de l'objet qui permetra d'activer la red bar ou l'outline lors du survole de l'élément (selon condition)
}