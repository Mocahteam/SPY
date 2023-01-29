using UnityEngine;

public class DetectRange : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	public int range;

	public bool selfRange;
	public enum Type {Line, Cross, Cone, Around};
	public Type type;
}