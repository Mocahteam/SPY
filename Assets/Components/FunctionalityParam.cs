using System.Collections.Generic;
using UnityEngine;

public class FunctionalityParam : MonoBehaviour {
	// Fonctionalité implémentée
	public Dictionary<string,bool> active = new Dictionary<string, bool>();
	// Fonctionalité basée sur le level design
	public Dictionary<string, bool> levelDesign = new Dictionary<string, bool>();
	// Quelles sont les fonctionalités à activer si on active celle-ci
	public Dictionary<string, List<string>> activeFunc = new Dictionary<string, List<string>>();
	// Quelles sont les fonctionalités à desactiver si on active celle-ci
	public Dictionary<string, List<string>> enableFunc = new Dictionary<string, List<string>>();
	// Quelles sont les fonctionnalités qui doivent être activées dans le niveau selon les compétences selectionnées par l'utilisateur
	public List<string> funcActiveInLevel = new List<string>();
	// Lors de l'activation de certaines fonctions, quels sont les éléments dont les limites sont à vérifier
	public Dictionary<string, List<string>> elementRequiermentLibrary= new Dictionary<string, List<string>>();
	// Liste des capteurs
	public List<string> listCaptor = new List<string>();
}