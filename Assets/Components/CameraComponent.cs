using UnityEngine;

public class CameraComponent : MonoBehaviour {
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

    // Orbit
    public float xDeg = 0.0f;
    public float yDeg = 0.0f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public float zoomDampening = 5.0f;
    public Quaternion currentRotation;
    public Quaternion desiredRotation;
    public Quaternion rotation;

    public float dragSpeed = 100f;
}