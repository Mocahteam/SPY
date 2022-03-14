using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class checkPositiveNumber : MonoBehaviour
{
	public void onlyPositiveInteger(string input)
	{
		int res;
		bool success = Int32.TryParse(input, out res);
		if (!success || (success && Int32.Parse(input) < 0))
		{
			GetComponent<TMP_InputField>().text = "0";
		}
	}
}
