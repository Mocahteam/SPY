using System.Collections.Generic;
using UnityEngine;

public class FunctionalityParam : MonoBehaviour {
	// Fonctionalité implémenté
	public Dictionary<string,bool> active = new Dictionary<string, bool>();
	// Fonctionalité basé sur le level design
	public Dictionary<string, bool> levelDesign = new Dictionary<string, bool>();
	// Quel sont les fonctionalités à activé si on active celle-ci
	public Dictionary<string, List<string>> activeFunc = new Dictionary<string, List<string>>();
	// Quel sont les fonctionalités à desactivé si on active celle-ci
	public Dictionary<string, List<string>> enableFunc = new Dictionary<string, List<string>>();
}