using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Compétence implémenté
	public bool active = true;
	// A quelle fonction est liée la compétence
	public List<string> compLinkWhitFunc = new List<string>();
	// A quel compétence est liée la compétence
	public List<string> compLinkWhitComp = new List<string>();
	// List des compétences dont au moins une doit être selectionnée
	public List<string> listSelectMinOneComp = new List<string>();
}