using UnityEngine;

// Used in ForBloc
public class SettingsManagerBridge : MonoBehaviour
{
	public void resetParameters()
    {
		SettingsManager.instance.resetParameters();
	}

	public void saveParameters()
    {
		SettingsManager.instance.saveParameters();
	}

	public void setQualitySetting(int value)
    {
        SettingsManager.instance.setQualitySetting(value);
	}

	public void setInteraction(int value)
	{
		SettingsManager.instance.setInteraction(value);
	}

	public void increaseUISize()
	{
		SettingsManager.instance.increaseUISize();
	}

	public void decreaseUISize()
	{
		SettingsManager.instance.decreaseUISize();
	}

	public void setWallTransparency(int value)
    {
		SettingsManager.instance.setWallTransparency(value);
    }

	public void setGameView(int value)
    {
		SettingsManager.instance.setGameView(value);
    }

	public void hookListener(string key)
    {
		SettingsManager.instance.hookListener(key);
	}

	public void setBorderTickness(int value)
	{
		SettingsManager.instance.setBorderTickness(value);
	}

	public void syncFonts(int value)
	{
		SettingsManager.instance.syncFonts(value);
	}

	public void setCaretWidth(int value)
	{
		SettingsManager.instance.setCaretWidth(value);
	}

	public void setCaretHeight(int value)
	{
		SettingsManager.instance.setCaretHeight(value);
	}
}
