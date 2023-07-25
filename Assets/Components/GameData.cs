using UnityEngine;
using System.Collections.Generic;
using System.Xml;

public class GameData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public Dictionary<string, XmlNode> levels; // The associated XmlNode to its path
	public Dictionary<string, WebGlScenario> scenarios; // The associated scenario description to its name
	public string selectedScenario; // name of the scenario to play
	public int levelToLoad; // level to load inside the selected scenario
	public int[] levelToLoadScore; //levelToLoadScore[0] = best score (3 stars) ; levelToLoadScore[1] = medium score (2 stars)
	public Dictionary<string, int> actionBlockLimit; //Is block available in library?
	public string scoreKey = "score";
	public int totalStep;
	public int totalActionBlocUsed;
	public int totalExecute;
	public int totalCoin;
	public GameObject actionsHistory; //all actions made in the level, displayed at the end
	public float gameSpeed_default = 1f;
	public float gameSpeed_current = 1f;
	public bool dragDropEnabled = true;
	public bool sendStatementEnabled = true;
	public string[] localization; // dynamic texts for localization
}