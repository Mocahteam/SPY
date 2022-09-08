using UnityEngine;
using System.Collections.Generic;

public class FunctionalityInLevel : MonoBehaviour {
	// Note quel niveau utilisé pour une fonctionnalité précise
	// Dictionaire avec les fonctionnalités en clef et un tableau de boolean correspondant aux niveaux ou la fonctionnalité peut être utilisée (true) ou non (false)
	public Dictionary<string, List<string>> levelByFuncLevelDesign = new Dictionary<string, List<string>>();
	// Note quel niveau utiliser pour une compétence précise autre qu'une fonctionnalité de level design
	public Dictionary<string, List<string>> levelByFunc = new Dictionary<string, List<string>>();

}