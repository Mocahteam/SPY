using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Manage popup windows to display messages to the user
/// </summary>
public class PopupManager : FSystem {

	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, absence de niveaux etc...)
	private TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adressés à l'utilisateur

	private Family f_newMessageForUser = FamilyManager.getFamily(new AllOfComponents(typeof(MessageForUser)));
	private Family f_canvasGroup = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas), typeof(CanvasGroup)));


	// L'instance
	public static PopupManager instance;

	public PopupManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		f_newMessageForUser.addEntryCallback(displayMessageUser);
		messageForUser = panelInfoUser.transform.Find("Panel/Message").GetComponent<TMP_Text>();
	}

	// Affiche le panel message avec le bon message
	private void displayMessageUser(GameObject go)
	{
		foreach (GameObject canvas in f_canvasGroup)
			canvas.GetComponent<CanvasGroup>().interactable = false;

		MessageForUser mfu = go.GetComponent<MessageForUser>();
		messageForUser.text = mfu.message;
		GameObject buttons = panelInfoUser.transform.Find("Panel").Find("Buttons").gameObject;

		GameObjectManager.setGameObjectState(buttons.transform.GetChild(0).gameObject, mfu.OkButton != "");
		buttons.transform.GetChild(0).GetComponentInChildren<TMP_Text>(true).text = mfu.OkButton;
		if (mfu.call != null)
		{
			buttons.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
			buttons.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(mfu.call);
		}

		GameObjectManager.setGameObjectState(buttons.transform.GetChild(1).gameObject, mfu.CancelButton != "");
		buttons.transform.GetChild(1).GetComponentInChildren<TMP_Text>(true).text = mfu.CancelButton;

		GameObjectManager.setGameObjectState(panelInfoUser, true);

		// in case of several messages pop in one frame
		foreach (MessageForUser message in go.GetComponents<MessageForUser>())
			GameObjectManager.removeComponent(message);
	}

	public void turnOnCanvas()
	{
		foreach (GameObject canvas in f_canvasGroup)
			canvas.GetComponent<CanvasGroup>().interactable = true;
	}
}