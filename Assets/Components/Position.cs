using UnityEngine;

public class Position : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public int x;
	public int y;
	public int targetX = -1;
	public int targetY = -1;
}