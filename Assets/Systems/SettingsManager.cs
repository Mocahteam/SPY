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
	private Family f_allTexts = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)));
	private Family f_textsSelectable = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI), typeof(Selectable)));
	private Family f_modifiableFonts = FamilyManager.getFamily(new AnyOfComponents(typeof(TextMeshProUGUI), typeof(TMP_InputField)), new NoneOfTags("UI_OverrideFont"));
	private Family f_fixedFonts = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new AnyOfTags("UI_OverrideFont"));
	private Family f_dropdown = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_Dropdown)));
	private Family f_inputfield = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));
	private Family f_inputfieldCaret = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_SelectionCaret)));
	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("DefaultButton"));
	private Family f_buttonsIcon = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new NoneOfProperties(PropertyMatcher.PROPERTY.HAS_CHILD)); // des boutons comme pour ouvrir les paramètres ou augmenter/réduire la taille de l'UI
	private Family f_highlightable = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)));
	private Family f_SyncSelectedColor = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_SyncSelectedColor"));
	private Family f_panels = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Panel"));
	private Family f_borders = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Border"));
	private Family f_scrollbar = FamilyManager.getFamily(new AllOfComponents(typeof(Scrollbar), typeof(Image)));
	private Family f_scrollview = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollRect), typeof(Image)), new NoneOfComponents(typeof(AutoBind))); // Le Autobind permet d'exclure les scrollRect contenus dans des dropdown
	private Family f_toggle = FamilyManager.getFamily(new AllOfComponents(typeof(Toggle)), new NoneOfTags("UI_Avatar"));
	private Family f_tooltip = FamilyManager.getFamily(new AllOfComponents(typeof(Tooltip), typeof(Image)));


	public static SettingsManager instance;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern bool ClearPlayerPrefs(); // call javascript

	public Transform settingsContent;
	public FlexibleColorPicker flexibleColorPicker;
	public CanvasScaler [] canvasScaler;
	public TMP_FontAsset[] fonts;
	public Selectable LoadingLogs;

	public int defaultQuality = 2;
	public int defaultInteractionMode = 0;
	public float defaultUIScale = 1;
	public int defaultWallTransparency = 1;
	public int defaultGameView = 0;
	public int defaultFont = 6;
	public int defaultCaretWidth = 0;
	public int defaultCaretHeight = 0;
	public Color defaultNormalColor_Text = Color.black; // black
	public Color defaultSelectedColor_Text = new Color(131f / 255f, 71f / 255f, 2f / 255f, 1f); // brown dark
	public Color defaultPlaceholderColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 128f / 255f); // grey dark transparent
	public Color defaultNormalColor_Dropdown = Color.white;
	public Color defaultNormalColor_Inputfield = Color.white;
	public Color defaultSelectedColor_Inputfield = new Color(200f / 255f, 200f / 255f, 200f / 255f, 1f); // grey light
	public Color defaultSelectionColor_Inputfield = new Color(168f / 255f, 206f / 255f, 1f, 192f / 255f); // blue light transparent
	public Color defaultCaretColor_Inputfield = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f); // frey dark
	public Color defaultNormalColor_Button = new Color(1f, 1f, 1f, 0f); // transparent
	public Color defaultHighlightedColor = new Color(1f, 178f/255f, 56f/255f, 1f); // orange light
	public Color defaultPressedColor = new Color(194f / 255f, 94f / 255f, 0f, 1f); // brown
	public Color defaultSelectedColor = new Color(223f / 255f, 127f / 255f, 2f / 255f, 1f); // orange
	public Color defaultDisabledColor = new Color(187f / 255f, 187f / 255f, 187f / 255f, 128f / 255f); // grey transparent
	public Color defaultColor_Icon = Color.black; // black
	public Color defaultColor_Panel = Color.white;
	public Color defaultColor_PanelTexture = new Color(0f, 0f, 0f, 7f / 255f); // black transparent
	public Color defaultColor_Border = new Color(77f / 255f, 77f / 255f, 77f / 255f, 1f); // grey dark
	public int defaultBorderThickness = 1;
	public Color defaultNormalColor_Scrollbar = new Color(223f / 255f, 127f / 255f, 2f / 255f, 1f); // orange
	public Color defaultBackgroundColor_Scrollbar = new Color(1f, 178f / 255f, 56f / 255f, 1f); // orange light
	public Color defaultBackgroundColor_Scrollview = new Color(1f, 1f, 1f, 0f); // transparent
	public Color defaultNormalColor_Toggle = Color.white;
	public Color defaultColor_Tooltip = Color.white;

	private int currentQuality;
	private int currentInteractionMode;
	private float currentUIScale;
	private int currentWallTransparency;
	private int currentGameView;
	private int currentFont;
	private int currentCaretWidth;
	private int currentCaretHeight;
	private Color currentNormalColor_Text;
	private Color currentSelectedColor_Text;
	private Color currentPlaceholderColor;
	private Color currentNormalColor_Dropdown;
	private Color currentNormalColor_Inputfield;
	private Color currentSelectedColor_Inputfield;
	private Color currentSelectionColor_Inputfield;
	private Color currentCaretColor_Inputfield;
	private Color currentNormalColor_Button;
	private Color currentHighlightedColor;
	private Color currentPressedColor;
	private Color currentSelectedColor;
	private Color currentDisabledColor;
	private Color currentColor_Icon;
	private Color currentColor_Panel;
	private Color currentColor_PanelTexture;
	private Color currentColor_Border;
	private int currentBorderThickness;
	private Color currentNormalColor_Scrollbar;
	private Color currentBackgroundColor_Scrollbar;
	private Color currentBackgroundColor_Scrollview;
	private Color currentNormalColor_Toggle;
	private Color currentColor_Tooltip;

	private TMP_Text currentSizeText;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && ClearPlayerPrefs())
			PlayerPrefs.DeleteAll();

		f_allTexts.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_textsSelectable.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_modifiableFonts.addEntryCallback(syncFont);
		f_fixedFonts.addEntryCallback(fixFont);
		f_dropdown.addEntryCallback(delegate (GameObject go) { syncColor_Dropdown(go); });
		f_inputfield.addEntryCallback(delegate (GameObject go) { sync_Inputfield(go); });
		f_inputfieldCaret.addEntryCallback(sync_CaretHeight);
		f_buttons.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, currentNormalColor_Button); });
		f_buttonsIcon.addEntryCallback(delegate (GameObject go) {syncNormalColor(go, currentColor_Icon); });
		f_highlightable.addEntryCallback(delegate (GameObject go) { syncHighlightedColor(go); });
		f_SyncSelectedColor.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, currentSelectedColor); });
		f_panels.addEntryCallback(delegate (GameObject go) { syncColor_Panel(go); });
		f_borders.addEntryCallback(delegate (GameObject go) { syncBorderProperties(go); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, currentNormalColor_Scrollbar); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, currentBackgroundColor_Scrollbar); });
		f_scrollview.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, currentBackgroundColor_Scrollview); });
		f_toggle.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, currentNormalColor_Toggle); });
		f_toggle.addEntryCallback(delegate (GameObject go) { syncColor_ToggleCheckmark(go); });
		f_tooltip.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, currentColor_Tooltip); });

		f_settingsOpened.addEntryCallback(delegate (GameObject unused) { loadPlayerPrefs(); });

		MainLoop.instance.StartCoroutine(waitLocalizationLoaded());
	}
	private IEnumerator waitLocalizationLoaded()
	{
		while (f_localizationLoaded.Count == 0)
			yield return null;
		loadPlayerPrefs();
		saveParameters();
		syncColors();
		foreach (GameObject caretGO in f_inputfieldCaret)
			sync_CaretHeight(caretGO);
	}

	private void syncColors()
    {
		syncColor(f_allTexts, syncColor_Text);
		SyncLocalization.instance.syncLocale();
		syncColor(f_dropdown, syncColor_Dropdown);
		syncColor(f_inputfield, sync_Inputfield);
		syncColor(f_buttons, syncNormalColor, currentNormalColor_Button);
		syncColor(f_buttonsIcon, syncNormalColor, currentColor_Icon);
		syncColor(f_highlightable, syncHighlightedColor);
		syncColor(f_SyncSelectedColor, syncGraphicColor, currentSelectedColor);
		syncColor(f_panels, syncColor_Panel);
		syncColor(f_borders, syncBorderProperties);
		syncColor(f_scrollbar, syncNormalColor, currentNormalColor_Scrollbar);
		syncColor(f_scrollbar, syncGraphicColor, currentBackgroundColor_Scrollbar);
		syncColor(f_scrollview, syncGraphicColor, currentBackgroundColor_Scrollview);
		syncColor(f_toggle, syncNormalColor, currentNormalColor_Toggle);
		syncColor(f_toggle, syncColor_ToggleCheckmark);
		syncColor(f_tooltip, syncGraphicColor, currentColor_Tooltip);
	}

	// lit les PlayerPrefs et initialise les UI en conséquence
	private void loadPlayerPrefs()
	{
		currentQuality = PlayerPrefs.GetInt("quality", defaultQuality);
		settingsContent.Find("SectionGraphic/Quality").GetComponentInChildren<TMP_Dropdown>().value = currentQuality;

		currentInteractionMode = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : defaultInteractionMode);
		settingsContent.Find("SectionGraphic/InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = currentInteractionMode;

		// définition de la taille de l'interface
		currentSizeText = settingsContent.Find("SectionGraphic/UISize").Find("CurrentSize").GetComponent<TMP_Text>();
		currentUIScale = PlayerPrefs.GetFloat("UIScale", (float)Math.Max(defaultUIScale, Math.Round((double)Screen.currentResolution.width / 2048, 2))); // do not reduce scale under defaultUIScale and multiply scale for definition higher than 2048
		currentSizeText.text = currentUIScale + "";
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = currentUIScale;

		currentWallTransparency = PlayerPrefs.GetInt("wallTransparency", defaultWallTransparency);
		settingsContent.Find("SectionGraphic/WallTransparency").GetComponentInChildren<TMP_Dropdown>().value = currentWallTransparency;

		currentGameView = PlayerPrefs.GetInt("orthographicView", defaultGameView);
		settingsContent.Find("SectionGraphic/GameView").GetComponentInChildren<TMP_Dropdown>().value = currentGameView;

		currentFont = PlayerPrefs.GetInt("font", defaultFont);
		settingsContent.Find("SectionText/FontDropdown").GetComponentInChildren<TMP_Dropdown>().value = currentFont;

		currentCaretWidth = PlayerPrefs.GetInt("caretWidth", defaultCaretWidth);
		settingsContent.Find("SectionText/CaretWidth").GetComponentInChildren<TMP_Dropdown>().value = currentCaretWidth;
		currentCaretHeight = PlayerPrefs.GetInt("caretHeight", defaultCaretHeight);
		settingsContent.Find("SectionText/CaretHeight").GetComponentInChildren<TMP_Dropdown>().value = currentCaretHeight;

		// Synchronisation de la couleur des textes
		syncPlayerPrefColor("TextColorNormal", defaultNormalColor_Text, out currentNormalColor_Text, "SectionText/ColorTextNormal");
		syncPlayerPrefColor("TextColorSelected", defaultSelectedColor_Text, out currentSelectedColor_Text, "SectionText/ColorTextSelected");
		syncPlayerPrefColor("PlaceholderColor", defaultPlaceholderColor, out currentPlaceholderColor, "SectionText/ColorPlaceholder");
		// Synchronisation de la couleur des dropdown
		syncPlayerPrefColor("DropdownColorNormal", defaultNormalColor_Dropdown, out currentNormalColor_Dropdown, "SectionColor/ColorDropdownNormal");
		// Synchronisation de la couleur des inputfield
		syncPlayerPrefColor("InputfieldColorNormal", defaultNormalColor_Inputfield, out currentNormalColor_Inputfield, "SectionColor/ColorInputfieldNormal");
		syncPlayerPrefColor("InputfieldColorSelected", defaultSelectedColor_Inputfield, out currentSelectedColor_Inputfield, "SectionColor/ColorInputfieldSelected");
		syncPlayerPrefColor("InputfieldColorSelection", defaultSelectionColor_Inputfield, out currentSelectionColor_Inputfield, "SectionColor/ColorInputfieldSelection");
		syncPlayerPrefColor("InputfieldColorCaret", defaultCaretColor_Inputfield, out currentCaretColor_Inputfield, "SectionColor/ColorInputfieldCaret");
		// Synchronisation de la couleur des bouttons
		syncPlayerPrefColor("ButtonColorNormal", defaultNormalColor_Button, out currentNormalColor_Button, "SectionColor/ColorButtonNormal");
		// Synchronisation de la couleur des highlighted
		syncPlayerPrefColor("HighlightedColor", defaultHighlightedColor, out currentHighlightedColor, "SectionColor/ColorHighlighted");
		// Synchronisation de la couleur des pressed
		syncPlayerPrefColor("PressedColor", defaultPressedColor, out currentPressedColor, "SectionColor/ColorPressed");
		// Synchronisation de la couleur des selected
		syncPlayerPrefColor("SelectedColor", defaultSelectedColor, out currentSelectedColor, "SectionColor/ColorSelected");
		// Synchronisation de la couleur des disabled
		syncPlayerPrefColor("DisabledColor", defaultDisabledColor, out currentDisabledColor, "SectionColor/ColorDisabled");
		// Synchronisation de la couleur des bouttons icônes
		syncPlayerPrefColor("IconColor", defaultColor_Icon, out currentColor_Icon, "SectionColor/ColorIcon");
		// Synchronisation de la couleur des panels
		syncPlayerPrefColor("PanelColor", defaultColor_Panel, out currentColor_Panel, "SectionColor/ColorPanel");
		syncPlayerPrefColor("PanelColorTexture", defaultColor_PanelTexture, out currentColor_PanelTexture, "SectionColor/ColorPanelTexture");
		// Synchronisation des propriétés de bordure
		syncPlayerPrefColor("BorderColor", defaultColor_Border, out currentColor_Border, "SectionColor/ColorBorder");
		currentBorderThickness = PlayerPrefs.GetInt("BorderThickness", defaultBorderThickness);
		settingsContent.Find("SectionColor/ThicknessBorder").GetComponentInChildren<TMP_Dropdown>().value = currentBorderThickness-1;
		// Synchronisation de la couleur des scrollbars
		syncPlayerPrefColor("ScrollbarColorNormal", defaultNormalColor_Scrollbar, out currentNormalColor_Scrollbar, "SectionColor/ColorScrollbarNormal");
		syncPlayerPrefColor("ScrollbarColorBackground", defaultBackgroundColor_Scrollbar, out currentBackgroundColor_Scrollbar, "SectionColor/ColorScrollbarBackground");
		syncPlayerPrefColor("ScrollviewColor", defaultBackgroundColor_Scrollview, out currentBackgroundColor_Scrollview, "SectionColor/ColorScrollview");
		// Synchronisation de la couleur des toggles
		syncPlayerPrefColor("ToggleColorNormal", defaultNormalColor_Toggle, out currentNormalColor_Toggle, "SectionColor/ColorToggleNormal");
		// Synchronisation de la couleur des tooltip
		syncPlayerPrefColor("TooltipColor", defaultColor_Tooltip, out currentColor_Tooltip, "SectionColor/ColorTooltip");
	}

	private void syncPlayerPrefColor(string playerPrefKey, Color defaultColor, out Color currentColor, string goName)
	{
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString(playerPrefKey, ColorUtility.ToHtmlStringRGBA(defaultColor)), out currentColor);
		settingsContent.Find(goName+"/ButtonWithBorder").GetComponent<Image>().color = currentColor;
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
		PlayerPrefs.SetInt("font", currentFont);
		PlayerPrefs.SetInt("caretWidth", currentCaretWidth);
		PlayerPrefs.SetInt("caretHeight", currentCaretHeight);
		PlayerPrefs.SetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Text));
		PlayerPrefs.SetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(currentSelectedColor_Text));
		PlayerPrefs.SetString("PlaceholderColor", ColorUtility.ToHtmlStringRGBA(currentPlaceholderColor));
		PlayerPrefs.SetString("DropdownColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Dropdown));
		PlayerPrefs.SetString("InputfieldColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelected", ColorUtility.ToHtmlStringRGBA(currentSelectedColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelection", ColorUtility.ToHtmlStringRGBA(currentSelectionColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorCaret", ColorUtility.ToHtmlStringRGBA(currentCaretColor_Inputfield));
		PlayerPrefs.SetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Button));
		PlayerPrefs.SetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(currentHighlightedColor));
		PlayerPrefs.SetString("PressedColor", ColorUtility.ToHtmlStringRGBA(currentPressedColor));
		PlayerPrefs.SetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(currentSelectedColor));
		PlayerPrefs.SetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(currentDisabledColor));
		PlayerPrefs.SetString("IconColor", ColorUtility.ToHtmlStringRGBA(currentColor_Icon));
		PlayerPrefs.SetString("PanelColor", ColorUtility.ToHtmlStringRGBA(currentColor_Panel));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(currentColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(currentColor_Border));
		PlayerPrefs.SetInt("BorderThickness", currentBorderThickness);
		PlayerPrefs.SetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Scrollbar));
		PlayerPrefs.SetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(currentBackgroundColor_Scrollbar));
		PlayerPrefs.SetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(currentBackgroundColor_Scrollview));
		PlayerPrefs.SetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Toggle));
		PlayerPrefs.SetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(currentColor_Tooltip));
		PlayerPrefs.Save();
		// TODO : Penser à sauvegarder dans la BD le choix de la langue

	}

	public void resetParameters()
    {
		PlayerPrefs.SetInt("quality", defaultQuality);
		PlayerPrefs.SetInt("interaction", defaultInteractionMode);
		PlayerPrefs.SetFloat("UIScale", defaultUIScale);
		PlayerPrefs.SetInt("wallTransparency", defaultWallTransparency);
		PlayerPrefs.SetInt("orthographicView", defaultGameView);
		PlayerPrefs.SetInt("font", defaultFont);
		PlayerPrefs.SetInt("caretWidth", defaultCaretWidth);
		PlayerPrefs.SetInt("caretHeight", defaultCaretHeight);
		PlayerPrefs.SetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Text));
		PlayerPrefs.SetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(defaultSelectedColor_Text));
		PlayerPrefs.SetString("PlaceholderColor", ColorUtility.ToHtmlStringRGBA(defaultPlaceholderColor));
		PlayerPrefs.SetString("DropdownColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Dropdown));
		PlayerPrefs.SetString("InputfieldColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelected", ColorUtility.ToHtmlStringRGBA(defaultSelectedColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelection", ColorUtility.ToHtmlStringRGBA(defaultSelectionColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorCaret", ColorUtility.ToHtmlStringRGBA(defaultCaretColor_Inputfield));
		PlayerPrefs.SetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Button));
		PlayerPrefs.SetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(defaultHighlightedColor));
		PlayerPrefs.SetString("PressedColor", ColorUtility.ToHtmlStringRGBA(defaultPressedColor));
		PlayerPrefs.SetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(defaultSelectedColor));
		PlayerPrefs.SetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(defaultDisabledColor));
		PlayerPrefs.SetString("IconColor", ColorUtility.ToHtmlStringRGBA(defaultColor_Icon));
		PlayerPrefs.SetString("PanelColor", ColorUtility.ToHtmlStringRGBA(defaultColor_Panel));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(defaultColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(defaultColor_Border));
		PlayerPrefs.SetInt("BorderThickness", defaultBorderThickness);
		PlayerPrefs.SetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Scrollbar));
		PlayerPrefs.SetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(defaultBackgroundColor_Scrollbar));
		PlayerPrefs.SetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(defaultBackgroundColor_Scrollview));
		PlayerPrefs.SetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(defaultNormalColor_Toggle));
		PlayerPrefs.SetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(defaultColor_Tooltip));
		// Synchronisation des PlayerPrefs avec les UI
		loadPlayerPrefs();
		PlayerPrefs.Save();

		syncColors();
	}

	public void hookListener(string key)
	{
		flexibleColorPicker.onColorChange.RemoveAllListeners();
		switch (key)
		{
			case "TextColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "TextColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentSelectedColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "PlaceholderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentPlaceholderColor = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "DropdownColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_Dropdown = c;
					syncColor(f_dropdown, syncColor_Dropdown);
				});
				break;
			case "InputfieldColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentSelectedColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelection":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentSelectionColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorCaret":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentCaretColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "ButtonColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_Button = c;
					syncColor(f_buttons, syncNormalColor, currentNormalColor_Button);
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
					syncColor(f_SyncSelectedColor, syncGraphicColor, currentSelectedColor);
				});
				break;
			case "DisabledColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentDisabledColor = c;
					syncColor(f_allTexts, syncColor_Text);
					syncColor(f_highlightable, syncHighlightedColor);
				});
				break;
			case "IconColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Icon = c;
					syncColor(f_buttonsIcon, syncNormalColor, currentColor_Icon);
					syncColor(f_dropdown, syncColor_Dropdown);
					syncColor(f_toggle, syncColor_ToggleCheckmark);
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
					syncColor(f_borders, syncBorderProperties, currentColor_Border);
				});
				break;
			case "ScrollbarColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_Scrollbar = c;
					syncColor(f_scrollbar, syncNormalColor, currentNormalColor_Scrollbar);
				});
				break;
			case "ScrollbarColorBackground":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentBackgroundColor_Scrollbar= c;
					syncColor(f_scrollbar, syncGraphicColor, currentBackgroundColor_Scrollbar);
				});
				break;
			case "ScrollviewColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentBackgroundColor_Scrollview = c;
					syncColor(f_scrollview, syncGraphicColor, currentBackgroundColor_Scrollview);
				});
				break;
			case "ToggleColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentNormalColor_Toggle = c;
					syncColor(f_toggle, syncNormalColor, currentNormalColor_Toggle);
				});
				break;
			case "TooltipColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Tooltip = c;
					syncColor(f_tooltip, syncGraphicColor, currentColor_Tooltip);
				});
				break;
		}
	}

	private void syncColor(Family family, Action<GameObject, Color?> call, Color? color = null)
	{
		foreach (GameObject text in family)
		{
			call(text, color);
		}
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

	public void syncFonts(int value)
    {
		currentFont = value;
		foreach (GameObject go in f_modifiableFonts)
			syncFont(go);
    }

	private void syncFont(GameObject go)
    {
		TMP_InputField inputField = go.GetComponent<TMP_InputField>();
		if (inputField != null)
			inputField.fontAsset = fonts[currentFont];
		else
			go.GetComponent<TextMeshProUGUI>().font = fonts[currentFont];
	}

	// Fonction utilisée pour définir la font dans la liste déroulante de sélection de la font dans les paramètres
	private void fixFont(GameObject go)
    {
		TextMeshProUGUI option = go.GetComponent<TextMeshProUGUI>();
		switch (option.text)
        {
			case "Arial": option.font = fonts[0];
				break;
			case "Comic Sans MS":
				option.font = fonts[1];
				break;
			case "Liberation Sans SDF":
				option.font = fonts[2];
				break;
			case "Luciole":
				option.font = fonts[3];
				break;
			case "Open Dyslexic":
				option.font = fonts[4];
				break;
			case "Orbitron":
				option.font = fonts[5];
				break;
			case "Roboto":
				option.font = fonts[6];
				break;
			case "Tahoma":
				option.font = fonts[7];
				break;
			case "Verdana":
				option.font = fonts[8];
				break;
		}
	}

	public void setCaretWidth(int value)
    {
		currentCaretWidth = value;
		foreach (GameObject inputGO in f_inputfield)
			sync_Inputfield(inputGO);
	}

	public void setCaretHeight(int value)
	{
		currentCaretHeight = value;
		foreach (GameObject caretGO in f_inputfieldCaret)
			sync_CaretHeight(caretGO);
	}

	private void syncColor_Text(GameObject text, Color? unused = null)
    {
		Selectable textSel = text.GetComponent<Selectable>();

		// Ne rien faire si le texte à traiter est le logs des chargements de fichier
		if (LoadingLogs != null && textSel == LoadingLogs)
			return;
		
		// Pour tous les textes Selectable dont la cible est le texte lui même => Mettre à jour les couleurs du selectable
		if (textSel != null && textSel.targetGraphic == text.GetComponent<Graphic>())
		{
			// forcer la couleur du texte à blanc pour être sûr que la couleur du selectable soit bien celle prise en compte
			syncGraphicColor(text, Color.white);
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
		else
		{
			Selectable parentSel = text.GetComponentInParent<Selectable>(true);
			// Pour les textes qui sont enfant d'un Selectable et dont le Selectable contrôle la couleur du texte => Mettre à jour la couleur du Selectable
			if (parentSel != null && parentSel.targetGraphic == text.GetComponent<Graphic>())
			{
				ColorBlock currentColor = parentSel.colors;
				currentColor.normalColor = currentNormalColor_Text;
				parentSel.colors = currentColor;
			}
			// Sinon tous les autres textes, on change simplement leur couleur sauf si on est sur un placeholder d'un input field (pour ce cas voir syncColor_Inputfield)
			else
			{
				TMP_InputField inputField = text.GetComponentInParent<TMP_InputField>(true);
				if (inputField == null || inputField.placeholder != text.GetComponent<Graphic>())
					syncGraphicColor(text, currentNormalColor_Text);
			}
		}
	}

	private void syncColor_Dropdown(GameObject go, Color? unused = null)
    {
		syncNormalColor(go, currentNormalColor_Dropdown);
		syncGraphicColor(go.transform.Find("Template").gameObject, currentNormalColor_Dropdown);
		syncGraphicColor(go.transform.Find("Arrow").gameObject, currentColor_Icon);
	}

	private void syncColor_ToggleCheckmark(GameObject go, Color? unused = null)
    {
		Toggle item = go.GetComponent<Toggle>();
		syncGraphicColor(item.graphic.gameObject, currentColor_Icon);
	}

	private void sync_Inputfield(GameObject go, Color? unused = null)
	{
		TMP_InputField input = go.GetComponent<TMP_InputField>();
		ColorBlock currentColor = input.colors;
		currentColor.normalColor = currentNormalColor_Inputfield;
		currentColor.pressedColor = currentSelectedColor_Inputfield;
		input.colors = currentColor;
		input.placeholder.color = currentPlaceholderColor;
		input.selectionColor = currentSelectionColor_Inputfield;
		input.caretColor = currentCaretColor_Inputfield;
		input.caretWidth = (currentCaretWidth+1)*2;
	}

	private void sync_CaretHeight(GameObject go)
	{
		go.transform.localScale = new Vector3(1f, currentCaretHeight + 1, 1f);
	}

	private void syncNormalColor(GameObject go, Color? color)
	{
		Selectable selectable = go.GetComponent<Selectable>();
		ColorBlock currentColor = selectable.colors;
		currentColor.normalColor = color ?? Color.magenta;
		selectable.colors = currentColor;
	}

	private void syncHighlightedColor(GameObject go, Color? unused = null)
	{
		Selectable select = go.GetComponent<Selectable>();
		ColorBlock currentColor = select.colors;
		currentColor.highlightedColor = currentHighlightedColor;
		currentColor.pressedColor = currentPressedColor;
		currentColor.selectedColor = currentSelectedColor;
		currentColor.disabledColor = currentDisabledColor;
		select.colors = currentColor;
	}

	private void syncColor_Panel(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, currentColor_Panel);
		Transform texture = go.transform.Find("Texture");
		if (texture != null)
			syncGraphicColor(texture.gameObject, currentColor_PanelTexture);
	}

	public void setBorderTickness(int value)
	{
		currentBorderThickness = value+1;
		syncColor(f_borders, syncBorderProperties);
	}

	private void syncBorderProperties(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, currentColor_Border);
		go.GetComponent<Image>().pixelsPerUnitMultiplier = 1f / currentBorderThickness;
	}

	private void syncGraphicColor(GameObject go, Color? color)
	{
		go.GetComponent<Graphic>().color = color ?? Color.magenta;
	}
}
