using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public string schoolClass;
	public bool isTeacher;
	public Dictionary<string, int> progression; // store for each scenario the number of unlocked levels
	public Dictionary<string, int> highScore; // store for each level its star number
}