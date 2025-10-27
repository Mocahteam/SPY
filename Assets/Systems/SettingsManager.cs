using UnityEngine;
using FYFY;
using TMPro;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// This system manage the settings window
/// </summary>
public class SettingsManager : FSystem
{
	private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));
	private Family f_settingsOpened = FamilyManager.getFamily(new AllOfComponents(typeof(SettingsManagerBridge)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_texts = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)));
	private Family f_highlightable = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)));
	private Family f_buttons = FamilyManager.getFamily(new AnyOfComponents(typeof(Button)), new AnyOfTags("DefaultButton"));
	private Family f_buttonsIcon = FamilyManager.getFamily(new AnyOfComponents(typeof(Button)), new NoneOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD)); // des boutons comme pour ouvrir les paramètres ou augmenter/réduire la taille de l'UI

	public static SettingsManager instance;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern bool ClearPlayerPrefs(); // call javascript

	public Transform settingsContent;
	public FlexibleColorPicker flexibleColorPicker;
	public CanvasScaler [] canvasScaler;

	public int defaultQuality = 2;
	public int defaultInteractionMode = 0;
	public float defaultUIScale = 1;
	public int defaultWallTransparency = 1;
	public int defaultGameView = 0;
	public Color defaultNormalColor_Text = new Color(0f, 0f, 0f, 1f); // black
	public Color defaultSelectedColor_Text = new Color(131f / 255f, 71f / 255f, 2f / 255f, 1f); // brown dark
	public Color defaultNormalColor_Button = new Color(1f, 1f, 1f, 0f); // transparent
	public Color defaultNormalColor_ButtonIcon = new Color(0f, 0f, 0f, 1f); // black
	public Color defaultHighlightedColor = new Color(1f, 178f/255f, 56f/255f, 1f); // orange light

	private int currentQuality;
	private int currentInteractionMode;
	private float currentUIScale;
	private int currentWallTransparency;
	private int currentGameView;
	private Color currentNormalColor_Text;
	private Color currentSelectedColor_Text;
	private Color currentNormalColor_Button;
	private Color currentNormalColor_ButtonIcon;
	private Color currentHighlightedColor;

	private TMP_Text currentSizeText;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && ClearPlayerPrefs())
			PlayerPrefs.DeleteAll();

		f_texts.addEntryCallback(delegate (GameObject go) { syncColor_Text(go, new Color()); });
		f_buttons.addEntryCallback(delegate (GameObject go) { syncNormalColor_Button(go, currentNormalColor_Button); });
		f_buttonsIcon.addEntryCallback(delegate (GameObject go) { syncNormalColor_Button(go, currentNormalColor_ButtonIcon); });
		f_highlightable.addEntryCallback(delegate (GameObject go) { syncHighlightedColor(go, currentHighlightedColor); });

		f_settingsOpened.addEntryCallback(delegate (GameObject unused) { loadPlayerPrefs(); });

		MainLoop.instance.StartCoroutine(waitLocalizationLoaded());
	}
	private IEnumerator waitLocalizationLoaded()
	{
		while (f_localizationLoaded.Count == 0)
			yield return null;
		loadPlayerPrefs();
		saveParameters();

		OnChangeColor(currentNormalColor_Text, out currentNormalColor_Text, f_texts, syncColor_Text);
		OnChangeColor(currentSelectedColor_Text, out currentSelectedColor_Text, f_texts, syncColor_Text);
		SyncLocalization.instance.syncLocale();
		OnChangeColor(currentNormalColor_Button, out currentNormalColor_Button, f_buttons, syncNormalColor_Button);
		OnChangeColor(currentNormalColor_ButtonIcon, out currentNormalColor_ButtonIcon, f_buttonsIcon, syncNormalColor_Button);
		OnChangeColor(currentHighlightedColor, out currentHighlightedColor, f_highlightable, syncHighlightedColor);
	}

	// lit les PlayerPrefs et initialise les UI en conséquence
	private void loadPlayerPrefs()
	{
		currentQuality = PlayerPrefs.GetInt("quality", defaultQuality);
		settingsContent.Find("Quality").GetComponentInChildren<TMP_Dropdown>().value = currentQuality;

		currentInteractionMode = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : defaultInteractionMode);
		settingsContent.Find("InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = currentInteractionMode;

		// définition de la taille de l'interface
		currentSizeText = settingsContent.Find("UISize").Find("CurrentSize").GetComponent<TMP_Text>();
		currentUIScale = PlayerPrefs.GetFloat("UIScale", (float)Math.Max(defaultUIScale, Math.Round((double)Screen.currentResolution.width / 2048, 2))); // do not reduce scale under defaultUIScale and multiply scale for definition higher than 2048
		currentSizeText.text = currentUIScale + "";
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = currentUIScale;

		currentWallTransparency = PlayerPrefs.GetInt("wallTransparency", defaultWallTransparency);
		settingsContent.Find("WallTransparency").GetComponentInChildren<TMP_Dropdown>().value = currentWallTransparency;

		currentGameView = PlayerPrefs.GetInt("orthographicView", defaultGameView);
		settingsContent.Find("GameView").GetComponentInChildren<TMP_Dropdown>().value = currentGameView;

		// Synchronisation de la couleur des textes
		syncPlayerPrefColor("TextColorNormal", defaultNormalColor_Text, out currentNormalColor_Text, "ColorTextNormal");
		syncPlayerPrefColor("TextColorSelected", defaultSelectedColor_Text, out currentSelectedColor_Text, "ColorTextSelected");
		// Synchronisation de la couleur des bouttons
		syncPlayerPrefColor("ButtonColorNormal", defaultNormalColor_Button, out currentNormalColor_Button, "ColorButtonNormal");
		// Synchronisation de la couleur des bouttons icônes
		syncPlayerPrefColor("ButtonIconColorNormal", defaultNormalColor_ButtonIcon, out currentNormalColor_ButtonIcon, "ColorButtonIconNormal");
		// Synchronisation de la couleur des highlighted
		syncPlayerPrefColor("HighlightedColor", defaultHighlightedColor, out currentHighlightedColor, "ColorHighlighted");
	}

	private void syncPlayerPrefColor(string playerPrefKey, Color defaultColor, out Color currentColor, string goName)
	{
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString(playerPrefKey, ColorUtility.ToHtmlStringRGBA(defaultColor)), out currentColor);
		settingsContent.Find(goName).Find("ButtonWithBorder").GetComponent<Image>().color = currentColor;
	}

	public void saveParameters()
    {
		// TODO : voir comment gérer la sauvegarde des PlayerPref dissiminés dans le code, cas du orthographicView
		// TODO : background inputField + Placeholder, Borders, Scrollbar (couleur normal == selected)
		PlayerPrefs.SetInt("quality", currentQuality);
		PlayerPrefs.SetInt("interaction", currentInteractionMode);
		PlayerPrefs.SetFloat("UIScale", currentUIScale);
		PlayerPrefs.SetInt("wallTransparency", currentWallTransparency);
		PlayerPrefs.SetInt("orthographicView", currentGameView);
		PlayerPrefs.SetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Text));
		PlayerPrefs.SetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(currentSelectedColor_Text));
		PlayerPrefs.SetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Button));
		PlayerPrefs.SetString("ButtonIconColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_ButtonIcon));
		PlayerPrefs.SetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(currentHighlightedColor));
		PlayerPrefs.Save();
		// TODO : Penser à sauvegarder dans la BD le choix de la langue

	}

	public void resetParameters()
    {
		PlayerPrefs.SetInt("quality", defaultQuality);
		PlayerPrefs.SetInt("interaction", defaultInteractionMode);
		PlayerPrefs.SetFloat("UIScale", defaultUIScale);
		PlayerPrefs.SetInt("wallTransparency", defaultWallTransparency);
		PlayerPrefs.SetInt("orthographicView", currentGameView);
		PlayerPrefs.SetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Text));
		PlayerPrefs.SetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(defaultSelectedColor_Text));
		PlayerPrefs.SetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Button));
		PlayerPrefs.SetString("ButtonIconColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_ButtonIcon));
		PlayerPrefs.SetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(defaultHighlightedColor));
		// Synchronisation des PlayerPrefs avec les UI
		loadPlayerPrefs();
		PlayerPrefs.Save();

		OnChangeColor(currentNormalColor_Text, out currentNormalColor_Text, f_texts, syncColor_Text);
		OnChangeColor(currentSelectedColor_Text, out currentSelectedColor_Text, f_texts, syncColor_Text);
		SyncLocalization.instance.syncLocale();
		OnChangeColor(currentNormalColor_Button, out currentNormalColor_Button, f_buttons, syncNormalColor_Button);
		OnChangeColor(currentNormalColor_ButtonIcon, out currentNormalColor_ButtonIcon, f_buttonsIcon, syncNormalColor_Button);
		OnChangeColor(currentHighlightedColor, out currentHighlightedColor, f_highlightable, syncHighlightedColor);
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
		currentQuality = value;
	}

	public void setInteraction(int value)
	{
		currentInteractionMode = value;
	}

	public void increaseUISize()
    {
		currentUIScale += 0.25f;
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = currentUIScale;
		currentSizeText.text = currentUIScale + "";
	}
	public void decreaseUISize()
	{
		if (currentUIScale >= 0.5f)
			currentUIScale -= 0.25f;
		foreach (CanvasScaler canvas in canvasScaler)
				canvas.scaleFactor = currentUIScale;
		currentSizeText.text = currentUIScale + "";
	}

	public void setWallTransparency(int value)
    {
		if (ObstableTransparencySystem.instance != null)
			ObstableTransparencySystem.instance.Pause = value == 0;
		currentWallTransparency = value;
	}

	public void setGameView(int value)
	{
		if (CameraSystem.instance != null)
			CameraSystem.instance.setOrthographicView(value == 1);
		currentGameView = value;
	}

	public void hookListener(string key)
    {
		flexibleColorPicker.onColorChange.RemoveAllListeners();
        switch (key)
        {
			case "TextColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					OnChangeColor(c, out currentNormalColor_Text, f_texts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "TextColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					OnChangeColor(c, out currentSelectedColor_Text, f_texts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "ButtonColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					OnChangeColor(c, out currentNormalColor_Button, f_buttons, syncNormalColor_Button);
				});
				break;
			case "ButtonIconColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					OnChangeColor(c, out currentNormalColor_ButtonIcon, f_buttonsIcon, syncNormalColor_Button);
				});
				break;
			case "HighlightedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					OnChangeColor(c, out currentHighlightedColor, f_highlightable, syncHighlightedColor);
				});
				break;
		}
		
	}

	private void OnChangeColor(Color newColor, out Color current, Family family, Action<GameObject, Color> call)
	{
		current = newColor;
		foreach (GameObject text in family) {
			call(text, current);
		}
	}

	private void syncColor_Text(GameObject text, Color unused)
    {
		Selectable textSel = text.GetComponent<Selectable>();
		// Pour tous les textes Selectable dont la cible est le texte lui même => Mettre à jour les couleurs du selectable
		if (textSel != null && textSel.targetGraphic == text.GetComponent<Graphic>())
		{
			ColorBlock currentColor = textSel.colors;
			currentColor.normalColor = currentNormalColor_Text;
			currentColor.highlightedColor = currentNormalColor_Text;
			currentColor.pressedColor = currentNormalColor_Text;
			currentColor.selectedColor = currentSelectedColor_Text;
			textSel.colors = currentColor;
		}
		// Pour les textes qui sont enfant d'un LangOption => Mettre à jour la couleur du bouton et le LangOption
		else if (text.GetComponentInParent<LangOption>(true))
        {
			Button langBtn = text.GetComponentInParent<Button>(true);
			ColorBlock currentColor = langBtn.colors;
			currentColor.normalColor = currentNormalColor_Text;
			langBtn.colors = currentColor;
			LangOption langOpt = text.GetComponentInParent<LangOption>(true);
			langOpt.on = currentSelectedColor_Text;
			langOpt.off = currentNormalColor_Text;
        }
		// Pour les textes qui sont enfant d'un Toggle (liste déroulantes) => Mettre à jour la couleur du Toggle
		else if (text.GetComponentInParent<Toggle>(true))
		{
			Toggle toggle = text.GetComponentInParent<Toggle>(true);
			ColorBlock currentColor = toggle.colors;
			currentColor.normalColor = currentNormalColor_Text;
			toggle.colors = currentColor;
		}
		// Sinon tous les autres textes, on change simplement leur couleur
		else
			text.GetComponent<TMP_Text>().color = currentNormalColor_Text;
	}

	public void syncNormalColor_Button(GameObject go, Color color)
	{
		Button button = go.GetComponent<Button>();
		ColorBlock currentColor = button.colors;
		currentColor.normalColor = color;
		button.colors = currentColor;
	}

	public void syncHighlightedColor(GameObject go, Color color)
	{
		Selectable select = go.GetComponent<Selectable>();
		ColorBlock currentColor = select.colors;
		currentColor.highlightedColor = color;
		select.colors = currentColor;
	}
}
