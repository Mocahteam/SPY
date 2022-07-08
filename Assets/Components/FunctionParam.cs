using System.Collections.Generic;
using UnityEngine;

public class FunctionParam : MonoBehaviour {
	// Compétence implémenté
	public Dictionary<string,bool> active = new Dictionary<string, bool>();
	// Compétence implémenté
	public Dictionary<string, bool> levelDesign = new Dictionary<string, bool>();
	// Quel sont les fonctions à activé si on active celle-ci
	public Dictionary<string, List<string>> activeFunc = new Dictionary<string, List<string>>();
	// Quel sont les fonctions à desactivé si on active celle-ci
	public Dictionary<string, List<string>> enableFunc = new Dictionary<string, List<string>>();
}