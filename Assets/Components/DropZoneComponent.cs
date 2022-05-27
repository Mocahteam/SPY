using UnityEngine;

public class DropZoneComponent : MonoBehaviour {
	public GameObject target; // Le container qui contiendra l'objet que l'on drop sur la drop zone
	public bool parentTarget = false; // Si oui, on cherchera le parent de l'objet mis en target (le container qui contient l'objet)
}