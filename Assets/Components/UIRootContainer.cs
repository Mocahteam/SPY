using UnityEngine;

public class UIRootContainer : MonoBehaviour {
	// Nom du script
	public string scriptName = "Agent";
	public enum EditMode
	{
		Locked, // Le nom est défini par le système
		Synch, // Si on change le nom du script container, cela change aussi le nom de l'agent associé
		Editable // On autorise le changement de nom par l'utilisateur
	};
	public EditMode editState = EditMode.Synch;
}