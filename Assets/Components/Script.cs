using UnityEngine;
using System.Collections.Generic;

public class Script : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public enum Actions {Forward, TurnLeft, TurnRight};
	public List<Actions> actions;

	public int currentAction = 0;
}