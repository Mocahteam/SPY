using UnityEngine;

public class ActivationSlot : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	public int slotID;
	public enum ActivationType {Destroy};
	public ActivationType type;
}