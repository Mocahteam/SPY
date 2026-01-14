using UnityEngine;
using FYFY;

public class SettingsManager_wrapper : BaseWrapper
{
	public UnityEngine.Transform settingsWindow;
	public UnityEngine.UI.Selectable LoadingLogs;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "settingsWindow", settingsWindow);
		MainLoop.initAppropriateSystemField (system, "LoadingLogs", LoadingLogs);
	}

	public void saveParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveParameters", null);
	}

	public void resetParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetParameters", null);
	}

	public void hookListener(System.String key)
	{
		MainLoop.callAppropriateSystemMethod (system, "hookListener", key);
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

	public void setWallTransparency(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setWallTransparency", value);
	}

	public void setGameView(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setGameView", value);
	}

	public void setTooltipView(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setTooltipView", value);
	}

	public void syncFonts(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "syncFonts", value);
	}

	public void setCaretWidth(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setCaretWidth", value);
	}

	public void setCaretHeight(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setCaretHeight", value);
	}

	public void setBorderTickness(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setBorderTickness", value);
	}

	public void setCharSpacing(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setCharSpacing", value);
	}

	public void setWordSpacing(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setWordSpacing", value);
	}

	public void setLineSpacing(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setLineSpacing", value);
	}

	public void setParagraphSpacing(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setParagraphSpacing", value);
	}

}
