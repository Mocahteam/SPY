using UnityEngine;

[DisallowMultipleComponent]
public class BaseElement : Highlightable {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public GameObject target;
    public GameObject next;
    public int currentAction;
}