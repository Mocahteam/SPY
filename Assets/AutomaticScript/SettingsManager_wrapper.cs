using UnityEngine;
using FYFY;

public class SettingsManager_wrapper : BaseWrapper
{
	public UnityEngine.Transform settingsWindow;
	public UnityEngine.UI.Selectable LoadingLogs;
	public System.Boolean settingsUpdated;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "settingsWindow", settingsWindow);
		MainLoop.initAppropriateSystemField (system, "LoadingLogs", LoadingLogs);
		MainLoop.initAppropriateSystemField (system, "settingsUpdated", settingsUpdated);
	}

	public void saveParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveParameters", null);
	}

	public void sendUserData()
	{
		MainLoop.callAppropriateSystemMethod (system, "sendUserData", null);
	}

	public void exportSettings()
	{
		MainLoop.callAppropriateSystemMethod (system, "exportSettings", null);
	}

	public void importSettingsFromJS(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "importSettingsFromJS", content);
	}

	public void importSettings(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "importSettings", content);
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

	public void syncCanvasScaler()
	{
		MainLoop.callAppropriateSystemMethod (system, "syncCanvasScaler", null);
	}

	public void setWallTransparency(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setWallTransparency", value);
	}

	public void setCameraTracking(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setCameraTracking", value);
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

	public void selectAvatar(UnityEngine.UI.Image src)
	{
		MainLoop.callAppropriateSystemMethod (system, "selectAvatar", src);
	}

	public void setBirthYear(System.String year)
	{
		MainLoop.callAppropriateSystemMethod (system, "setBirthYear", year);
	}

	public void setIsTeacher(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod (system, "setIsTeacher", state);
	}

}
