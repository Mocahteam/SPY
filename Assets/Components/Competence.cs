using UnityEngine;
using System.Collections.Generic;

public class Competence : MonoBehaviour {
	// Compétence implémenté
	public bool active = true;
	// Information sur la compétene
	public string info;
	// Liste des niveaux contenant la compétence
	public List<string> listLevel;
	// Quel competence coché si celle-ci est selectionner
	public List<string> compLink;
	// Quel compétence griser si celle-ci est coché
	public List<string> compNoPossible;
}