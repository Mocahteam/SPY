using DIG.GBLXAPI;
using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public string birthYear;
	public bool isTeacher;
	public Dictionary<string, int> progression; // store for each scenario the number of unlocked levels
	public Dictionary<string, int> highScore; // store for each level its star number
	public string currentScenario;
	public int levelToContinue;
	public List<int> unlockedAvatars;
	public int avatarSelected;
	public int newAvatarAvailable;

	public long lastFocusOut = -1;
}