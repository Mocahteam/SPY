using UnityEngine;
using System.Collections.Generic;

public class CompetenceInLevel : MonoBehaviour {
	// Note quel niveau utilisé pour une compétence précise
	// Dictionaire avec les compétenc en clef et un tableau de boolean correspondant aux niveaux ou la compétence peux être utilisée (true) ou non (false)
	public Dictionary<string, bool[]> competencPossible;
}