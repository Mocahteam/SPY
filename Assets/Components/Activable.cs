using UnityEngine;
using System.Collections.Generic;

public class Activable : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	public bool isActivated;
	public bool isFullyActivated;
	public List<int> slotID;

}