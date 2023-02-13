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

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class DialogSystem : FSystem
{
	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	public GameObject LevelGO;
	private GameData gameData;
	public GameObject dialogPanel;
	public GameObject showDialogsMenu;
	public GameObject showDialogsBottom;
	private int nDialog = 0;

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();

			// Always disable bottom button, it will be enabled at the end of the dialogs (see Ok button)
			GameObjectManager.setGameObjectState(showDialogsBottom, false);
			if (gameData.levelToLoad.overridedDialogs.Count == 0)
				showDialogsMenu.GetComponent<Button>().interactable = false;
            else
				showDialogsMenu.GetComponent<Button>().interactable = true;
		}

		f_playingMode.addEntryCallback(delegate {
			GameObjectManager.setGameObjectState(showDialogsBottom, false);
		});

		f_editingMode.addEntryCallback(delegate {
			if (gameData.levelToLoad.overridedDialogs.Count > 0)
				GameObjectManager.setGameObjectState(showDialogsBottom, true);
		});

		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		//Activate DialogPanel if there is a message
		if (gameData != null && nDialog < gameData.levelToLoad.overridedDialogs.Count && !dialogPanel.transform.parent.gameObject.activeSelf)
			showDialogPanel();
	}


	// Affiche le panneau de dialoge au début de niveau (si besoin)
	public void showDialogPanel()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, true);
		nDialog = 0;

		string content = configureDialog();
		GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
		{
			verb = "openned",
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
		nDialog++; // On incrémente le nombre de dialogue

		string content = configureDialog();

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
		nDialog--; // On décrémente le nombre de dialogue

		string content = configureDialog();
	}

	private string configureDialog()
    {
		string dialogReturn = "";
		// set text
		GameObject textGO = dialogPanel.transform.Find("Text").gameObject;
		if (gameData.levelToLoad.overridedDialogs[nDialog].text != null)
		{
			GameObjectManager.setGameObjectState(textGO, true);
			textGO.GetComponent<TextMeshProUGUI>().text = gameData.levelToLoad.overridedDialogs[nDialog].text;
			LayoutRebuilder.ForceRebuildLayoutImmediate(textGO.transform as RectTransform);
			dialogReturn = gameData.levelToLoad.overridedDialogs[nDialog].text;
		}
		else
			GameObjectManager.setGameObjectState(textGO, false);
		// set image
		GameObject imageGO = dialogPanel.transform.Find("Image").gameObject;
		if (gameData.levelToLoad.overridedDialogs[nDialog].img != null)
		{
			if (gameData.levelToLoad.overridedDialogs[nDialog].img.ToLower().StartsWith("http"))
				MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), gameData.levelToLoad.overridedDialogs[nDialog].img));
			else
			{
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					Uri uri = new Uri(gameData.levelToLoad.src);
					MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments[uri.Segments.Length - 1].Length) + "Images/" + gameData.levelToLoad.overridedDialogs[nDialog].img));
				}
				else
					MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), Path.GetDirectoryName(gameData.levelToLoad.src) + "/Images/" + gameData.levelToLoad.overridedDialogs[nDialog].img));
			}
		}
		else
			GameObjectManager.setGameObjectState(imageGO, false);
		// set camera pos
		if (gameData.levelToLoad.overridedDialogs[nDialog].camX != -1 && gameData.levelToLoad.overridedDialogs[nDialog].camY != -1)
        {
			GameObjectManager.addComponent<FocusCamOn>(MainLoop.instance.gameObject, new { camX = gameData.levelToLoad.overridedDialogs[nDialog].camX, camY = gameData.levelToLoad.overridedDialogs[nDialog].camY });
		}
		// set sound
		AudioSource audio = dialogPanel.GetComponent<AudioSource>();
		if (gameData.levelToLoad.overridedDialogs[nDialog].sound != null)
		{
			if (gameData.levelToLoad.overridedDialogs[nDialog].sound.ToLower().StartsWith("http"))
				MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), gameData.levelToLoad.overridedDialogs[nDialog].sound));
			else
			{
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					Uri uri = new Uri(gameData.levelToLoad.src);
					MainLoop.instance.StartCoroutine(GetAudioWebRequest(audio, uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments[uri.Segments.Length - 1].Length) + "Sounds/" + gameData.levelToLoad.overridedDialogs[nDialog].sound));
				}
				else
					MainLoop.instance.StartCoroutine(GetAudioWebRequest(audio, Path.GetDirectoryName(gameData.levelToLoad.src) + "/Sounds/" + gameData.levelToLoad.overridedDialogs[nDialog].sound));
			}
		}
		else
			audio.Stop();
		// set video
		VideoPlayer videoPlayer = dialogPanel.GetComponentInChildren<VideoPlayer>(true);
		if (gameData.levelToLoad.overridedDialogs[nDialog].video != null)
		{
			Debug.Log(gameData.levelToLoad.overridedDialogs[nDialog].video);
			GameObjectManager.setGameObjectState(videoPlayer.gameObject, true);
			videoPlayer.url = gameData.levelToLoad.overridedDialogs[nDialog].video;
			RawImage rawImage = dialogPanel.GetComponentInChildren<RawImage>(true);
			rawImage.enabled = false;
			MainLoop.instance.StartCoroutine(waitLoadingVideo());
		}
		else
			GameObjectManager.setGameObjectState(videoPlayer.gameObject, false);
		// set background
		dialogPanel.transform.parent.GetComponent<Image>().enabled = !gameData.levelToLoad.overridedDialogs[nDialog].enableInteraction;

		// Be sure all buttons are disabled
		setActiveOKButton(false);
		setActiveNextButton(false);
		setActivePrevButton(false);

		if (nDialog + 1 < gameData.levelToLoad.overridedDialogs.Count)
			setActiveNextButton(true);
		if (nDialog > 0)
			setActivePrevButton(true);
		if (nDialog + 1 >= gameData.levelToLoad.overridedDialogs.Count)
			setActiveOKButton(true);
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
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("OKButton").gameObject, active);
	}


	// Active ou non le bouton next du panel dialogue
	public void setActiveNextButton(bool active)
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("NextButton").gameObject, active);
	}


	// Active ou non le bouton next du panel dialogue
	public void setActivePrevButton(bool active)
	{
		dialogPanel.transform.Find("Buttons").Find("PrevButton").gameObject.GetComponent<Button>().interactable = active;
	}


	// See OKButton in editor
	// Désactive le panel de dialogue
	public void closeDialogPanel()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
		nDialog = gameData.levelToLoad.overridedDialogs.Count;

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
			Debug.Log(www.error);
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
		float maxHeight = Screen.height - (rectParent.sizeDelta.y + rectParent.anchoredPosition.y * 2); // compute available space
		if (gameData.levelToLoad.overridedDialogs[nDialog].imgHeight != -1)
			((RectTransform)img.transform).sizeDelta = new Vector2(((RectTransform)img.transform).sizeDelta.x, Math.Min(gameData.levelToLoad.overridedDialogs[nDialog].imgHeight, maxHeight));
		else
			((RectTransform)img.transform).sizeDelta = new Vector2(((RectTransform)img.transform).sizeDelta.x, Math.Min(img.GetComponent<LayoutElement>().preferredHeight, maxHeight));
		GameObjectManager.setGameObjectState(img.gameObject, true); // Know we can show image
	}
}