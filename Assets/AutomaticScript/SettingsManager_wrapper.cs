using UnityEngine;
using FYFY;

public class SettingsManager_wrapper : BaseWrapper
{
	public UnityEngine.Transform settingsPanel;
	public UnityEngine.UI.CanvasScaler[] canvasScaler;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "settingsPanel", settingsPanel);
		MainLoop.initAppropriateSystemField (system, "canvasScaler", canvasScaler);
	}

	public void setQualitySetting(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setQualitySetting", value);
	}

	public void setInteraction(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setInteraction", value);
	}

	public void increaseUISize()
	{
		MainLoop.callAppropriateSystemMethod (system, "increaseUISize", null);
	}

	public void decreaseUISize()
	{
		MainLoop.callAppropriateSystemMethod (system, "decreaseUISize", null);
	}

}
