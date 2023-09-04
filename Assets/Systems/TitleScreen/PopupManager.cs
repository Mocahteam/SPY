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
	public TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adressés à l'utilisateur

	private Family f_newMessageForUser = FamilyManager.getFamily(new AllOfComponents(typeof(MessageForUser)));
	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));


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
		buttons.transform.GetChild(0).GetComponentInChildren<TMP_Text>(true).text = mfu.OkButton;
		buttons.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
		buttons.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(mfu.call);

		GameObjectManager.setGameObjectState(buttons.transform.GetChild(1).gameObject, mfu.CancelButton != "");
		buttons.transform.GetChild(1).GetComponentInChildren<TMP_Text>(true).text = mfu.CancelButton;
		
		panelInfoUser.SetActive(true); // not use GameObjectManager here else ForceRebuildLayout doesn't work
		LayoutRebuilder.ForceRebuildLayoutImmediate(messageForUser.transform as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(messageForUser.transform.parent as RectTransform);

		if (mfu.OkButton != "")
			EventSystem.current.SetSelectedGameObject(buttons.transform.GetChild(0).gameObject);
		else if (mfu.CancelButton != "")
			EventSystem.current.SetSelectedGameObject(buttons.transform.GetChild(1).gameObject);

		// in case of several messages pop in one frame
		foreach (MessageForUser message in go.GetComponents<MessageForUser>())
			GameObjectManager.removeComponent(message);
	}

	// See ok and cancel buttons in PopupPanel (TitleScreen)
	public void focusLastButton()
    {
		MainLoop.instance.StartCoroutine(delayFocusLastButton());
    }

	private IEnumerator delayFocusLastButton()
    {
		yield return new WaitForSeconds(.25f);
		if (f_buttons.Count > 0 && (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy))
			EventSystem.current.SetSelectedGameObject(f_buttons.getAt(f_buttons.Count - 1));
	}
}