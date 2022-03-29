using UnityEngine;

public class AgentEdit : MonoBehaviour {
	// Pour l'édition du nom
	public string agentName = "N°1"; //Nom par defaut
	public bool editName = true; // On autorise le changement de nom par l'utilisateur
	public bool editNameAuto = true; // Si on change le nom du script container, cela change aussi le nom de l'agent associer
}