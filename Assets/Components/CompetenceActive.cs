using UnityEngine;

public class CompetenceActive : MonoBehaviour {
	// Vous trouverez ici l'ensemble des compétences PIAF présent (si possible et paramatrable) dans le jeu
	// Celle-ci sont toutes désactivées par défaut, seul le parametrage lors de la création de niveau déterminera la présence des compétences dans le niveau.

	// C1.1 Nommer des objets et séquence d'actions
	public bool nameObject = false;
	// C1.3 Identifier les paramètres d'entrée d'une séquence d'actions
	public bool multiAgent = false;
	// C1.5 Prédir le résultat d'une séquence d'actions
	public bool predictionSequence = false;
	// C1.6 Utiliser des objets dont la valeur peut changer
	public bool variableValue = false;
	// C1.7 Reconnaître, parmi des objets et séquences d'action connus, lesquels peuvent être utilisés pour atteindre un nouvel objectif
	public bool similarProblemSolution = false;
	// C2.1 Ordonner une séquence d'actions pour atteindre un objectif
	public bool orderSequence = false;
	// C2.2 Compléter une séquence d'actions pour atteindre un objectif simple
	public bool completeSequence = false;
	// C2.5 Combiner des séquences d'actions pour atteindre un objectif
	public bool combineSequence = false;
	// C2.6 Décomposer des objectifs en sous-objectifs plus simple
	public bool subGoal = false;
	// C3.1 Répéter une séquence d'actions un nombre donné de fois
	public bool forAction = false;
	// C3.2 Répéter une séquence d'actions jusqu'à ce qu'un objectif soit atteint
	public bool whileAction = false;
	// C3.3 Intégrer une condition simple dans une séquence d'actions
	public bool conditionAction = false;
	// C3.4 Intégrer une condition complexe dans une séquence d'actions
	public bool multiConditionAction = false;
	// C4.1 Comparer deux objets selon un critère donné
	public bool comparisonCriterion = false;
	// C4.2 Comparer deux séquences d'actions selon un critère donné
	public bool comparisonSequence = false;
	// C4.3 Améliorer une séquence d'actions par rapport à un critère donné
	public bool completeSequenceWithCriterion = false;
	// C5.2 Traduire des objets ou séquences d'actions entre représentations formelles
	public bool consolePresence = false;
	// C6.4 Etendre ou modifier une séquence d'actions pour atteindre un nouvelle objectif
	public bool oneSequenceMultiGoal = false;
}