using UnityEngine;

// Used in ForBloc
public class SettingsManagerBridge : MonoBehaviour
{
	public void SaveParameters()
    {
		SettingsManager.instance.SaveParameters();
	}

	public void SendUserData()
	{
		SettingsManager.instance.SendUserData();
	}

	public void ExportSettings()
    {
		SettingsManager.instance.ExportSettings();
    }

    public void OnPresetSelected(int selectedIndex)
	{
        SettingsManager.instance.OnPresetSelected(selectedIndex);
    }

    public void SetSkillsRepository(int value)
    {
        SettingsManager.instance.SetSkillsRepository(value);
    }

    public void SetQualitySetting(int value)
    {
        SettingsManager.instance.SetQualitySetting(value);
	}

	public void SetAutoHideTooltip(int value)
	{
		SettingsManager.instance.SetAutoHideTooltip(value);
	}

	public void IncreaseUISize()
	{
		SettingsManager.instance.IncreaseUISize();
	}

	public void DecreaseUISize()
	{
		SettingsManager.instance.DecreaseUISize();
	}

	public void SetWallTransparency(int value)
    {
		SettingsManager.instance.SetWallTransparency(value);
	}

	public void SetCameraTracking(int value)
	{
		SettingsManager.instance.SetCameraTracking(value);
	}

	public void SetGameView(int value)
    {
		SettingsManager.instance.SetGameView(value);
	}

	public void SetAnimation(int value)
    {
		SettingsManager.instance.SetAnimation(value);
	}

	public void SetTooltipView(int value)
	{
		SettingsManager.instance.SetTooltipView(value);
	}

	public void HookListener(string key)
    {
		SettingsManager.instance.HookListener(key);
	}

	public void SetBorderThickness(string value)
	{
		SettingsManager.instance.SetBorderThickness(value);
	}

	public void SyncFonts(int value)
	{
		SettingsManager.instance.SyncFonts(value);
	}

	public void SetCaretWidth(string value)
	{
		SettingsManager.instance.SetCaretWidth(value);
	}

	public void SetCaretHeight(string value)
	{
		SettingsManager.instance.SetCaretHeight(value);
	}

	public void SetCharSpacing(string value)
	{
		SettingsManager.instance.SetCharSpacing(value);
	}

	public void SetWordSpacing(string value)
	{
		SettingsManager.instance.SetWordSpacing(value);
	}

	public void SetLineSpacing(string value)
	{
		SettingsManager.instance.SetLineSpacing(value);
	}

	public void SetParagraphSpacing(string value)
	{
		SettingsManager.instance.SetParagraphSpacing(value);
	}

	public void SetDoubleClickDelay(string value)
	{
		SettingsManager.instance.SetDoubleClickDelay(value);
    }

    public void SetLongClickDelay(string value)
    {
        SettingsManager.instance.SetLongClickDelay(value);
    }

    public void SetAutoClick(int value)
    {
        SettingsManager.instance.SetAutoClick(value);
    }

    public void SetAutoClickDelay(string value)
    {
        SettingsManager.instance.SetAutoClickDelay(value);
    }
}
