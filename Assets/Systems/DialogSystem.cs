using UnityEngine;
using FYFY;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Manage dialogs at the begining of the level
/// </summary>
public class DialogSystem : FSystem
{
	private GameData gameData;
	public GameObject dialogPanel;
	private int nDialog = 0;

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		//Activate DialogPanel if there is a message
		if (gameData != null && gameData.dialogMessage.Count > 0 && !dialogPanel.transform.parent.gameObject.activeSelf)
		{
			showDialogPanel();
		}
	}


	// Affiche le panneau de dialoge au début de niveau (si besoin)
	public void showDialogPanel()
	{
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, true);
		nDialog = 0;

		configureDialog();

		if (gameData.dialogMessage.Count > 1)
		{
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else
		{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}

	// See NextButton in editor
	// Permet d'afficher la suite du dialogue
	public void nextDialog()
	{
		nDialog++; // On incrémente le nombre de dialogue

		configureDialog();

		// Si il reste des dialogues à afficher ensuite
		if (nDialog + 1 < gameData.dialogMessage.Count)
		{
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else
		{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}

	private void configureDialog()
    {
		GameObject textGO = dialogPanel.transform.Find("Text").gameObject;
		if (gameData.dialogMessage[nDialog].Item1 != null)
		{
			GameObjectManager.setGameObjectState(textGO, true);
			textGO.GetComponent<TextMeshProUGUI>().text = gameData.dialogMessage[nDialog].Item1;
			if (gameData.dialogMessage[nDialog].Item2 != -1)
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, gameData.dialogMessage[nDialog].Item2);
			else
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, textGO.GetComponent<LayoutElement>().preferredHeight);
		}
		else
			GameObjectManager.setGameObjectState(textGO, false);
		GameObject imageGO = dialogPanel.transform.Find("Image").gameObject;
		if (gameData.dialogMessage[nDialog].Item3 != null)
		{
			GameObjectManager.setGameObjectState(imageGO, true);
			setImageSprite(imageGO.GetComponent<Image>(), Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			Path.DirectorySeparatorChar + gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + "Images" + Path.DirectorySeparatorChar + gameData.dialogMessage[nDialog].Item3);
			if (gameData.dialogMessage[nDialog].Item4 != -1)
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, gameData.dialogMessage[nDialog].Item4);
			else
				((RectTransform)textGO.transform).sizeDelta = new Vector2(((RectTransform)textGO.transform).sizeDelta.x, textGO.GetComponent<LayoutElement>().preferredHeight);
		}
		else
			GameObjectManager.setGameObjectState(imageGO, false);
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


	// See OKButton in editor
	// Désactive le panel de dialogue et réinitialise le nombre de dialogue à 0
	public void closeDialogPanel()
	{
		nDialog = 0;
		gameData.dialogMessage = new List<(string, float, string, float)>();
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}

	// Affiche l'image associée au dialogue
	public void setImageSprite(Image img, string path)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			MainLoop.instance.StartCoroutine(GetTextureWebRequest(img, path));
		}
		else
		{
			Texture2D tex2D = new Texture2D(2, 2); //create new "empty" texture
			byte[] fileData = File.ReadAllBytes(path); //load image from SPY/path
			if (tex2D.LoadImage(fileData))
				img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
		}
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
		}
	}
}