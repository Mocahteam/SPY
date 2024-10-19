using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.EventSystems;

/// <summary>
/// Manage dialogs at the begining and end of the level
/// </summary>
public class DialogSystem : FSystem
{
	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));
	private Family f_ends = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	public GameObject LevelGO;
	private GameData gameData;
	public GameObject dialogPanel;
	public GameObject showDialogsMenu;
	public GameObject showDialogsBottom;
	private int nBriefingDialog = 0; // the briefing currently view
	private int nDebriefingWinDialog = 0; // the debriefing (win) currently view
	private int nDebriefingDefeatDialog = 0; // the debriefing (defeat) currently view
	private List<Dialog> overridedBriefingDialogs = new List<Dialog>();
	private List<Dialog> overridedDebriefingWinDialogs = new List<Dialog>();
	private List<Dialog> overridedDebriefingDefeatDialogs = new List<Dialog>();

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();
			// Always disable bottom button, it will be enabled at the end of the dialogs (see Ok button)
			GameObjectManager.setGameObjectState(showDialogsBottom, false);
			// Count number of briefing and debriefing dialogs
			List<Dialog> tmpList = gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].overridedDialogs;
			if (tmpList != null) {
				foreach (Dialog d in tmpList)
				{
					if (d.briefingType == 1)
						overridedDebriefingWinDialogs.Add(d);
					else if (d.briefingType == 2)
						overridedDebriefingDefeatDialogs.Add(d);
					else
						overridedBriefingDialogs.Add(d);
				}
			}
			// Set interactable depending on briefing dialogs count
			showDialogsMenu.GetComponent<Button>().interactable = overridedBriefingDialogs.Count != 0;
		}

		f_playingMode.addEntryCallback(delegate {
			GameObjectManager.setGameObjectState(showDialogsBottom, false);
		});

		f_editingMode.addEntryCallback(delegate {
			if (overridedBriefingDialogs.Count > 0)
				GameObjectManager.setGameObjectState(showDialogsBottom, true);
		});

		f_ends.addEntryCallback(delegate
		{
			if (dialogPanel.activeInHierarchy)
				closeDialogPanel();
			// Afficher la fenêtre de fin s'il y a au moins un dialogue de fin de configuré
			if (overridedDebriefingWinDialogs.Count > 0)
				showDialogPanel();
		});

		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		//Activate DialogPanel if there is a message
		if (gameData != null && !dialogPanel.transform.parent.gameObject.activeSelf && (
				(f_ends.Count == 0 && overridedBriefingDialogs != null && nBriefingDialog < overridedBriefingDialogs.Count) ||
				(f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win && overridedDebriefingWinDialogs != null && nDebriefingWinDialog < overridedDebriefingWinDialogs.Count) ||
				(f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win && overridedDebriefingDefeatDialogs != null && nDebriefingDefeatDialog < overridedDebriefingDefeatDialogs.Count)))
			showDialogPanel();
	}


	// Affiche le panneau de dialoge au début de niveau (si besoin)
	public void showDialogPanel()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, true);
		nBriefingDialog = f_ends.Count == 0 ? 0 : nBriefingDialog;
		nDebriefingWinDialog = f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win ? 0 : nDebriefingWinDialog;
		nDebriefingDefeatDialog = f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win ? 0 : nDebriefingDefeatDialog;

		string content = configureDialog(0);
		GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
		{
			verb = "opened",
			objectType = "briefing",
			activityExtensions = new Dictionary<string, string>() {
				{ "content", content }
			}
		});
	}

	// See NextButton in editor
	// Permet d'afficher la suite du dialogue
	public void nextDialog()
	{
		// On se positionne sur le prochain dialogue
		nBriefingDialog += f_ends.Count == 0 ? 1 : 0;
		nDebriefingWinDialog += f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win ? 1 : 0;
		nDebriefingDefeatDialog += f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win ? 1 : 0;

		string content = configureDialog(1);

		GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
		{
			verb = "interacted",
			objectType = "briefing",
			activityExtensions = new Dictionary<string, string>() {
				{ "value", "next" },
				{ "content", content }
			}
		});
	}

	// See PreviousButton in editor
	// Permet d'afficher le message précédent
	public void prevDialog()
	{
		// On se positionne sur le prochain dialogue
		nBriefingDialog -= f_ends.Count == 0 ? 1 : 0;
		nDebriefingWinDialog -= f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win ? 1 : 0;
		nDebriefingDefeatDialog -= f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win ? 1 : 0;

		string content = configureDialog(-1);

		GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
		{
			verb = "interacted",
			objectType = "briefing",
			activityExtensions = new Dictionary<string, string>() {
				{ "value", "previous" },
				{ "content", content }
			}
		});
	}

	private string configureDialog(int way)
    {
		string dialogReturn = "";
		// get Dialog
		Dialog dialog = f_ends.Count == 0 ? overridedBriefingDialogs[nBriefingDialog] : (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win ? overridedDebriefingWinDialogs[nDebriefingWinDialog] : overridedDebriefingDefeatDialogs[nDebriefingDefeatDialog]);
		// set text
		GameObject textGO = dialogPanel.transform.Find("Text").gameObject;
		if (dialog.text != null)
		{
			GameObjectManager.setGameObjectState(textGO, true);
			string localeDependent = Utility.extractLocale(dialog.text);
			textGO.GetComponent<TextMeshProUGUI>().text = localeDependent;
			LayoutRebuilder.ForceRebuildLayoutImmediate(textGO.transform as RectTransform);
			dialogReturn = localeDependent;
		}
		else
			GameObjectManager.setGameObjectState(textGO, false);
		// set image
		GameObject imageGO = dialogPanel.transform.Find("Image").gameObject;
		if (dialog.img != null)
		{
			string localeDependent = Utility.extractLocale(dialog.img);
			if (localeDependent.ToLower().StartsWith("http"))
				MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), localeDependent));
			else
			{
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					Uri uri = new Uri(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].src);
					MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments[uri.Segments.Length - 1].Length) + "Images/" + localeDependent));
				}
				else
					MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), Path.GetDirectoryName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].src) + "/Images/" + localeDependent));
			}
			dialogReturn += (dialogReturn != "" ? "\n" : "") + localeDependent;
		}
		else
			GameObjectManager.setGameObjectState(imageGO, false);
		// set camera pos
		if (dialog.camX != -1 && dialog.camY != -1)
        {
			GameObjectManager.addComponent<FocusCamOn>(MainLoop.instance.gameObject, new { camX = dialog.camX, camY = dialog.camY });
		}
		// set sound
		AudioSource audio = dialogPanel.GetComponent<AudioSource>();
		audio.Stop();
		if (dialog.sound != null)
		{
			string localeDependent = Utility.extractLocale(dialog.sound);
			if (localeDependent.ToLower().StartsWith("http"))
				MainLoop.instance.StartCoroutine(GetAudioWebRequest(audio, localeDependent));
			else if (localeDependent != "")
			{
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					Uri uri = new Uri(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].src);
					MainLoop.instance.StartCoroutine(GetAudioWebRequest(audio, uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments[uri.Segments.Length - 1].Length) + "Sounds/" + localeDependent));
				}
				else
					MainLoop.instance.StartCoroutine(GetAudioWebRequest(audio, Path.GetDirectoryName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].src) + "/Sounds/" + localeDependent));
			}
			dialogReturn += (dialogReturn != "" ? "\n" : "") + localeDependent;
		}
		// set video
		VideoPlayer videoPlayer = dialogPanel.GetComponentInChildren<VideoPlayer>(true);
		if (dialog.video != null)
		{
			string localeDependent = Utility.extractLocale(dialog.video);
			if (localeDependent != "")
			{
				GameObjectManager.setGameObjectState(videoPlayer.gameObject, true);
				videoPlayer.url = localeDependent;
				RawImage rawImage = dialogPanel.GetComponentInChildren<RawImage>(true);
				rawImage.enabled = false;
				MainLoop.instance.StartCoroutine(waitLoadingVideo());
			}
			else
				GameObjectManager.setGameObjectState(videoPlayer.gameObject, false);
			dialogReturn += (dialogReturn != "" ? "\n" : "") + localeDependent;
		}
		else
			GameObjectManager.setGameObjectState(videoPlayer.gameObject, false);
		// tag DEBRIEFING if it is the case
		if (f_ends.Count > 0)
			dialogReturn += (dialogReturn != "" ? "\nDEBRIEFING" : "DEBRIEFING");


		// set background
		dialogPanel.transform.parent.GetComponent<Image>().enabled = !dialog.enableInteraction;

		// Be sure all buttons are disabled
		setActiveOKButton(false);
		setActiveNextButton(false);
		setActivePrevButton(false);

		// if way is > 0 means we pass to next dialog => process previous dialog first in order to put selected go on ok/next button
		if (way > 0)
			if ((f_ends.Count == 0 && nBriefingDialog > 0) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win && nDebriefingWinDialog > 0) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win && nDebriefingDefeatDialog > 0))
				setActivePrevButton(true);

		if ((f_ends.Count == 0 && nBriefingDialog + 1 < overridedBriefingDialogs.Count) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win && nDebriefingWinDialog + 1 < overridedDebriefingWinDialogs.Count) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win && nDebriefingDefeatDialog + 1 < overridedDebriefingDefeatDialogs.Count))
			setActiveNextButton(true);
		if ((f_ends.Count == 0 && nBriefingDialog + 1 >= overridedBriefingDialogs.Count) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win && nDebriefingWinDialog + 1 >= overridedDebriefingWinDialogs.Count) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win && nDebriefingDefeatDialog + 1 >= overridedDebriefingDefeatDialogs.Count))
			setActiveOKButton(true);

		// if way is < 0 means we pass to previous dialog => process previous dialog in second to put selected go on previous button
		if (way < 0)
			if ((f_ends.Count == 0 && nBriefingDialog > 0) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win && nDebriefingWinDialog > 0) || (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win && nDebriefingDefeatDialog > 0))
				setActivePrevButton(true);

		return dialogReturn;
	}

	private IEnumerator waitLoadingVideo()
    {
		VideoPlayer videoPlayer = dialogPanel.GetComponentInChildren<VideoPlayer>(true);
		while (!videoPlayer.isPrepared)
			yield return null;
		setVideoSize();
	}

	private void setVideoSize()
    {
		VideoPlayer videoPlayer = dialogPanel.GetComponentInChildren<VideoPlayer>(true);
		RawImage rawImage = dialogPanel.GetComponentInChildren<RawImage>(true);
		rawImage.enabled = true;
		rawImage.rectTransform.sizeDelta = new Vector2(videoPlayer.width * rawImage.rectTransform.rect.height / videoPlayer.height, rawImage.rectTransform.rect.height);
	}

	// Active ou non le bouton Ok du panel dialogue
	public void setActiveOKButton(bool active)
	{
		GameObject okButtons = dialogPanel.transform.Find("Buttons").Find("OKButton").gameObject;
		GameObjectManager.setGameObjectState(okButtons, active);
		if (active)
			EventSystem.current.SetSelectedGameObject(okButtons);
	}


	// Active ou non le bouton next du panel dialogue
	public void setActiveNextButton(bool active)
	{
		GameObject nextButton = dialogPanel.transform.Find("Buttons").Find("NextButton").gameObject;
		GameObjectManager.setGameObjectState(nextButton, active);
		if (active)
			EventSystem.current.SetSelectedGameObject(nextButton);
	}


	// Active ou non le bouton next du panel dialogue
	public void setActivePrevButton(bool active)
	{
		GameObject prevButton = dialogPanel.transform.Find("Buttons").Find("PrevButton").gameObject;
		prevButton.GetComponent<Button>().interactable = active;
		if (active)
			EventSystem.current.SetSelectedGameObject(prevButton);
	}


	// See OKButton in editor
	// Désactive le panel de dialogue
	public void closeDialogPanel()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
		nBriefingDialog = f_ends.Count == 0 ? overridedBriefingDialogs.Count : 0;
		nDebriefingWinDialog = f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win ? overridedDebriefingWinDialogs.Count : 0;
		nDebriefingDefeatDialog = f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win ? overridedDebriefingDefeatDialogs.Count : 0;

		GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
		{
			verb = "closed",
			objectType = "briefing"
		});
	}

	private IEnumerator GetTextureWebRequest(Image img, string path)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			yield return new WaitForSeconds(0.5f);
			MainLoop.instance.StartCoroutine(GetTextureWebRequest(img, path));
		}
		else
		{
			Texture2D tex2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
			img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
			MainLoop.instance.StartCoroutine(setWantedHeight(img));
		}
	}

	private IEnumerator GetAudioWebRequest(AudioSource audio, string path)
	{
		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			yield return new WaitForSeconds(0.5f);
			MainLoop.instance.StartCoroutine(GetAudioWebRequest(audio, path));
		}
		else
		{
			audio.clip = DownloadHandlerAudioClip.GetContent(www);
			audio.Play();
		}
	}

	private IEnumerator setWantedHeight(Image img)
	{
		img.gameObject.SetActive(false); // Force disabling image to compute panel height with only buttons and text
		yield return new WaitForSeconds(0.1f); // take time to update UI
		RectTransform rectParent = (RectTransform)img.transform.parent;
		float maxHeight = Screen.height - (rectParent.sizeDelta.y + 20); // compute available space add 20 to include top and bottom margin
		// get Dialog
		Dialog dialog = f_ends.Count == 0 ? overridedBriefingDialogs[nBriefingDialog] : (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win ? overridedDebriefingWinDialogs[nDebriefingWinDialog] : overridedDebriefingDefeatDialogs[nDebriefingDefeatDialog]);
		if (dialog.imgHeight != -1)
			((RectTransform)img.transform).sizeDelta = new Vector2(((RectTransform)img.transform).sizeDelta.x, Math.Min(dialog.imgHeight, maxHeight));
		else
			((RectTransform)img.transform).sizeDelta = new Vector2(((RectTransform)img.transform).sizeDelta.x, Math.Min(img.GetComponent<LayoutElement>().preferredHeight, maxHeight));
		GameObjectManager.setGameObjectState(img.gameObject, true); // now we can show image
	}
}