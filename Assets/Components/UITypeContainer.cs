using UnityEngine;

public class UITypeContainer : MonoBehaviour {
	// Nom de l'agent à associer au container
	public string associedAgentName = "Agent";
	public bool actionContainer = false; // True si c'est le container d'un bloc
	public bool notScriptContainer = false; // Si ce n'est pas un container de script (bloc for par exemple)
}