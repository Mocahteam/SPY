using UnityEngine;
using FYFY;

public class RandomRotationSystem_wrapper : BaseWrapper
{
	public CurrentSettingsValues currentSettingsValues;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
	}

}
