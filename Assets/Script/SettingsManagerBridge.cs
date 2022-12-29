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

	public void setUISize(int value)
	{
		SettingsManager.instance.setUISize(value);
	}
}
