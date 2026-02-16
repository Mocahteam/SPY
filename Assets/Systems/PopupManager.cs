using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manage popup windows to display messages to the user
/// </summary>
public class PopupManager : FSystem {

	public GameObject panelInfoUser; // Panneau pour informer le joueur (erreurs de chargement, absence de niveaux etc...)
	private RectTransform panelPopup; // Le pannel contenant la popup à proprement dit
	private TMP_Text messageForUser; // Zone de texte pour les messages d'erreur adressés à l'utilisateur
	private RectTransform buttonsTransform; // les boutons

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
		panelPopup = panelInfoUser.transform.Find("Panel") as RectTransform;
		messageForUser = panelPopup.Find("Scroll View/Viewport/Content/Message").GetComponent<TMP_Text>();
		buttonsTransform = panelPopup.Find("Buttons") as RectTransform;
	}

    protected override void onProcess(int familiesUpdateCount)
    {
		if (panelInfoUser.activeInHierarchy)
			updatePopupSize();
	}

    // Affiche le panel message avec le bon message
    private void displayMessageUser(GameObject go)
	{
		foreach (GameObject canvas in f_canvasGroup)
			canvas.GetComponent<CanvasGroup>().interactable = false;

		MessageForUser mfu = go.GetComponent<MessageForUser>();
		messageForUser.text = mfu.message;

		GameObjectManager.setGameObjectState(buttonsTransform.GetChild(0).gameObject, mfu.OkButton != "");
		buttonsTransform.GetChild(0).GetComponentInChildren<TMP_Text>(true).text = mfu.OkButton;
		if (mfu.call != null)
		{
			buttonsTransform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
			buttonsTransform.GetChild(0).GetComponent<Button>().onClick.AddListener(mfu.call);
		}

		GameObjectManager.setGameObjectState(buttonsTransform.GetChild(1).gameObject, mfu.CancelButton != "");
		buttonsTransform.GetChild(1).GetComponentInChildren<TMP_Text>(true).text = mfu.CancelButton;

		GameObjectManager.setGameObjectState(panelInfoUser, true);

		// in case of several messages pop in one frame
		foreach (MessageForUser message in go.GetComponents<MessageForUser>())
			GameObjectManager.removeComponent(message);

		MainLoop.instance.StartCoroutine(forceScrollBarUp());
	}

	private void updatePopupSize()
	{
		Rect currentWindowRect = (panelInfoUser.transform as RectTransform).rect;
		Rect rect = panelPopup.rect;
		Rect currentTMPRect = (messageForUser.transform as RectTransform).rect;
		Rect currentButtonsRect = buttonsTransform.rect;

		float oldWidth = rect.width;
		float oldHeight = rect.height;

		rect.width = currentButtonsRect.width + 10;
		if (rect.width > currentWindowRect.width - 10)
			rect.width = currentWindowRect.width - 10;
		else
			rect.width += (currentWindowRect.width - rect.width) / 2;


		rect.height = currentTMPRect.height + currentButtonsRect.height + 40; // +40 pour les marges
		if (rect.height > currentWindowRect.height - 20)
			rect.height = currentWindowRect.height - 20;

		panelPopup.sizeDelta = new Vector2(rect.width, rect.height);

		// force scroll bar up
		if (rect.width != oldWidth || rect.height != oldHeight)
			MainLoop.instance.StartCoroutine(forceScrollBarUp());
	}

	private IEnumerator forceScrollBarUp()
	{
		yield return null;
		yield return null;
		panelPopup.GetComponentInChildren<Scrollbar>(true).value = 1f;
	}
}