using UnityEngine;
using FYFY;
using TMPro;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System;

/// <summary>
/// This system manage the settings window
/// </summary>
public class SettingsManager : FSystem {

	public static SettingsManager instance;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern bool ClearPlayerPrefs(); // call javascript

	public Transform settingsPanel;
	public CanvasScaler [] canvasScaler;

	private TMP_Text currentSizeText;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && ClearPlayerPrefs())
			PlayerPrefs.DeleteAll();

		settingsPanel.Find("Quality").GetComponentInChildren<TMP_Dropdown>().value = PlayerPrefs.GetInt("quality", 2);
		settingsPanel.Find("InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : 0);
		
		// définition de la taille de l'interface
		currentSizeText = settingsPanel.Find("UISize").Find("CurrentSize").GetComponent<TMP_Text>();
		
		float currentScale = PlayerPrefs.GetFloat("UIScale", (float)Math.Max(1, Math.Round((double)Screen.currentResolution.width / 2048, 2))); // do not reduce scale under 1 and multiply scale for definition higher than 2048

		currentSizeText.text = currentScale + "";
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = currentScale;
		PlayerPrefs.SetFloat("UIScale", currentScale);
		PlayerPrefs.Save();
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
		PlayerPrefs.Save();
	}

	public void setInteraction(int value)
	{
		PlayerPrefs.SetInt("interaction", value);
		PlayerPrefs.Save();
	}

	public void increaseUISize()
    {
		float newScale = PlayerPrefs.GetFloat("UIScale", 1f)+0.25f;
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = newScale;
		PlayerPrefs.SetFloat("UIScale", newScale);
		currentSizeText.text = newScale+"";
		PlayerPrefs.Save();
	}
	public void decreaseUISize()
	{
		float currentScale = PlayerPrefs.GetFloat("UIScale", 1f);
		if (currentScale >= 0.5f) 
			currentScale -= 0.25f;
		foreach (CanvasScaler canvas in canvasScaler)
				canvas.scaleFactor = currentScale;
		PlayerPrefs.SetFloat("UIScale", currentScale);
		currentSizeText.text = currentScale + "";
		PlayerPrefs.Save();
	}

}
