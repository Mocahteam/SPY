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
		NotComplete, // Le script donné n'est pas complet, le joueur doit le terminer
		Bugged, // Le script donné contient des erreurs que le joueur doit corriger
		Undefined // On autorise le changement de nom par l'utilisateur
	};
	public SolutionType type = SolutionType.Undefined;
}