using UnityEngine;
using FYFY;

public class SyncLocalization_wrapper : BaseWrapper
{
	public CurrentSettingsValues currentSettingsValues;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
	}

	public void SyncLocale()
	{
		MainLoop.callAppropriateSystemMethod (system, "SyncLocale", null);
	}

}
