using UnityEngine;
using System.Collections.Generic;

public class GameData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

	public GameObject ButtonExec;
	public GameObject ButtonReset;
	public GameObject Level;
	public bool step;
	public bool checkStep;
	public bool generateStep;
	public int nbStep;
	public bool initialize;
	public List<string> levelList;
	public int levelToLoad;
	public List<string> dialogMessage;
	public List<int> actionBlocLimit;
	public int endLevel;

	public int totalStep;
	public int totalActionBloc;
	public int totalExecute;
	public int totalCoin;
}