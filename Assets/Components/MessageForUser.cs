using UnityEngine;
using UnityEngine.Events;

public class MessageForUser : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public string message;
	public string OkButton;
	public string CancelButton;
	public UnityAction call;
}