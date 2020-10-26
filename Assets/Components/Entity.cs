using UnityEngine;

public class Entity : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public enum Type {Wall}
	public Type type;
}