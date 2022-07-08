using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Compétence implémenté
	public bool active = true;
	// A quel fonction est lié la compétence
	public List<string> compLinkWhitFunc = new List<string>();
	// A quel comp est lié la compétence
	public List<string> compLinkWhitComp = new List<string>();
	// List des comp dont au moins une doit être selectionné
	public List<string> listSelectMinOneComp = new List<string>();
}