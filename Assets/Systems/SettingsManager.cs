using UnityEngine;
using FYFY;
using System.Collections;
using TMPro;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.UI;

/// <summary>
/// This system check if the end of the level is reached and display end panel accordingly
/// </summary>
public class SettingsManager : FSystem {

	public static SettingsManager instance;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern void ToggleFullScreen(bool isFullScreen); // call javascript

	public Transform settingsPanel;
	public CanvasScaler canvasScaler = null;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObjectManager.setGameObjectState(settingsPanel.Find("InteractionMode").gameObject, Application.platform == RuntimePlatform.WebGLPlayer);
		settingsPanel.Find("Quality").GetComponentInChildren<TMP_Dropdown>().value = PlayerPrefs.GetInt("quality", 2);
		settingsPanel.Find("InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : 0);
		settingsPanel.Find("UISize").GetComponentInChildren<TMP_Dropdown>().value = PlayerPrefs.GetInt("UISize", 0);
	}

	public void setQualitySetting(int value)
	{
		QualitySettings.SetQualityLevel(value);
		switch (value)
		{
			case 0:
				Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier1;
				break;
			case 1:
				Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier2;
				break;
			case 2:
				Graphics.activeTier = UnityEngine.Rendering.GraphicsTier.Tier3;
				break;
		}
		PlayerPrefs.SetInt("quality", value);
	}

	public void setInteraction(int value)
	{
		PlayerPrefs.SetInt("interaction", value);
	}

	public void setUISize(int value)
	{
		if (canvasScaler != null)
			canvasScaler.uiScaleMode = value == 1 ? CanvasScaler.ScaleMode.ConstantPhysicalSize : CanvasScaler.ScaleMode.ConstantPixelSize;
		PlayerPrefs.SetInt("UISize", value);
	}

}
