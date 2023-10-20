using UnityEngine;

// Used in ForBloc
public class SettingsManagerBridge : MonoBehaviour
{
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
}
