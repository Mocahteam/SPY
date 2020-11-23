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
        
    // Limites de la caméra
    // Position
    public float MIN_X = -50f;
    public float MAX_X = 50f;
    public float MIN_Y = -10f;
    public float MAX_Y = 30f;
    public float MIN_Z = -50f;
    public float MAX_Z = 50f;
    public float init_X = 0f;
    public float init_Y = 0f;
    public float init_Z = 0f;
    // Rotation
    public float MIN_X_angle = -90f;
    public float MAX_X_angle = 90f;
    public float MIN_Y_angle = -90f;
    public float MAX_Y_angle = 90f;
    //public float MIN_Z_angle = -50f; // inutile dans notre cas
    //public float MAX_Z_angle = 50f; // inutile dans notre cas
    public Vector3 initRotation;
    //public float init_X_angle = 0f;
    //public float init_Y_angle = 0f;
    //public float init_Z_angle = 0f; // inutile dans notre cas
}