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
	public enum SolutionType
	{
		Optimal, // Le script donné résout le niveau de manière optimale
		NonOptimal, // Le script donné résout le niveau de manière non optimale
		Bugged, // Le script donné ne résout pas en l'état le niveau (incomplet, bug)
		Undefined
	};
	public SolutionType type = SolutionType.Undefined;
}