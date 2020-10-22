using UnityEngine;


public class Direction : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public enum Dir {North,South,East,West};
	public Dir direction;
}