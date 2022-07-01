using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Compétence implémenté
	public bool active = true;
	// Liste des niveaux contenant la compétence
	public List<string> listLevel;
	// Quel competence coché si celle-ci est selectionner
	public List<string> compLink;
	// Quel compétence griser si celle-ci est coché
	public List<string> compNoPossible;
	// Quel compétence décoché si celle-ci est décoché
	public List<string> compLinkUnselect;
}