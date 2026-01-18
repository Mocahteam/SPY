using UnityEngine;

public class Position : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public float x;
	public float y;
	public float targetX = -1;
	public float targetY = -1;
}