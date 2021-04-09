using UnityEngine;

[DisallowMultipleComponent]
public class BaseElement : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Color baseColor;
    public GameObject target;
    public GameObject next;
    public int currentAction;
}