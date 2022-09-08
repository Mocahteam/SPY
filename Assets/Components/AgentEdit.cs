using UnityEngine;

public class AgentEdit : MonoBehaviour
{
	public enum EditMode { 
		Locked, // Le nom est défini par le système
		Editable, // On autorise le changement de nom par l'utilisateur
		Synch // Si on change le nom du script container, cela change aussi le nom de l'agent associé
	};
	// Pour l'édition du nom
	public string agentName = "N°1"; //Nom par defaut
	public EditMode editState = EditMode.Synch;
}