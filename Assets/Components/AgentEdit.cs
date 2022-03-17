using UnityEngine;

public class AgentEdit : MonoBehaviour {
	// Pour l'édition du nom
	public string agentName = "Agent"; //Nom par defaut
	public bool editName = true; // On autorise le changement de nom par l'utilisateur
	public bool editNameAuto = true; // Si on change le nom du script container, cela change aussi le nom de l'agent associer
	public GameObject containerScriptAssocied = null; // Le script container associer à l'agent
}