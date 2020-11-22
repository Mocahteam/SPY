using UnityEngine;

public class CameraComponent : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public float cameraSpeed = 5f;

	// Zoom
    public float zoomSpeed = 1f;
    public int ScrollWheelLimit = 30; // limite du zoom qui s'éloigne
    public int ScrollWheelminPush = 0;
    public int ScrollCount = 10;

    // Déplacement avec la molette
    public float dragSpeed = 100f;

    // Orbit, déplacement clic droit
    public float orbitH = 0f;
    public float orbitV = 0f;
    public float lookSpeedH = 5f;
    public float lookSpeedV = 5f;
        
    // Déplacements relatifs à la caméra
    //public Vector3 movementRotation;
    public int limiteCount = 10;
    public int limiteMax = 30;
    public int limiteMin = 0;
    public float MIN_X = -100f;
    public float MAX_X = -20f;
    public float MIN_Y = -20f;
    public float MAX_Y = 60f;
    public float MIN_Z = -20f;
    public float MAX_Z = 40f;
    public Transform farLeft;  // End of screen Left
    public Transform farRight;  //End of Screen Right
}