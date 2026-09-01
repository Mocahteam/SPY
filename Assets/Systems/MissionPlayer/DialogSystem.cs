using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using static System.Net.WebRequestMethods;
using static UnityEngine.UI.GridLayoutGroup;

/// <summary>
/// Manage dialogs at the begining and end of the level
/// </summary>
public class DialogSystem : FSystem
{
	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));
	private Family f_ends = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_fadeOutEnd = FamilyManager.getFamily(new AllOfComponents(typeof(FadeOutEnd)));

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

    private RectTransform dialogPanelTransform;
    private RectTransform windowTransform;
    private RectTransform viewportTransform;
    private RectTransform contentTransform;
    private RectTransform imgTransform;
    private RectTransform videoTransform;
    private RectTransform buttonsTransform;
	private VideoPlayer videoPlayer;

	private Coroutine loadingImg;
    private Coroutine loadingSound;

    [DllImport("__Internal")]
    private static extern void PlaySound(string url); // call javascript

    [DllImport("__Internal")]
    private static extern void StopSound(); // call javascript

    [DllImport("__Internal")]
    private static extern void SetCinematic(string url); // call javascript

    [DllImport("__Internal")]
    private static extern void PlayCinematic(); // call javascript

    [DllImport("__Internal")]
    private static extern void PauseCinematic(); // call javascript

    [DllImport("__Internal")]
    private static extern void StopCinematic(); // call javascript

    [DllImport("__Internal")]
    private static extern int GetVideoWidth(); // call javascript

    [DllImport("__Internal")]
    private static extern int GetVideoHeight(); // call javascript

    [DllImport("__Internal")]
    private static extern int SetVideoPosition(int viewportX, int viewportY, int viewportWidth, int viewportHeight, int videoX, int videoY, int videoWidth, int videoHeight); // call javascript

    protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();
			// Always disable bottom button, it will be enabled at the end of the dialogs (see Ok button)
			GameObjectManager.setGameObjectState(showDialogsBottom.transform.parent.gameObject, false);
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

            dialogPanelTransform = dialogPanel.transform as RectTransform;
            windowTransform = dialogPanelTransform.parent as RectTransform;
            viewportTransform = dialogPanelTransform.Find("Scroll View/Viewport") as RectTransform;
            contentTransform = viewportTransform.Find("Content") as RectTransform;
            imgTransform = contentTransform.Find("Image") as RectTransform;
            videoTransform = contentTransform.Find("Video Player") as RectTransform;
            buttonsTransform = dialogPanelTransform.Find("Buttons").transform as RectTransform;
			
			videoPlayer = dialogPanel.GetComponentInChildren<VideoPlayer>(true);
        }

		f_playingMode.addEntryCallback(delegate {
			GameObjectManager.setGameObjectState(showDialogsBottom.transform.parent.gameObject, false);
		});

		f_editingMode.addEntryCallback(delegate {
			if (overridedBriefingDialogs.Count > 0)
				GameObjectManager.setGameObjectState(showDialogsBottom.transform.parent.gameObject, true);
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
				(f_ends.Count == 0 && overridedBriefingDialogs != null && nBriefingDialog < overridedBriefingDialogs.Count && f_fadeOutEnd.Count == 1) ||
				(f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win && overridedDebriefingWinDialogs != null && nDebriefingWinDialog < overridedDebriefingWinDialogs.Count) ||
				(f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType != NewEnd.Win && overridedDebriefingDefeatDialogs != null && nDebriefingDefeatDialog < overridedDebriefingDefeatDialogs.Count)))
			showDialogPanel();

		if (dialogPanel.activeInHierarchy)
			updateDialogSize();
	}


	// Affiche le panneau de dialogue
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
		GameObject textGO = dialogPanel.transform.Find("Scroll View/Viewport/Content/Text").gameObject;
		if (dialog.text != null)
		{
			GameObjectManager.setGameObjectState(textGO, true);
			string localeDependent = Utility.extractLocale(dialog.text);
			textGO.GetComponent<TextMeshProUGUI>().text = localeDependent;
			dialogReturn = localeDependent;
		}
		else
			GameObjectManager.setGameObjectState(textGO, false);

		// set image
		GameObject imageGO = dialogPanel.transform.Find("Scroll View/Viewport/Content/Image").gameObject;
        if (loadingImg != null)
            MainLoop.instance.StopCoroutine(loadingImg);
        if (dialog.img != null)
		{
            GameObjectManager.setGameObjectState(imageGO, true);
			string localeDependent = Utility.extractLocale(dialog.img);
			if (localeDependent.ToLower().StartsWith("http"))
				loadingImg = MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), localeDependent, dialog));
			else
			{
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					Uri uri = new Uri(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].filePath);
                    loadingImg = MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments[uri.Segments.Length - 1].Length) + "Images/" + localeDependent, dialog));
				}
				else
                    loadingImg = MainLoop.instance.StartCoroutine(GetTextureWebRequest(imageGO.GetComponent<Image>(), Path.GetDirectoryName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].filePath) + "/Images/" + localeDependent, dialog));
			}
			dialogReturn += (dialogReturn != "" ? "\n" : "") + localeDependent;
		}
		else
			GameObjectManager.setGameObjectState(imageGO, false);
		// set imgDesc
		if (dialog.imgDesc != null)
		{
			GameObject imgDescGO = dialogPanel.transform.Find("Scroll View/Viewport/Content/Image").gameObject;
			imgDescGO.GetComponent<ImgReplacementText>().replacementText = Utility.extractLocale(dialog.imgDesc);
		}

		// set camera pos
		if (dialog.camX != -1 && dialog.camY != -1)
        {
			GameObjectManager.addComponent<FocusCamOn>(MainLoop.instance.gameObject, new { camX = dialog.camX, camY = dialog.camY });
		}

		// set sound
		AudioSource audio = dialogPanel.GetComponent<AudioSource>();
        // Au cas où un son serait en cours de lecture, on le stoppe
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            StopSound();
		else
            audio.Stop();
        if (loadingSound != null)
            MainLoop.instance.StopCoroutine(loadingSound);
        if (dialog.sound != null)
		{
			string path = Utility.extractLocale(dialog.sound);
			if (path != "")
			{
				if (!path.ToLower().StartsWith("http"))
				{
					if (Application.platform == RuntimePlatform.WebGLPlayer)
					{
						Uri uri = new Uri(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].filePath);
						path = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments[uri.Segments.Length - 1].Length) + "Sounds/" + path;
					}
					else
						path = Path.GetDirectoryName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].filePath) + "/Sounds/" + path;
				}
				// voir commentaire ci-dessous sur les vidéo à propos du CORS (même problème ici)
				if (Application.platform == RuntimePlatform.WebGLPlayer)
					PlaySound(path);
				else
                    MainLoop.instance.StartCoroutine(GetAudioWebRequest(audio, path));
				dialogReturn += (dialogReturn != "" ? "\n" : "") + path;
			}
		}

		// set video
        // Au cas où une vidéo serait en cours de lecture, on la stoppe
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            StopCinematic();
        else
            videoPlayer.Stop();
        if (dialog.video != null)
		{
			string path = Utility.extractLocale(dialog.video);
			if (path != "")
			{
                if (!path.ToLower().StartsWith("http"))
                {
                    if (Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        Uri uri = new Uri(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].filePath);
                        path = uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments[uri.Segments.Length - 1].Length) + "Videos/" + path;
                    }
                    else
                        path = Path.GetDirectoryName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].filePath) + "/Videos/" + path;
                }
				// En WebGL on délègue la lecture de la vidéo à la page html pour contourner les problèmes CORS et de CORB. En effet, demander à Unity hébergé sur spy.lip6.fr de charger avec une WebRequest une vidéo dans un autre domaine viole le principe de CORS car dans le cas d'Unity la vidéo pourrait être modifiée ce qui est bloqué par le navigateur. En déléguant la lecture de la vidéo au html via une balise <video> c'est tout à fait correct car là on garanti qu'on n'est qu'en mode lecture et qu'on ne va pas chercher à la modifier dans l'application. La limite de cette astuce est une perte d'accessibilité car pour le joueur il faut revenir au contexte html pour accéder aux boutons de contrôle de la vidéo (même si les boutons dans Unity (Play et Pause) restent actifs). C'est un compromis pour laisser la possibilité aux utilisateur de pouvoir pointer des ressources à l'extérieur de spy.lip6.fr.
				if (Application.platform == RuntimePlatform.WebGLPlayer)
					SetCinematic(path);
				else
                {
                    videoPlayer.url = HttpUtility.UrlDecode(path);
                    RawImage rawImage = dialogPanel.GetComponentInChildren<RawImage>(true);
                    rawImage.enabled = false;
                    MainLoop.instance.StartCoroutine(waitLoadingVideo(dialog));
                }
				// Que l'on soit en WebGL ou pas, on active le GO du videoPlayer pour s'en servir afin d'occuper la place dans le content du scrollview
                GameObjectManager.setGameObjectState(videoPlayer.gameObject, true);
                dialogReturn += (dialogReturn != "" ? "\n" : "") + path;
            }
			else
				GameObjectManager.setGameObjectState(videoPlayer.gameObject, false);
		}
		else
			GameObjectManager.setGameObjectState(videoPlayer.gameObject, false);

		// tag DEBRIEFING if it is the case
		if (f_ends.Count > 0)
			dialogReturn += (dialogReturn != "" ? "\nDEBRIEFING" : "");


		// set background
		dialogPanel.transform.parent.GetComponent<Image>().enabled = !dialog.enableInteraction;
		dialogPanel.transform.parent.parent.GetComponentInParent<CanvasGroup>().interactable = dialog.enableInteraction;

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

		if (dialog.text != null)
		{
			// On décalle la sélection du texte de briefing d'une frame pour laisser la prochaine phase de gestion des évènements passer (ce qui sélectionnerai certainement automatiquement le prochain bouton "suivant" ou "ok") afin d'être sûr de mettre le focus sur le texte de briefing
			MainLoop.instance.StartCoroutine(Utility.delayGOSelection(textGO));
		}

		MainLoop.instance.StartCoroutine(forceScrollBarUp());

		return dialogReturn;
	}

	// Active ou non le bouton Ok du panel dialogue
	public void setActiveOKButton(bool active)
	{
		GameObject okButton = dialogPanel.transform.Find("Buttons/OKButton").gameObject;
		GameObjectManager.setGameObjectState(okButton, active);
		if (active)
		{
			EventSystem.current.SetSelectedGameObject(okButton);
			// Définir le bouton ok comme le voisin de droite du bouton précédent
			GameObject prevButton = dialogPanel.transform.Find("Buttons/PrevButton").gameObject;
			Navigation nav = prevButton.GetComponent<Button>().navigation;
			nav.selectOnRight = okButton.GetComponent<Button>();
			prevButton.GetComponent<Button>().navigation = nav;
		}

	}


	// Active ou non le bouton next du panel dialogue
	public void setActiveNextButton(bool active)
	{
		GameObject nextButton = dialogPanel.transform.Find("Buttons/NextButton").gameObject;
		GameObjectManager.setGameObjectState(nextButton, active);
		if (active)
		{
			EventSystem.current.SetSelectedGameObject(nextButton);
			// Définir le bouton suivant comme le voisin de droite du bouton précédent
			GameObject prevButton = dialogPanel.transform.Find("Buttons/PrevButton").gameObject;
			Navigation nav = prevButton.GetComponent<Button>().navigation;
			nav.selectOnRight = nextButton.GetComponent<Button>();
			prevButton.GetComponent<Button>().navigation = nav;
		}
	}


	// Active ou non le bouton next du panel dialogue
	public void setActivePrevButton(bool active)
	{
		GameObject prevButton = dialogPanel.transform.Find("Buttons/PrevButton").gameObject;
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

        // Au cas où un son ou une vidéo seraient en cours de lecture, on les stoppe, en effet pour le contexte WebGL on déporte la lecture du média à la page html, il faut donc l'informer qu'il doit stopper parceque le briefing est terminé
        if (Application.platform == RuntimePlatform.WebGLPlayer) {
			StopSound();
			StopCinematic();
		}

        GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
		{
			verb = "closed",
			objectType = "briefing"
		});
    }

	public void playVideo()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
			PlayCinematic();
		else
			videoPlayer.Play();
	}

	public void pauseVideo()
	{
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            PauseCinematic();
        else
            videoPlayer.Pause();
	}

    private IEnumerator waitLoadingVideo(Dialog dialog)
    {
		while (!videoPlayer.gameObject.activeInHierarchy)
			yield return null;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

		// On fait un Play/Pause pour se positionner sur la première image de la cinématique
        videoPlayer.Play();
		while (videoPlayer.frame <= 0)
            yield return null;
		videoPlayer.Pause();

        // show rawImage that render video
        RawImage rawImage = dialogPanel.GetComponentInChildren<RawImage>(true);
        rawImage.enabled = true;
        yield return forceScrollBarUp();
    }

    private IEnumerator GetTextureWebRequest(Image img, string path, Dialog dialog)
	{
        // réinitialiser l'image avant de charger la nouvelle
        img.sprite = null;
        LayoutElement layout = img.GetComponent<LayoutElement>();
		layout.preferredHeight = 130;
        layout.preferredWidth = 130;

        // activer l'animation de chargement (spinner) pour l'image
        GameObjectManager.setGameObjectState(img.transform.GetChild(0).gameObject, true);
        UnityWebRequest www;
		// On passe par notre proxy pour charger une image commençant par http sauf si elle est chez nous (spy.lip6.fr)
        if (path.ToLower().StartsWith("http") && !path.ToLower().StartsWith("https://spy.lip6.fr"))
            www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/index_new_v2.php?file=" + HttpUtility.UrlEncode(path));
        else
            www = UnityWebRequestTexture.GetTexture(path);
        yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(path + " " + www.error);
			yield return new WaitForSeconds(0.5f);
		}
		else
		{
			Texture2D tex2D;
            if (path.ToLower().StartsWith("http") && !path.ToLower().StartsWith("https://spy.lip6.fr")) { 
                byte[] data = www.downloadHandler.data;
				tex2D = new Texture2D(2, 2); // Taille arbitraire, sera redimensionnée par LoadImage
                tex2D.LoadImage(data);
            }
			else
                tex2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
			img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
            // désactiver l'animation de chargement (spinner) pour l'image
            GameObjectManager.setGameObjectState(img.transform.GetChild(0).gameObject, false);
            yield return forceScrollBarUp();
        }
	}

	private IEnumerator GetAudioWebRequest(AudioSource audio, string path)
	{
        UnityWebRequest www;
        // On passe par notre proxy pour charger un son commençant par http sauf s'il est chez nous (spy.lip6.fr)
        if (path.ToLower().StartsWith("http") && !path.ToLower().StartsWith("https://spy.lip6.fr"))
        {
			string url = "https://spy.lip6.fr/ServerREST_LIP6/index_new_v2.php?file=" + HttpUtility.UrlEncode(path);
			www = UnityWebRequest.Get(url);
            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        }
		else
			www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
        yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(path + " " + www.error);
			yield return new WaitForSeconds(0.5f);
		}
		else
		{
			audio.PlayOneShot(DownloadHandlerAudioClip.GetContent(www));
        }
	}

	private int getVideoOriginalWidth()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
			return GetVideoWidth();
		else
			return (int)videoPlayer.width;
	}

    private int getVideoOriginalHeight()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return GetVideoHeight();
        else
            return (int)videoPlayer.width;
    }


    private void updateDialogSize()
	{
        // get Dialog
		Dialog dialog = f_ends.Count == 0 ? overridedBriefingDialogs[nBriefingDialog] : (f_ends.Count > 0 && f_ends.First().GetComponent<NewEnd>().endType == NewEnd.Win ? overridedDebriefingWinDialogs[nDebriefingWinDialog] : overridedDebriefingDefeatDialogs[nDebriefingDefeatDialog]);

		// Appliquer la taille maximale à la vidéo
		int videoOriginalWidth = getVideoOriginalWidth();
		int videoOriginalHeight = getVideoOriginalHeight();
		LayoutElement VideoLayout = videoTransform.GetComponent<LayoutElement>();
		float videoRatio = 1;
		if (videoOriginalHeight > 0)
		{
			videoRatio = (float)videoOriginalWidth / videoOriginalHeight;
			SetMaxPreferedSize(VideoLayout, dialog.videoHeight, videoRatio, videoOriginalWidth, videoOriginalHeight);
		}

		// Appliquer la taille maximale à l'image
		LayoutElement ImgLayout = imgTransform.GetComponent<LayoutElement>();
		float imgRatio = 1;
		Image img = imgTransform.GetComponent<Image>();
		if (img.sprite != null)
		{
            Texture2D tex2D = img.sprite.texture;
            imgRatio = (float)tex2D.width / tex2D.height;
            SetMaxPreferedSize(ImgLayout, dialog.imgHeight, imgRatio, tex2D.width, tex2D.height);
        }
        // Calcul de la taille maximale en largeur en fonction des GO affichés
        int scrollViewPadding = 2*10 + 2*15; // 2*10+2*15 respectivement pour les left et right du ScrollView et le padding left et right du verticalLayout du Content
        float newWidth = scrollViewPadding + Mathf.Max(imgTransform.gameObject.activeInHierarchy ? ImgLayout.preferredWidth : 0, videoTransform.gameObject.activeInHierarchy ? VideoLayout.preferredWidth : 0, buttonsTransform.rect.width); 
        // Si newWidth est plus grand que la taille de la fenêtre du jeu, on le réduit au maximum de la taille possible (tout en gardant une petite marge)
        if (newWidth >= windowTransform.rect.width - 60)
            newWidth = windowTransform.rect.width - 60;

        int dialogPadding = 10 + 55 + 2*15; // 10 et 55 respectivement pour le top et le bottom du ScrollView et 2*15 pour le top/bottom du content
        int dialogMargin = 18 + 40; // 18 de brodure du BackgroundChevron + 40 pour le Y du dialogPanel

        float currentWidthSpaceInContent = newWidth - scrollViewPadding;
        float maxHeightSpaceInPanel = windowTransform.rect.height - (dialogMargin + dialogPadding);
        
        // Pour éviter de déformer la vidéo et d'avoir à utiliser des scroll, on va limiter la taille de la vidéo à la taille de la fenêtre. On va donc vérifier si la vidéo est trop large ou trop haute pour la fenêtre et ajuster sa taille en conséquence.
        setAdjustedPreferredSize(VideoLayout, videoRatio, currentWidthSpaceInContent, maxHeightSpaceInPanel);
        // Pour l'image on veut juste la caler au pire sur la largeur du content, si elle est trop haute on laisse le scroll possible
        setAdjustedPreferredSize(ImgLayout, imgRatio, currentWidthSpaceInContent, Mathf.Infinity);

        float newHeight = dialogPadding + contentTransform.rect.height;
        if (newHeight > windowTransform.rect.height - dialogMargin)
            newHeight = windowTransform.rect.height - dialogMargin;
        dialogPanelTransform.sizeDelta = new Vector2(newWidth, newHeight);

		// envoyer les bonnes tailles et positions de la vidéo au html
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Vector3[] corners = new Vector3[4];

            // Calcul de la position du viewport dans l'écran
			Rect viewportRect = GetScreenPos(viewportTransform);
            // Calcul de la position de la vidéo dans l'écran
            Rect videoRect = GetScreenPos(videoTransform);
            // Comme dans le html la vidéo est positionnée comme enfant du viewport (pour avoir l'effet de clip sur le scroll), on doit recaler la position de la vidéo par rapport au viewport et non pas par rapport à l'écran. On va donc soustraire la position du viewport à celle de la vidéo.
            videoRect.x = videoRect.x - viewportRect.x;
            videoRect.y = videoRect.y - viewportRect.y;

            SetVideoPosition((int)viewportRect.x, (int)viewportRect.y, (int)viewportRect.width, (int)viewportRect.height, (int)videoRect.x, (int)videoRect.y, (int)videoRect.width, (int)videoRect.height);
		}
    }

	private Rect GetScreenPos(RectTransform transform)
    {
        Vector3[] corners = new Vector3[4];
        // Remplit le tableau dans l'ordre : bas-gauche, haut-gauche, haut-droite, bas-droite
        transform.GetWorldCorners(corners);
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
        return new Rect(
            bottomLeft.x,
            Screen.height - topRight.y, // pour passer du repère de Unity (x vers droite et y vers le haut) au répère de l'écran (x vers droite et y vers le bas)
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y
        );

    }

    private void SetMaxPreferedSize(LayoutElement layout, float requestedHeight, float ratio, float defaultWidth, float defaultHeight)
    {
        if (layout.gameObject.activeInHierarchy)
        {
            if (requestedHeight != -1)
            {
                layout.preferredHeight = requestedHeight;
                layout.preferredWidth = requestedHeight * ratio;
            }
            else
            {
                layout.preferredHeight = defaultHeight;
                layout.preferredWidth = defaultWidth;
            }
        }
    }

	private void setAdjustedPreferredSize(LayoutElement layout, float ratio, float currentWidth, float maxHeight)
	{
        if (layout.gameObject.activeInHierarchy)
        {
            // Si le layout est trop large pour le content, on ajuste la taille du layout en fonction du ratio
            if (layout.preferredWidth > currentWidth)
            {
                layout.preferredWidth = currentWidth;
                layout.preferredHeight = currentWidth / ratio;
            }

            // Si le layout est trop haut pour le panel, on ajuste la taille du layout en fonction du ratio
            if (layout.preferredHeight > maxHeight)
            {
                layout.preferredHeight = maxHeight;
                layout.preferredWidth = maxHeight * ratio;
            }
        }
    }

    private IEnumerator forceScrollBarUp()
	{
		yield return null;
		yield return null;
		dialogPanel.GetComponentInChildren<Scrollbar>(true).value = 1f;
	}
}