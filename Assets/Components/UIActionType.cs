using UnityEngine;
[RequireComponent(typeof(BaseElement))]
public class UIActionType : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public GameObject prefab;
	public GameObject linkedTo;
}