using UnityEngine;

public class Camera : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public float cameraSpeed = 5f;

	// Zoom
    public float zoomSpeed = 1f;
    public int ScrollWheelLimit = 30; // limite du zoom qui s'éloigne
    public int ScrollWheelminPush = 0;
    public int ScrollCount = 10;

    // Déplacement clic droit
    public float DragSpeed = 2f;
    public bool ReverseDrag = true; 
    public Vector3 DragOrigin;
    public Vector3 Move;
}