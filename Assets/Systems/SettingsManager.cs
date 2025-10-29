using UnityEngine;
using FYFY;
using TMPro;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// This system manage the settings window
/// </summary>
public class SettingsManager : FSystem
{
	private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));
	private Family f_settingsOpened = FamilyManager.getFamily(new AllOfComponents(typeof(SettingsManagerBridge)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_texts = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)));
	private Family f_textsSelectable = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI), typeof(Selectable)));
	private Family f_buttons = FamilyManager.getFamily(new AnyOfComponents(typeof(Button)), new AnyOfTags("DefaultButton"));
	private Family f_buttonsIcon = FamilyManager.getFamily(new AnyOfComponents(typeof(Button)), new NoneOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD)); // des boutons comme pour ouvrir les paramètres ou augmenter/réduire la taille de l'UI
	private Family f_highlightable = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)));
	private Family f_panels = FamilyManager.getFamily(new AnyOfComponents(typeof(Image)), new AnyOfTags("UI_Panel"));
	private Family f_borders = FamilyManager.getFamily(new AnyOfComponents(typeof(Image)), new AnyOfTags("UI_Border"));

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
	public Color defaultNormalColor_Text = Color.black; // black
	public Color defaultSelectedColor_Text = new Color(131f / 255f, 71f / 255f, 2f / 255f, 1f); // brown dark
	public Color defaultNormalColor_Button = new Color(1f, 1f, 1f, 0f); // transparent
	public Color defaultNormalColor_ButtonIcon = Color.black; // black
	public Color defaultHighlightedColor = new Color(1f, 178f/255f, 56f/255f, 1f); // orange light
	public Color defaultPressedColor = new Color(194f / 255f, 94f / 255f, 0f, 1f); // brown
	public Color defaultSelectedColor = new Color(223f / 255f, 127f / 255f, 2f / 255f, 1f); // orange
	public Color defaultDisabledColor = new Color(187f / 255f, 187f / 255f, 187f / 255f, 128f / 255f); // grey transparent
	public Color defaultColor_Panel = Color.white;
	public Color defaultColor_PanelTexture = new Color(0f, 0f, 0f, 7f / 255f); // black transparent
	public Color defaultColor_Border = new Color(77f / 255f, 77f / 255f, 77f / 255f, 1f); // grey dark

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
	private Color currentPressedColor;
	private Color currentSelectedColor;
	private Color currentDisabledColor;
	private Color currentColor_Panel;
	private Color currentColor_PanelTexture;
	private Color currentColor_Border;

	private TMP_Text currentSizeText;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && ClearPlayerPrefs())
			PlayerPrefs.DeleteAll();

		f_texts.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_textsSelectable.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_buttons.addEntryCallback(delegate (GameObject go) { syncNormalColor_Button(go, currentNormalColor_Button); });
		f_buttonsIcon.addEntryCallback(delegate (GameObject go) { syncNormalColor_Button(go, currentNormalColor_ButtonIcon); });
		f_highlightable.addEntryCallback(delegate (GameObject go) { syncHighlightedColor(go); });
		f_panels.addEntryCallback(delegate (GameObject go) { syncColor_Panel(go); });
		f_borders.addEntryCallback(delegate (GameObject go) { syncColor_Image(go, currentColor_Border); });

		f_settingsOpened.addEntryCallback(delegate (GameObject unused) { loadPlayerPrefs(); });

		MainLoop.instance.StartCoroutine(waitLocalizationLoaded());
	}
	private IEnumerator waitLocalizationLoaded()
	{
		while (f_localizationLoaded.Count == 0)
			yield return null;
		loadPlayerPrefs();
		saveParameters();

		syncColor(f_texts, syncColor_Text);
		SyncLocalization.instance.syncLocale();
		syncColor(f_buttons, syncNormalColor_Button, currentNormalColor_Button);
		syncColor(f_buttonsIcon, syncNormalColor_Button, currentNormalColor_ButtonIcon);
		syncColor(f_highlightable, syncHighlightedColor);
		syncColor(f_panels, syncColor_Panel);
		syncColor(f_borders, syncColor_Image, currentColor_Border);
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
		// Synchronisation de la couleur des pressed
		syncPlayerPrefColor("PressedColor", defaultPressedColor, out currentPressedColor, "ColorPressed");
		// Synchronisation de la couleur des pressed
		syncPlayerPrefColor("SelectedColor", defaultSelectedColor, out currentSelectedColor, "ColorSelected");
		// Synchronisation de la couleur des disabled
		syncPlayerPrefColor("DisabledColor", defaultDisabledColor, out currentDisabledColor, "ColorDisabled");
		// Synchronisation de la couleur des panels
		syncPlayerPrefColor("PanelColor", defaultColor_Panel, out currentColor_Panel, "ColorPanel");
		syncPlayerPrefColor("PanelColorTexture", defaultColor_PanelTexture, out currentColor_PanelTexture, "ColorPanelTexture");
		// Synchronisation de la couleur des bordures
		syncPlayerPrefColor("BorderColor", defaultColor_Border, out currentColor_Border, "ColorBorder");
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
		PlayerPrefs.SetString("PressedColor", ColorUtility.ToHtmlStringRGBA(currentPressedColor));
		PlayerPrefs.SetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(currentSelectedColor));
		PlayerPrefs.SetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(currentDisabledColor));
		PlayerPrefs.SetString("PanelColor", ColorUtility.ToHtmlStringRGBA(currentColor_Panel));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(currentColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(currentColor_Border));
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
		PlayerPrefs.SetString("PressedColor", ColorUtility.ToHtmlStringRGBA(defaultPressedColor));
		PlayerPrefs.SetString("SekectedColor", ColorUtility.ToHtmlStringRGBA(defaultSelectedColor));
		PlayerPrefs.SetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(defaultDisabledColor));
		PlayerPrefs.SetString("PanelColor", ColorUtility.ToHtmlStringRGBA(defaultColor_Panel));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(defaultColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(defaultColor_Border));
		// Synchronisation des PlayerPrefs avec les UI
		loadPlayerPrefs();
		PlayerPrefs.Save();

		syncColor(f_texts, syncColor_Text);
		SyncLocalization.instance.syncLocale();
		syncColor(f_buttons, syncNormalColor_Button, currentNormalColor_Button);
		syncColor(f_buttonsIcon, syncNormalColor_Button, currentNormalColor_ButtonIcon);
		syncColor(f_highlightable, syncHighlightedColor);
		syncColor(f_panels, syncColor_Panel);
		syncColor(f_borders, syncColor_Image, currentColor_Border);
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
					currentNormalColor_Text = c;
					syncColor(f_texts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "TextColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentSelectedColor_Text = c;
					syncColor(f_texts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "ButtonColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_Button = c;
					syncColor(f_buttons, syncNormalColor_Button, currentNormalColor_Button);
				});
				break;
			case "ButtonIconColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_ButtonIcon = c;
					syncColor(f_buttonsIcon, syncNormalColor_Button, currentNormalColor_ButtonIcon);
				});
				break;
			case "HighlightedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentHighlightedColor = c;
					syncColor(f_highlightable, syncHighlightedColor);
				});
				break;
			case "PressedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentPressedColor = c;
					syncColor(f_highlightable, syncHighlightedColor);
				});
				break;
			case "SelectedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentSelectedColor = c;
					syncColor(f_highlightable, syncHighlightedColor);
				});
				break;
			case "DisabledColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentDisabledColor = c;
					syncColor(f_texts, syncColor_Text);
					syncColor(f_highlightable, syncHighlightedColor);
				});
				break;
			case "PanelColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Panel = c;
					syncColor(f_panels, syncColor_Panel);
				});
				break;
			case "PanelColorTexture":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_PanelTexture = c;
					syncColor(f_panels, syncColor_Panel);
				});
				break;
			case "BorderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Border = c;
					syncColor(f_borders, syncColor_Image, currentColor_Border);
				});
				break;
		}
		
	}

	private void syncColor (Family family, Action<GameObject, Color?> call, Color? color = null)
    {
		foreach (GameObject text in family)
		{
			call(text, color);
		}
	}

	private void syncColor_Text(GameObject text, Color? unused = null)
    {
		Selectable textSel = text.GetComponent<Selectable>();
		// Pour tous les textes Selectable dont la cible est le texte lui même => Mettre à jour les couleurs du selectable
		if (textSel != null && textSel.targetGraphic == text.GetComponent<Graphic>())
		{
			// forcer la couleur du texte à blanc pour être sûr que la couleur du selectable soit bien celle prise en compte
			text.GetComponent<TMP_Text>().color = Color.white;
			// définir les couleurs du selectable
			ColorBlock currentColor = textSel.colors;
			currentColor.normalColor = currentNormalColor_Text;
			currentColor.highlightedColor = currentNormalColor_Text;
			currentColor.pressedColor = currentNormalColor_Text;
			currentColor.selectedColor = currentSelectedColor_Text;
			currentColor.disabledColor = currentDisabledColor;
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

	public void syncNormalColor_Button(GameObject go, Color? color)
	{
		Button button = go.GetComponent<Button>();
		ColorBlock currentColor = button.colors;
		currentColor.normalColor = color ?? defaultNormalColor_Button;
		button.colors = currentColor;
	}

	public void syncHighlightedColor(GameObject go, Color? unused = null)
	{
		Selectable select = go.GetComponent<Selectable>();
		ColorBlock currentColor = select.colors;
		currentColor.highlightedColor = currentHighlightedColor;
		currentColor.pressedColor = currentPressedColor;
		currentColor.selectedColor = currentSelectedColor;
		currentColor.disabledColor = currentDisabledColor;
		select.colors = currentColor;
	}

	public void syncColor_Panel(GameObject go, Color? unused = null)
	{
		go.GetComponent<Image>().color = currentColor_Panel;
		Transform texture = go.transform.Find("Texture");
		if (texture != null)
			texture.GetComponent<Image>().color = currentColor_PanelTexture;
	}

	public void syncColor_Image(GameObject go, Color? color)
	{
		go.GetComponent<Image>().color = color ?? Color.black;
	}
}
