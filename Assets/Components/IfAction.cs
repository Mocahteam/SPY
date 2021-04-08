using UnityEngine;

public class IfAction : BaseElement {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public int ifEntityType;
    public bool ifNot;
    public int range;
    public int ifDirection;
    public GameObject firstChild;
}