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
	// Quel sont les fonctionnalités qui doivent être activées dans le niveau selon les compétences selectionnées par l'utilisateur
	public List<string> funcActiveInLevel = new List<string>();
	// Lors de l'activation de certaine fonction, quel sont les éléments dont les limites sont à vérifié
	public Dictionary<string, List<string>> elementRequiermentLibrary= new Dictionary<string, List<string>>();
	// List des capteurs
	public List<string> listCaptor = new List<string>();
}