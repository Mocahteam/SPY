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

	public void SaveParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveParameters", null);
	}

	public void SendUserData()
	{
		MainLoop.callAppropriateSystemMethod (system, "SendUserData", null);
	}

	public void ExportSettings()
	{
		MainLoop.callAppropriateSystemMethod (system, "ExportSettings", null);
	}

	public void OnPresetSelected(System.Int32 selectedIndex)
	{
		MainLoop.callAppropriateSystemMethod (system, "OnPresetSelected", selectedIndex);
	}

	public void ImportSettingsFromJS(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "ImportSettingsFromJS", content);
	}

	public void ImportSettings(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "ImportSettings", content);
	}

	public void HookListener(System.String key)
	{
		MainLoop.callAppropriateSystemMethod (system, "HookListener", key);
	}

	public void SetSkillsRepository(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetSkillsRepository", value);
	}

	public void SetQualitySetting(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetQualitySetting", value);
	}

	public void SetAutoHideTooltip(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetAutoHideTooltip", value);
	}

	public void IncreaseUISize()
	{
		MainLoop.callAppropriateSystemMethod (system, "IncreaseUISize", null);
	}

	public void DecreaseUISize()
	{
		MainLoop.callAppropriateSystemMethod (system, "DecreaseUISize", null);
	}

	public void SyncCanvasScaler()
	{
		MainLoop.callAppropriateSystemMethod (system, "SyncCanvasScaler", null);
	}

	public void SetWallTransparency(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetWallTransparency", value);
	}

	public void SetCameraTracking(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetCameraTracking", value);
	}

	public void SetGameView(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetGameView", value);
	}

	public void SetAnimation(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetAnimation", value);
	}

	public void SetTooltipView(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetTooltipView", value);
	}

	public void SyncFonts(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SyncFonts", value);
	}

	public void SetCaretWidth(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetCaretWidth", value);
	}

	public void SetCaretHeight(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetCaretHeight", value);
	}

	public void SetBorderThickness(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetBorderThickness", value);
	}

	public void SetCharSpacing(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetCharSpacing", value);
	}

	public void SetWordSpacing(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetWordSpacing", value);
	}

	public void SetLineSpacing(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetLineSpacing", value);
	}

	public void SetParagraphSpacing(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetParagraphSpacing", value);
	}

	public void SetDoubleClickDelay(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetDoubleClickDelay", value);
	}

	public void SetLongClickDelay(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetLongClickDelay", value);
	}

	public void SetAutoClick(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetAutoClick", value);
	}

	public void SetAutoClickDelay(System.String value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetAutoClickDelay", value);
	}

	public void SelectAvatar(UnityEngine.UI.Image src)
	{
		MainLoop.callAppropriateSystemMethod (system, "SelectAvatar", src);
	}

	public void SetBirthYear(System.String year)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetBirthYear", year);
	}

	public void SetIsTeacher(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetIsTeacher", state);
	}

}
