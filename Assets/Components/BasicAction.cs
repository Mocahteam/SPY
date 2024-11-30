using UnityEngine;

public class BasicAction : BaseElement {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    //Ajouter ici une nouvelle action type init variable
    public enum ActionType { Forward, TurnLeft, TurnRight, Wait, Activate, TurnBack };
    public ActionType actionType;
}