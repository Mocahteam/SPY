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
	private Family f_buttonsIcon = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("ButtonIcon")); // des boutons comme pour ouvrir les paramètres ou augmenter/réduire la taille de l'UI ou ouvrir les compétences, les icones des dropdown et des toggle...
	private Family f_buttonsPlay = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("PlayButton"));
	private Family f_buttonsPause = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("PauseButton"));
	private Family f_buttonsStop = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("StopButton"));
	private Family f_highlightable = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)));
	private Family f_SyncSelectedColor = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_SyncSelectedColor"));
	private Family f_panels1 = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Panel"));
	private Family f_panels2 = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Panel2"));
	private Family f_panels3 = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Panel3"));
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

	private DefaultSettingsValues dsf;
	public Transform settingsWindow;
	private Transform settingsContent;
	private FlexibleColorPicker flexibleColorPicker;
	public CanvasScaler [] canvasScaler;
	public Selectable LoadingLogs;
	
	private int currentQuality;
	private int currentInteractionMode;
	private float currentUIScale;
	private int currentWallTransparency;
	private int currentGameView;
	private int currentTooltipView;
	private int currentFont;
	private int currentCaretWidth;
	private int currentCaretHeight;
	private int currentCharSpacing;
	private int currentWordSpacing;
	private int currentLineSpacing;
	private int currentParagraphSpacing;
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
	private Color currentColor_Panel1;
	private Color currentColor_Panel2;
	private Color currentColor_Panel3;
	private Color currentColor_PanelTexture;
	private Color currentColor_Border;
	private int currentBorderThickness;
	private Color currentNormalColor_Scrollbar;
	private Color currentBackgroundColor_Scrollbar;
	private Color currentBackgroundColor_Scrollview;
	private Color currentNormalColor_Toggle;
	private Color currentColor_Tooltip;
	private Color currentPlayButtonColor;
	private Color currentPauseButtonColor;
	private Color currentStopButtonColor;

	private TMP_Text currentSizeText;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		settingsContent = settingsWindow.Find("BackgroundPanel/Scroll View/Viewport/Content");
		flexibleColorPicker = settingsWindow.GetComponentInChildren<FlexibleColorPicker>(true);
		dsf = settingsWindow.GetComponent<DefaultSettingsValues>();

		if (Application.platform == RuntimePlatform.WebGLPlayer && ClearPlayerPrefs())
			PlayerPrefs.DeleteAll();

		f_allTexts.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_allTexts.addEntryCallback(syncSpacing_Text);
		f_textsSelectable.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_modifiableFonts.addEntryCallback(syncFont);
		f_fixedFonts.addEntryCallback(fixFont);
		f_dropdown.addEntryCallback(delegate (GameObject go) { syncColor_Dropdown(go); });
		f_inputfield.addEntryCallback(delegate (GameObject go) { sync_Inputfield(go); });
		f_inputfieldCaret.addEntryCallback(sync_CaretHeight);
		f_buttons.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, currentNormalColor_Button); });
		f_buttonsIcon.addEntryCallback(delegate (GameObject go) {syncNormalColor(go, currentColor_Icon); });
		f_buttonsPlay.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, currentPlayButtonColor); });
		f_buttonsPause.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, currentPauseButtonColor); });
		f_buttonsStop.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, currentStopButtonColor); });
		f_highlightable.addEntryCallback(delegate (GameObject go) { syncHighlightedColor(go); });
		f_SyncSelectedColor.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, currentSelectedColor); });
		f_panels1.addEntryCallback(delegate (GameObject go) { syncColor_Panel(go); });
		f_panels2.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, currentColor_Panel2); });
		f_panels3.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, currentColor_Panel3); });
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
		foreach (GameObject textGO in f_allTexts)
			syncSpacing_Text(textGO);
	}

	private void syncColors()
    {
		syncColor(f_allTexts, syncColor_Text);
		SyncLocalization.instance.syncLocale();
		syncColor(f_dropdown, syncColor_Dropdown);
		syncColor(f_inputfield, sync_Inputfield);
		syncColor(f_buttons, syncNormalColor, currentNormalColor_Button);
		syncColor(f_buttonsIcon, syncNormalColor, currentColor_Icon);
		syncColor(f_buttonsPlay, syncNormalColor, currentPlayButtonColor);
		syncColor(f_buttonsPause, syncNormalColor, currentPauseButtonColor);
		syncColor(f_buttonsStop, syncNormalColor, currentStopButtonColor);
		syncColor(f_highlightable, syncHighlightedColor);
		syncColor(f_SyncSelectedColor, syncGraphicColor, currentSelectedColor);
		syncColor(f_panels1, syncColor_Panel);
		syncColor(f_panels2, syncGraphicColor, currentColor_Panel2);
		syncColor(f_panels3, syncGraphicColor, currentColor_Panel3);
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
		currentQuality = PlayerPrefs.GetInt("quality", dsf.defaultQuality);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Quality").GetComponentInChildren<TMP_Dropdown>().value = currentQuality;

		currentInteractionMode = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : dsf.defaultInteractionMode);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = currentInteractionMode;

		// définition de la taille de l'interface
		currentSizeText = settingsContent.Find("SectionGraphic/GridContainer/Grid/UISize").Find("CurrentSize").GetComponent<TMP_Text>();
		currentUIScale = PlayerPrefs.GetFloat("UIScale", (float)Math.Max(dsf.defaultUIScale, Math.Round((double)Screen.currentResolution.width / 2048, 2))); // do not reduce scale under defaultUIScale and multiply scale for definition higher than 2048
		currentSizeText.text = currentUIScale + "";
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = currentUIScale;

		currentWallTransparency = PlayerPrefs.GetInt("wallTransparency", dsf.defaultWallTransparency);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/WallTransparency").GetComponentInChildren<TMP_Dropdown>().value = currentWallTransparency;

		currentGameView = PlayerPrefs.GetInt("orthographicView", dsf.defaultGameView);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/GameView").GetComponentInChildren<TMP_Dropdown>().value = currentGameView;

		currentTooltipView = PlayerPrefs.GetInt("tooltipView", dsf.defaultTooltipView);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Tooltip").GetComponentInChildren<TMP_Dropdown>().value = currentTooltipView;

		currentFont = PlayerPrefs.GetInt("font", dsf.defaultFont);
		settingsContent.Find("SectionText/GridContainer/Grid/FontDropdown").GetComponentInChildren<TMP_Dropdown>().value = currentFont;

		currentCaretWidth = PlayerPrefs.GetInt("caretWidth", dsf.defaultCaretWidth);
		settingsContent.Find("SectionText/GridContainer/Grid/CaretWidth").GetComponentInChildren<TMP_Dropdown>().value = currentCaretWidth;
		currentCaretHeight = PlayerPrefs.GetInt("caretHeight", dsf.defaultCaretHeight);
		settingsContent.Find("SectionText/GridContainer/Grid/CaretHeight").GetComponentInChildren<TMP_Dropdown>().value = currentCaretHeight;

		currentCharSpacing = PlayerPrefs.GetInt("charSpacing", dsf.defaultCharSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/CharSpacing").GetComponentInChildren<TMP_Dropdown>().value = currentCharSpacing;
		currentWordSpacing = PlayerPrefs.GetInt("wordSpacing", dsf.defaultWordSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/WordSpacing").GetComponentInChildren<TMP_Dropdown>().value = currentWordSpacing;
		currentLineSpacing = PlayerPrefs.GetInt("lineSpacing", dsf.defaultLineSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/LineSpacing").GetComponentInChildren<TMP_Dropdown>().value = currentLineSpacing;
		currentParagraphSpacing = PlayerPrefs.GetInt("paragraphSpacing", dsf.defaultParagraphSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/ParagraphSpacing").GetComponentInChildren<TMP_Dropdown>().value = currentParagraphSpacing;

		// Synchronisation de la couleur des textes
		syncPlayerPrefColor("TextColorNormal", dsf.defaultNormalColor_Text, out currentNormalColor_Text, "SectionColor/GridContainer/Grid/ColorTextNormal");
		syncPlayerPrefColor("TextColorSelected", dsf.defaultSelectedColor_Text, out currentSelectedColor_Text, "SectionColor/GridContainer/Grid/ColorTextSelected");
		syncPlayerPrefColor("PlaceholderColor", dsf.defaultPlaceholderColor, out currentPlaceholderColor, "SectionColor/GridContainer/Grid/ColorPlaceholder");
		// Synchronisation de la couleur des dropdown
		syncPlayerPrefColor("DropdownColorNormal", dsf.defaultNormalColor_Dropdown, out currentNormalColor_Dropdown, "SectionColor/GridContainer/Grid/ColorDropdownNormal");
		// Synchronisation de la couleur des inputfield
		syncPlayerPrefColor("InputfieldColorNormal", dsf.defaultNormalColor_Inputfield, out currentNormalColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldNormal");
		syncPlayerPrefColor("InputfieldColorSelected", dsf.defaultSelectedColor_Inputfield, out currentSelectedColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldSelected");
		syncPlayerPrefColor("InputfieldColorSelection", dsf.defaultSelectionColor_Inputfield, out currentSelectionColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldSelection");
		syncPlayerPrefColor("InputfieldColorCaret", dsf.defaultCaretColor_Inputfield, out currentCaretColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldCaret");
		// Synchronisation de la couleur des bouttons
		syncPlayerPrefColor("ButtonColorNormal", dsf.defaultNormalColor_Button, out currentNormalColor_Button, "SectionColor/GridContainer/Grid/ColorButtonNormal");
		// Synchronisation de la couleur des highlighted
		syncPlayerPrefColor("HighlightedColor", dsf.defaultHighlightedColor, out currentHighlightedColor, "SectionColor/GridContainer/Grid/ColorHighlighted");
		// Synchronisation de la couleur des pressed
		syncPlayerPrefColor("PressedColor", dsf.defaultPressedColor, out currentPressedColor, "SectionColor/GridContainer/Grid/ColorPressed");
		// Synchronisation de la couleur des selected
		syncPlayerPrefColor("SelectedColor", dsf.defaultSelectedColor, out currentSelectedColor, "SectionColor/GridContainer/Grid/ColorSelected");
		// Synchronisation de la couleur des disabled
		syncPlayerPrefColor("DisabledColor", dsf.defaultDisabledColor, out currentDisabledColor, "SectionColor/GridContainer/Grid/ColorDisabled");
		// Synchronisation de la couleur des bouttons icônes
		syncPlayerPrefColor("IconColor", dsf.defaultColor_Icon, out currentColor_Icon, "SectionColor/GridContainer/Grid/ColorIcon");
		// Synchronisation de la couleur des panels
		syncPlayerPrefColor("Panel1Color", dsf.defaultColor_Panel1, out currentColor_Panel1, "SectionColor/GridContainer/Grid/ColorPanel1");
		syncPlayerPrefColor("Panel2Color", dsf.defaultColor_Panel2, out currentColor_Panel2, "SectionColor/GridContainer/Grid/ColorPanel2");
		syncPlayerPrefColor("Panel3Color", dsf.defaultColor_Panel3, out currentColor_Panel3, "SectionColor/GridContainer/Grid/ColorPanel3");
		syncPlayerPrefColor("PanelColorTexture", dsf.defaultColor_PanelTexture, out currentColor_PanelTexture, "SectionColor/GridContainer/Grid/ColorPanelTexture");
		// Synchronisation des propriétés de bordure
		syncPlayerPrefColor("BorderColor", dsf.defaultColor_Border, out currentColor_Border, "SectionColor/GridContainer/Grid/ColorBorder");
		currentBorderThickness = PlayerPrefs.GetInt("BorderThickness", dsf.defaultBorderThickness);
		settingsContent.Find("SectionColor/GridContainer/Grid/ThicknessBorder").GetComponentInChildren<TMP_Dropdown>().value = currentBorderThickness-1;
		// Synchronisation de la couleur des scrollbars
		syncPlayerPrefColor("ScrollbarColorNormal", dsf.defaultNormalColor_Scrollbar, out currentNormalColor_Scrollbar, "SectionColor/GridContainer/Grid/ColorScrollbarNormal");
		syncPlayerPrefColor("ScrollbarColorBackground", dsf.defaultBackgroundColor_Scrollbar, out currentBackgroundColor_Scrollbar, "SectionColor/GridContainer/Grid/ColorScrollbarBackground");
		syncPlayerPrefColor("ScrollviewColor", dsf.defaultBackgroundColor_Scrollview, out currentBackgroundColor_Scrollview, "SectionColor/GridContainer/Grid/ColorScrollview");
		// Synchronisation de la couleur des toggles
		syncPlayerPrefColor("ToggleColorNormal", dsf.defaultNormalColor_Toggle, out currentNormalColor_Toggle, "SectionColor/GridContainer/Grid/ColorToggleNormal");
		// Synchronisation de la couleur des tooltip
		syncPlayerPrefColor("TooltipColor", dsf.defaultColor_Tooltip, out currentColor_Tooltip, "SectionColor/GridContainer/Grid/ColorTooltip");
		// Synchronisation des couleurs du player
		syncPlayerPrefColor("PlayColor", dsf.defaultPlayButtonColor, out currentPlayButtonColor, "PlayerColors/GridContainer/Grid/ColorPlayButton");
		syncPlayerPrefColor("PauseColor", dsf.defaultPauseButtonColor, out currentPauseButtonColor, "PlayerColors/GridContainer/Grid/ColorPauseButton");
		syncPlayerPrefColor("StopColor", dsf.defaultStopButtonColor, out currentStopButtonColor, "PlayerColors/GridContainer/Grid/ColorStopButton");
	}

	private void syncPlayerPrefColor(string playerPrefKey, Color defaultColor, out Color currentColor, string goName)
	{
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString(playerPrefKey, ColorUtility.ToHtmlStringRGBA(defaultColor)), out currentColor);
		settingsContent.Find(goName+"/ButtonWithBorder").GetComponent<Image>().color = currentColor;
	}

	public void saveParameters()
    {
		// TODO : voir comment gérer la sauvegarde des PlayerPref dissiminés dans le code, cas du orthographicView
		PlayerPrefs.SetInt("quality", currentQuality);
		PlayerPrefs.SetInt("interaction", currentInteractionMode);
		PlayerPrefs.SetFloat("UIScale", currentUIScale);
		PlayerPrefs.SetInt("wallTransparency", currentWallTransparency);
		PlayerPrefs.SetInt("orthographicView", currentGameView);
		PlayerPrefs.SetInt("tooltipView", currentTooltipView);
		PlayerPrefs.SetInt("font", currentFont);
		PlayerPrefs.SetInt("caretWidth", currentCaretWidth);
		PlayerPrefs.SetInt("caretHeight", currentCaretHeight);
		PlayerPrefs.SetInt("charSpacing", currentCharSpacing);
		PlayerPrefs.SetInt("wordSpacing", currentWordSpacing);
		PlayerPrefs.SetInt("lineSpacing", currentLineSpacing);
		PlayerPrefs.SetInt("paragraphSpacing", currentParagraphSpacing);
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
		PlayerPrefs.SetString("Panel1Color", ColorUtility.ToHtmlStringRGBA(currentColor_Panel1));
		PlayerPrefs.SetString("Panel2Color", ColorUtility.ToHtmlStringRGBA(currentColor_Panel2));
		PlayerPrefs.SetString("Panel3Color", ColorUtility.ToHtmlStringRGBA(currentColor_Panel3));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(currentColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(currentColor_Border));
		PlayerPrefs.SetInt("BorderThickness", currentBorderThickness);
		PlayerPrefs.SetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Scrollbar));
		PlayerPrefs.SetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(currentBackgroundColor_Scrollbar));
		PlayerPrefs.SetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(currentBackgroundColor_Scrollview));
		PlayerPrefs.SetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(currentNormalColor_Toggle));
		PlayerPrefs.SetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(currentColor_Tooltip));
		PlayerPrefs.SetString("PlayColor", ColorUtility.ToHtmlStringRGBA(currentPlayButtonColor));
		PlayerPrefs.SetString("PauseColor", ColorUtility.ToHtmlStringRGBA(currentPauseButtonColor));
		PlayerPrefs.SetString("StopColor", ColorUtility.ToHtmlStringRGBA(currentStopButtonColor));
		PlayerPrefs.Save();
		// TODO : Penser à sauvegarder dans la BD le choix de la langue

	}

	public void resetParameters()
    {
		currentQuality = dsf.defaultQuality;
		currentInteractionMode = dsf.defaultInteractionMode;
		currentUIScale = dsf.defaultUIScale;
		currentWallTransparency = dsf.defaultWallTransparency;
		currentGameView = dsf.defaultGameView;
		currentTooltipView = dsf.defaultTooltipView;
		currentFont = dsf.defaultFont;
		currentCaretWidth = dsf.defaultCaretWidth;
		currentCaretHeight = dsf.defaultCaretHeight;
		currentCharSpacing = dsf.defaultCharSpacing;
		currentWordSpacing = dsf.defaultWordSpacing;
		currentLineSpacing = dsf.defaultLineSpacing;
		currentParagraphSpacing = dsf.defaultParagraphSpacing;
		currentNormalColor_Text = dsf.defaultNormalColor_Text;
		currentSelectedColor_Text = dsf.defaultSelectedColor_Text;
		currentPlaceholderColor = dsf.defaultPlaceholderColor;
		currentNormalColor_Dropdown = dsf.defaultNormalColor_Dropdown;
		currentNormalColor_Inputfield = dsf.defaultNormalColor_Inputfield;
		currentSelectedColor_Inputfield = dsf.defaultSelectedColor_Inputfield;
		currentSelectionColor_Inputfield = dsf.defaultSelectionColor_Inputfield;
		currentCaretColor_Inputfield = dsf.defaultCaretColor_Inputfield;
		currentNormalColor_Button = dsf.defaultNormalColor_Button;
		currentHighlightedColor = dsf.defaultHighlightedColor;
		currentPressedColor = dsf.defaultPressedColor;
		currentSelectedColor = dsf.defaultSelectedColor;
		currentDisabledColor = dsf.defaultDisabledColor;
		currentColor_Icon = dsf.defaultColor_Icon;
		currentColor_Panel1 = dsf.defaultColor_Panel1;
		currentColor_Panel2 = dsf.defaultColor_Panel2;
		currentColor_Panel3 = dsf.defaultColor_Panel3;
		currentColor_PanelTexture = dsf.defaultColor_PanelTexture;
		currentColor_Border = dsf.defaultColor_Border;
		currentBorderThickness = dsf.defaultBorderThickness;
		currentNormalColor_Scrollbar = dsf.defaultNormalColor_Scrollbar;
		currentBackgroundColor_Scrollbar = dsf.defaultBackgroundColor_Scrollbar;
		currentBackgroundColor_Scrollview = dsf.defaultBackgroundColor_Scrollview;
		currentNormalColor_Toggle = dsf.defaultNormalColor_Toggle;
		currentColor_Tooltip = dsf.defaultColor_Tooltip;
		currentPlayButtonColor = dsf.defaultPlayButtonColor;
		currentPauseButtonColor = dsf.defaultPauseButtonColor;
		currentStopButtonColor = dsf.defaultStopButtonColor;

		saveParameters();
		// Synchronisation des PlayerPrefs avec les UI
		loadPlayerPrefs();

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
			case "Panel1Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Panel1 = c;
					syncColor(f_panels1, syncColor_Panel);
				});
				break;
			case "Panel2Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Panel2 = c;
					syncColor(f_panels2, syncGraphicColor, currentColor_Panel2);
				});
				break;
			case "Panel3Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Panel3 = c;
					syncColor(f_panels3, syncGraphicColor, currentColor_Panel3);
				});
				break;
			case "PanelColorTexture":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_PanelTexture = c;
					syncColor(f_panels1, syncColor_Panel);
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
			case "PlayColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Icon = c;
					syncColor(f_buttonsPlay, syncNormalColor, currentPlayButtonColor);
				});
				break;
			case "PauseColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Icon = c;
					syncColor(f_buttonsPause, syncNormalColor, currentPauseButtonColor);
				});
				break;
			case "StopColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					currentColor_Icon = c;
					syncColor(f_buttonsStop, syncNormalColor, currentStopButtonColor);
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

	public void setTooltipView(int value)
	{
		currentTooltipView = value;
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
			inputField.fontAsset = dsf.fonts[currentFont];
		else
			go.GetComponent<TextMeshProUGUI>().font = dsf.fonts[currentFont];
	}

	// Fonction utilisée pour définir la font dans la liste déroulante de sélection de la font dans les paramètres
	private void fixFont(GameObject go)
    {
		TextMeshProUGUI option = go.GetComponent<TextMeshProUGUI>();
		switch (option.text)
        {
			case "Arial": option.font = dsf.fonts[0];
				break;
			case "Comic Sans MS":
				option.font = dsf.fonts[1];
				break;
			case "Liberation Sans SDF":
				option.font = dsf.fonts[2];
				break;
			case "Luciole":
				option.font = dsf.fonts[3];
				break;
			case "Open Dyslexic":
				option.font = dsf.fonts[4];
				break;
			case "Orbitron":
				option.font = dsf.fonts[5];
				break;
			case "Roboto":
				option.font = dsf.fonts[6];
				break;
			case "Tahoma":
				option.font = dsf.fonts[7];
				break;
			case "Verdana":
				option.font = dsf.fonts[8];
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
		syncGraphicColor(go, currentColor_Panel1);
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

	public void setCharSpacing(int value)
    {
		currentCharSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setWordSpacing(int value)
	{
		currentWordSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setLineSpacing(int value)
	{
		currentLineSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setParagraphSpacing(int value)
	{
		currentParagraphSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	private void syncSpacing_Text(GameObject textGO)
    {
		TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
		// Transformation des numéro d'item sélectionné en valeurs d'espacement
		text.characterSpacing = (currentCharSpacing - 2) * 10;
		text.wordSpacing = (currentWordSpacing - 2) * 10;
		text.lineSpacing = (currentLineSpacing - 2) * 10;
		text.paragraphSpacing = (currentParagraphSpacing - 2) * 10;
	}
}
