using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Compétence implémenté
	public bool active = true;
	// A quel fonction est lié la compétence
	public List<string> compLinkWhitFunc;
	// A quel comp est lié la compétence
	public List<string> compLinkWhitComp;
}