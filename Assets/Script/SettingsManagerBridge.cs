using UnityEngine;

// Used in ForBloc
public class SettingsManagerBridge : MonoBehaviour
{
	public void resetParameters()
    {
		SettingsManager.instance.ResetParameters();
	}

	public void saveParameters()
    {
		SettingsManager.instance.SaveParameters();
	}

	public void sendUserData()
	{
		SettingsManager.instance.SendUserData();
	}

	public void exportSettings()
    {
		SettingsManager.instance.ExportSettings();
    }

    public void onPresetSelected(int selectedIndex)
	{
        SettingsManager.instance.OnPresetSelected(selectedIndex);
    }

    public void setSkillsRepository(int value)
    {
        SettingsManager.instance.SetSkillsRepository(value);
    }

    public void setQualitySetting(int value)
    {
        SettingsManager.instance.SetQualitySetting(value);
	}

	public void setAutoHideTooltip(int value)
	{
		SettingsManager.instance.SetAutoHideTooltip(value);
	}

	public void increaseUISize()
	{
		SettingsManager.instance.IncreaseUISize();
	}

	public void decreaseUISize()
	{
		SettingsManager.instance.DecreaseUISize();
	}

	public void setWallTransparency(int value)
    {
		SettingsManager.instance.SetWallTransparency(value);
	}

	public void setCameraTracking(int value)
	{
		SettingsManager.instance.SetCameraTracking(value);
	}

	public void setGameView(int value)
    {
		SettingsManager.instance.SetGameView(value);
	}

	public void setAnimation(int value)
    {
		SettingsManager.instance.SetAnimation(value);
	}

	public void setTooltipView(int value)
	{
		SettingsManager.instance.SetTooltipView(value);
	}

	public void hookListener(string key)
    {
		SettingsManager.instance.HookListener(key);
	}

	public void setBorderThickness(string value)
	{
		SettingsManager.instance.SetBorderThickness(value);
	}

	public void syncFonts(int value)
	{
		SettingsManager.instance.SyncFonts(value);
	}

	public void setCaretWidth(string value)
	{
		SettingsManager.instance.SetCaretWidth(value);
	}

	public void setCaretHeight(string value)
	{
		SettingsManager.instance.SetCaretHeight(value);
	}

	public void setCharSpacing(string value)
	{
		SettingsManager.instance.SetCharSpacing(value);
	}

	public void setWordSpacing(string value)
	{
		SettingsManager.instance.SetWordSpacing(value);
	}

	public void setLineSpacing(string value)
	{
		SettingsManager.instance.SetLineSpacing(value);
	}

	public void setParagraphSpacing(string value)
	{
		SettingsManager.instance.SetParagraphSpacing(value);
	}

	public void setDoubleClickDelay(string value)
	{
		SettingsManager.instance.SetDoubleClickDelay(value);
    }

    public void setLongClickDelay(string value)
    {
        SettingsManager.instance.SetLongClickDelay(value);
    }

    public void setAutoClick(int value)
    {
        SettingsManager.instance.SetAutoClick(value);
    }

    public void setAutoClickDelay(string value)
    {
        SettingsManager.instance.SetAutoClickDelay(value);
    }
}
