using DIG.GBLXAPI;
using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public string schoolClass;
	public bool isTeacher;
	public Dictionary<string, int> progression; // store for each scenario the number of unlocked levels
	public Dictionary<string, int> highScore; // store for each level its star number

	private long lastFocusOut = -1;

	private void OnApplicationFocus(bool hasFocus)
	{
		StartCoroutine(catchApplicationState(hasFocus));
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		StartCoroutine(catchApplicationState(!pauseStatus));
	}

	private IEnumerator catchApplicationState(bool hasFocus)
	{
		if (!hasFocus) // player click outside the game
			lastFocusOut = DateTime.Now.ToUniversalTime().Ticks;
		else // player come back in the game
		{
			if (lastFocusOut != -1 && new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - lastFocusOut).Minutes >= 10)
			{
				GameObject xAPI = GameObject.Find("GBLXAPI");
				if (xAPI != null)
				{
					GameObject.Destroy(xAPI);
					GBLXAPI.IsInit = false;
				}
				GameData gd = GameObject.Find("GameData").GetComponent<GameData>();
				gd.selectedScenario = "";
				gd.actionsHistory = null;
				yield return null;
				yield return null;
				GameObjectManager.loadScene("TitleScreen");
			}
		}
	}
}