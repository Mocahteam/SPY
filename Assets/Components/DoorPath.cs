using System.Collections.Generic;
using UnityEngine;

public class DoorPath : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public List<DoorPath> nexts = new List<DoorPath>();
}