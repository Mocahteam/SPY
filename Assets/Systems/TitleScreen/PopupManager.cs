using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manage popup windows to display messages to the user
/// </summary>
public class PopupManager : FSystem {

	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, absence de niveaux etc...)
	public TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adressés à l'utilisateur

	private Family f_newMessageForUser = FamilyManager.getFamily(new AllOfComponents(typeof(MessageForUser)));


	// L'instance
	public static PopupManager instance;

	public PopupManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		f_newMessageForUser.addEntryCallback(displayMessageUser);
	}

	// Affiche le panel message avec le bon message
	private void displayMessageUser(GameObject go)
	{
		MessageForUser mfu = go.GetComponent<MessageForUser>();
		messageForUser.text = mfu.message;
		GameObject buttons = panelInfoUser.transform.Find("Panel").Find("Buttons").gameObject;
		GameObjectManager.setGameObjectState(buttons.transform.GetChild(0).gameObject, mfu.OkButton != "");
		buttons.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = mfu.OkButton;
		buttons.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
		buttons.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(mfu.call);
		GameObjectManager.setGameObjectState(buttons.transform.GetChild(1).gameObject, mfu.CancelButton != "");
		buttons.transform.GetChild(1).GetComponentInChildren<TMP_Text>().text = mfu.CancelButton;
		panelInfoUser.SetActive(true); // not use GameObjectManager here else ForceRebuildLayout doesn't work
		LayoutRebuilder.ForceRebuildLayoutImmediate(messageForUser.transform as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(messageForUser.transform.parent as RectTransform);

		GameObjectManager.removeComponent(mfu);
	}
}