using UnityEngine;
using System.Collections.Generic;

public class GameData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	public GameObject Level;
	public Dictionary <string, List<string>> levelList; //key = directory name, value = list of level file name
	public (string, int) levelToLoad; //directory name, level index
	public int[] levelToLoadScore; //levelToLoadScore[0] = best score (3 stars) ; levelToLoadScore[1] = medium score (2 stars)
	public List<(string,string)> dialogMessage; //list of (dialogText, imageName)
	public Dictionary<string, int> actionBlocLimit;
	public string scoreKey = "score";
	public int totalStep;
	public int totalActionBloc;
	public int totalExecute;
	public int totalCoin;
	public GameObject actionsHistory; //all actions made in the level, displayed at the end

	//public Dictionary<string, int>  currentLevelBlocLimits;
}