using UnityEngine;

public class DialogListEntry : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public string dialogText;
	public bool cameraMove;
	public int cameraMoveX;
	public int cameraMoveY;
}