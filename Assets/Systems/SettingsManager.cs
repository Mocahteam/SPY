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
	private Family f_icons = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(Selectable), typeof(Image)), new AnyOfTags("UI_Icon"));
	private Family f_buttonsPlay = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("PlayButton"));
	private Family f_buttonsPause = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("PauseButton"));
	private Family f_buttonsStop = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("StopButton", "ResetButton", "RemoveButton"));
	private Family f_selectable = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar), typeof(Selectable)), new NoneOfComponents(typeof(TextMeshProUGUI), typeof(LibraryItemRef), typeof(ElementToDrag)));
	private Family f_SyncSelectedColor = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_SyncSelectedColor"));
	private Family f_panels1 = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Panel"));
	private Family f_panels2 = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Panel2"));
	private Family f_panels3 = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Panel3"));
	private Family f_borders = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_Border"));
	private Family f_scrollbar = FamilyManager.getFamily(new AllOfComponents(typeof(Scrollbar), typeof(Image)));
	private Family f_scrollview = FamilyManager.getFamily(new AllOfComponents(typeof(ScrollRect), typeof(Image)), new NoneOfComponents(typeof(AutoBind))); // Le Autobind permet d'exclure les scrollRect contenus dans des dropdown
	private Family f_toggle = FamilyManager.getFamily(new AllOfComponents(typeof(Toggle)), new NoneOfTags("UI_Avatar"));
	private Family f_tooltip = FamilyManager.getFamily(new AllOfComponents(typeof(Tooltip), typeof(Image)));
	private Family f_blocks = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable)), new AnyOfComponents(typeof(LibraryItemRef), typeof(ElementToDrag)), new AnyOfTags("UI_Action", "UI_Control", "UI_Operator", "UI_Captor"));
	private Family f_dropArea = FamilyManager.getFamily(new AnyOfComponents(typeof(DropZone), typeof(ReplacementSlot))); // Les drops zones et les replacement slots
	private Family f_highlightable = FamilyManager.getFamily(new AnyOfComponents(typeof(Highlightable), typeof(LibraryItemRef)));
	private Family f_tileSelection = FamilyManager.getFamily(new AllOfComponents(typeof(SpriteRenderer)), new AnyOfTags("TileSelection"));
	private Family f_conditionNotif = FamilyManager.getFamily(new AnyOfComponents(typeof(Image)), new AnyOfTags("ConditionNotif"));
	private Family f_canvasScaler = FamilyManager.getFamily(new AllOfComponents(typeof(CanvasScaler)));
	private Family f_avatarTarget = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_AvatarTarget"));


	public static SettingsManager instance;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern bool ClearPlayerPrefs(); // call javascript

	[DllImport("__Internal")]
	private static extern void Save(string content, string defaultName); // call javascript

	private DefaultSettingsValues ds;
	private CurrentSettingsValues cs;
	public Transform settingsWindow;
	private Transform settingsContent;
	private FlexibleColorPicker flexibleColorPicker;
	public Selectable LoadingLogs;
	public bool settingsUpdated = false;

	private UserData userData;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			userData = go.GetComponent<UserData>();

		settingsContent = settingsWindow.Find("BackgroundPanel/Scroll View/Viewport/Content");
		flexibleColorPicker = settingsWindow.GetComponentInChildren<FlexibleColorPicker>(true);
		ds = settingsWindow.GetComponent<DefaultSettingsValues>();
		cs = settingsWindow.GetComponent<CurrentSettingsValues>();

		if (Application.platform == RuntimePlatform.WebGLPlayer && ClearPlayerPrefs())
			PlayerPrefs.DeleteAll();

		loadPlayerPrefs();
		saveParameters(); // in case no PlayerPrefs exists, loadPlayerPrefs init currentValues with defaultValues, then we ensure to save currentValues to PlayerPrefs

		f_allTexts.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_allTexts.addEntryCallback(syncSpacing_Text);
		f_textsSelectable.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_modifiableFonts.addEntryCallback(syncFont);
		f_fixedFonts.addEntryCallback(fixFont);
		f_dropdown.addEntryCallback(delegate (GameObject go) { syncColor_Dropdown(go); });
		f_inputfield.addEntryCallback(delegate (GameObject go) { sync_Inputfield(go); });
		f_inputfieldCaret.addEntryCallback(sync_CaretHeight);
		f_buttons.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, cs.values.currentNormalColor_Button); });
		f_icons.addEntryCallback(delegate (GameObject go) { syncIconColor(go, cs.values.currentColor_Icon); });
		f_buttonsPlay.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, cs.values.currentPlayButtonColor); });
		f_buttonsPause.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, cs.values.currentPauseButtonColor); });
		f_buttonsStop.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, cs.values.currentStopButtonColor); });
		f_selectable.addEntryCallback(delegate (GameObject go) { syncHighlightedColor(go); });
		f_SyncSelectedColor.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, cs.values.currentSelectedColor); });
		f_panels1.addEntryCallback(delegate (GameObject go) { syncColor_Panel(go); });
		f_panels2.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, cs.values.currentColor_Panel2); });
		f_panels3.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, cs.values.currentColor_Panel3); });
		f_borders.addEntryCallback(delegate (GameObject go) { syncBorderProperties(go); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, cs.values.currentNormalColor_Scrollbar); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, cs.values.currentBackgroundColor_Scrollbar); });
		f_scrollview.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, cs.values.currentBackgroundColor_Scrollview); });
		f_toggle.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, cs.values.currentNormalColor_Toggle); });
		f_tooltip.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, cs.values.currentColor_Tooltip); });
		f_blocks.addEntryCallback(delegate (GameObject go) { syncBlockColor(go); });
		f_dropArea.addEntryCallback(delegate (GameObject go) { syncDropAreaColor(go); });
		f_highlightable.addEntryCallback(delegate (GameObject go) { setHighlightingColor(go); });
		f_tileSelection.addEntryCallback(delegate (GameObject go) { syncTileColor(go); });
		f_conditionNotif.addEntryCallback(delegate (GameObject go) { syncConditionNotif(go); });

		f_settingsOpened.addEntryCallback(delegate (GameObject unused) { syncSettingsUI(); });

		MainLoop.instance.StartCoroutine(waitLocalizationLoaded());

		Pause = true;
	}
	private IEnumerator waitLocalizationLoaded()
	{
		while (f_localizationLoaded.Count == 0)
			yield return null;
		applySettings();
	}

	private void applySettings()
    {
		syncColors();
		setQualitySetting(cs.values.currentQuality);
		syncCanvasScaler();
		setWallTransparency(cs.values.currentWallTransparency);
		setGameView(cs.values.currentGameView);
		syncFonts(cs.values.currentFont);
		setCaretWidth(cs.values.currentCaretWidth);
		setCaretHeight(cs.values.currentCaretHeight);
		setCharSpacing(cs.values.currentCharSpacing);
		setWordSpacing(cs.values.currentWordSpacing);
		setLineSpacing(cs.values.currentLineSpacing);
		setParagraphSpacing(cs.values.currentParagraphSpacing);
	}

	private void syncColors()
	{
		syncColor(f_allTexts, syncColor_Text);
		SyncLocalization.instance.syncLocale(); // because it change colors
		syncColor(f_dropdown, syncColor_Dropdown);
		syncColor(f_inputfield, sync_Inputfield);
		syncColor(f_buttons, syncNormalColor, cs.values.currentNormalColor_Button);
		syncColor(f_icons, syncIconColor, cs.values.currentColor_Icon);
		syncColor(f_buttonsPlay, syncNormalColor, cs.values.currentPlayButtonColor);
		syncColor(f_buttonsPause, syncNormalColor, cs.values.currentPauseButtonColor);
		syncColor(f_buttonsStop, syncNormalColor, cs.values.currentStopButtonColor);
		syncColor(f_selectable, syncHighlightedColor);
		syncColor(f_SyncSelectedColor, syncGraphicColor, cs.values.currentSelectedColor);
		syncColor(f_panels1, syncColor_Panel);
		syncColor(f_panels2, syncGraphicColor, cs.values.currentColor_Panel2);
		syncColor(f_panels3, syncGraphicColor, cs.values.currentColor_Panel3);
		syncColor(f_borders, syncBorderProperties);
		syncColor(f_scrollbar, syncNormalColor, cs.values.currentNormalColor_Scrollbar);
		syncColor(f_scrollbar, syncGraphicColor, cs.values.currentBackgroundColor_Scrollbar);
		syncColor(f_scrollview, syncGraphicColor, cs.values.currentBackgroundColor_Scrollview);
		syncColor(f_toggle, syncNormalColor, cs.values.currentNormalColor_Toggle);
		syncColor(f_tooltip, syncGraphicColor, cs.values.currentColor_Tooltip);
		syncColor(f_blocks, syncBlockColor);
		syncColor(f_dropArea, syncDropAreaColor);
		syncColor(f_highlightable, setHighlightingColor);
		syncColor(f_tileSelection, syncTileColor);
		syncColor(f_conditionNotif, syncConditionNotif);
	}

	// lit les PlayerPrefs et initialise les currentSettingsValues
	private void loadPlayerPrefs()
	{
		// Voir SyncLocalization pour la gestion de la langue
		cs.values.currentQuality = PlayerPrefs.GetInt("quality", ds.defaultQuality);
		cs.values.currentInteractionMode = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : ds.defaultInteractionMode);
		// définition de la taille de l'interface
		float uiWidth = f_canvasScaler.Count > 0 ? (f_canvasScaler.First().transform as RectTransform).rect.width : Screen.currentResolution.width;
		cs.values.currentUIScale = PlayerPrefs.GetFloat("UIScale", (float)Math.Max(ds.defaultUIScale, Math.Round(uiWidth / 1280, 2))*(Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 2 : 1)); // do not reduce scale under defaultUIScale and multiply scale for definition higher than 1280. Sur mobile on multiplie l'echelle par deux
		foreach (GameObject scalerGo in f_canvasScaler)
			scalerGo.GetComponent<CanvasScaler>().scaleFactor = cs.values.currentUIScale;
		cs.values.currentWallTransparency = PlayerPrefs.GetInt("wallTransparency", ds.defaultWallTransparency);
		cs.values.currentCameraTracking = PlayerPrefs.GetInt("cameraTracking", ds.defaultCameraTracking);
		cs.values.currentGameView = PlayerPrefs.GetInt("orthographicView", ds.defaultGameView);
		cs.values.currentTooltipView = PlayerPrefs.GetInt("tooltipView", ds.defaultTooltipView);

		cs.values.currentFont = PlayerPrefs.GetInt("font", ds.defaultFont);
		cs.values.currentCaretWidth = PlayerPrefs.GetInt("caretWidth", ds.defaultCaretWidth);
		cs.values.currentCaretHeight = PlayerPrefs.GetInt("caretHeight", ds.defaultCaretHeight);
		cs.values.currentCharSpacing = PlayerPrefs.GetInt("charSpacing", ds.defaultCharSpacing);
		cs.values.currentWordSpacing = PlayerPrefs.GetInt("wordSpacing", ds.defaultWordSpacing);
		cs.values.currentLineSpacing = PlayerPrefs.GetInt("lineSpacing", ds.defaultLineSpacing);
		cs.values.currentParagraphSpacing = PlayerPrefs.GetInt("paragraphSpacing", ds.defaultParagraphSpacing);

		// Synchronisation de la couleur des textes
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(ds.defaultNormalColor_Text)), out cs.values.currentNormalColor_Text);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(ds.defaultSelectedColor_Text)), out cs.values.currentSelectedColor_Text);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PlaceholderColor", ColorUtility.ToHtmlStringRGBA(ds.defaultPlaceholderColor)), out cs.values.currentPlaceholderColor);
		// Synchronisation de la couleur des dropdown
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("DropdownColorNormal", ColorUtility.ToHtmlStringRGBA(ds.defaultNormalColor_Dropdown)), out cs.values.currentNormalColor_Dropdown);
		// Synchronisation de la couleur des inputfield
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorNormal", ColorUtility.ToHtmlStringRGBA(ds.defaultNormalColor_Inputfield)), out cs.values.currentNormalColor_Inputfield);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorSelected", ColorUtility.ToHtmlStringRGBA(ds.defaultSelectedColor_Inputfield)), out cs.values.currentSelectedColor_Inputfield);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorSelection", ColorUtility.ToHtmlStringRGBA(ds.defaultSelectionColor_Inputfield)), out cs.values.currentSelectionColor_Inputfield);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorCaret", ColorUtility.ToHtmlStringRGBA(ds.defaultCaretColor_Inputfield)), out cs.values.currentCaretColor_Inputfield);
		// Synchronisation de la couleur des bouttons
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(ds.defaultNormalColor_Button)), out cs.values.currentNormalColor_Button);
		// Synchronisation de la couleur des highlighted
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(ds.defaultHighlightedColor)), out cs.values.currentHighlightedColor);
		// Synchronisation de la couleur des pressed
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PressedColor", ColorUtility.ToHtmlStringRGBA(ds.defaultPressedColor)), out cs.values.currentPressedColor);
		// Synchronisation de la couleur des selected
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(ds.defaultSelectedColor)), out cs.values.currentSelectedColor);
		// Synchronisation de la couleur des disabled
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(ds.defaultDisabledColor)), out cs.values.currentDisabledColor);
		// Synchronisation de la couleur des bouttons icônes
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("IconColor", ColorUtility.ToHtmlStringRGBA(ds.defaultColor_Icon)), out cs.values.currentColor_Icon);
		// Synchronisation de la couleur des panels
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("Panel1Color", ColorUtility.ToHtmlStringRGBA(ds.defaultColor_Panel1)), out cs.values.currentColor_Panel1);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("Panel2Color", ColorUtility.ToHtmlStringRGBA(ds.defaultColor_Panel2)), out cs.values.currentColor_Panel2);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("Panel3Color", ColorUtility.ToHtmlStringRGBA(ds.defaultColor_Panel3)), out cs.values.currentColor_Panel3);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(ds.defaultColor_PanelTexture)), out cs.values.currentColor_PanelTexture);
		// Synchronisation des propriétés de bordure
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("BorderColor", ColorUtility.ToHtmlStringRGBA(ds.defaultColor_Border)), out cs.values.currentColor_Border);
		cs.values.currentBorderThickness = PlayerPrefs.GetInt("BorderThickness", ds.defaultBorderThickness);
		// Synchronisation de la couleur des scrollbars
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(ds.defaultNormalColor_Scrollbar)), out cs.values.currentNormalColor_Scrollbar);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(ds.defaultBackgroundColor_Scrollbar)), out cs.values.currentBackgroundColor_Scrollbar);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(ds.defaultBackgroundColor_Scrollview)), out cs.values.currentBackgroundColor_Scrollview);
		// Synchronisation de la couleur des toggles
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(ds.defaultNormalColor_Toggle)), out cs.values.currentNormalColor_Toggle);
		// Synchronisation de la couleur des tooltip
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(ds.defaultColor_Tooltip)), out cs.values.currentColor_Tooltip);

		// Synchronisation des couleurs du player
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PlayColor", ColorUtility.ToHtmlStringRGBA(ds.defaultPlayButtonColor)), out cs.values.currentPlayButtonColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PauseColor", ColorUtility.ToHtmlStringRGBA(ds.defaultPauseButtonColor)), out cs.values.currentPauseButtonColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("StopColor", ColorUtility.ToHtmlStringRGBA(ds.defaultStopButtonColor)), out cs.values.currentStopButtonColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ActionBlockColor", ColorUtility.ToHtmlStringRGBA(ds.defaultActionBlockColor)), out cs.values.currentActionBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ControlBlockColor", ColorUtility.ToHtmlStringRGBA(ds.defaultControlBlockColor)), out cs.values.currentControlBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("OperatorBlockColor", ColorUtility.ToHtmlStringRGBA(ds.defaultOperatorBlockColor)), out cs.values.currentOperatorBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("CaptorBlockColor", ColorUtility.ToHtmlStringRGBA(ds.defaultCaptorBlockColor)), out cs.values.currentCaptorBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("DropAreaColor", ColorUtility.ToHtmlStringRGBA(ds.defaultDropAreaColor)), out cs.values.currentDropAreaColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("HighlightingColor", ColorUtility.ToHtmlStringRGBA(ds.defaultHighlightingColor)), out cs.values.currentHighlightingColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("CaptorTrueColor", ColorUtility.ToHtmlStringRGBA(ds.defaultCaptorTrueColor)), out cs.values.currentCaptorTrueColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("CaptorFalseColor", ColorUtility.ToHtmlStringRGBA(ds.defaultCaptorFalseColor)), out cs.values.currentCaptorFalseColor);
	}

	// initialise l'UI des settings à partir des currentSettingsValues
	private void syncSettingsUI()
	{
		// Voir SyncLocalization pour la gestion de la langue
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Quality").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentQuality;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentInteractionMode;
		ds.UIScale.text = cs.values.currentUIScale + "";
		settingsContent.Find("SectionGraphic/GridContainer/Grid/WallTransparency").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentWallTransparency;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/CameraTracking").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentCameraTracking;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/GameView").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentGameView;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Tooltip").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentTooltipView;

		settingsContent.Find("SectionText/GridContainer/Grid/FontDropdown").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentFont;
		settingsContent.Find("SectionText/GridContainer/Grid/CaretWidth").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentCaretWidth;
		settingsContent.Find("SectionText/GridContainer/Grid/CaretHeight").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentCaretHeight;
		settingsContent.Find("SectionText/GridContainer/Grid/CharSpacing").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentCharSpacing;
		settingsContent.Find("SectionText/GridContainer/Grid/WordSpacing").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentWordSpacing;
		settingsContent.Find("SectionText/GridContainer/Grid/LineSpacing").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentLineSpacing;
		settingsContent.Find("SectionText/GridContainer/Grid/ParagraphSpacing").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentParagraphSpacing;

		// Synchronisation de la couleur des textes
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorTextNormal/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentNormalColor_Text;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorTextSelected/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentSelectedColor_Text;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPlaceholder/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentPlaceholderColor;
		// Synchronisation de la couleur des dropdown
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorDropdownNormal/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentNormalColor_Dropdown;
		// Synchronisation de la couleur des inputfield
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldNormal/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentNormalColor_Inputfield;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldSelected/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentSelectedColor_Inputfield;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldSelection/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentSelectionColor_Inputfield;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldCaret/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentCaretColor_Inputfield;
		// Synchronisation de la couleur des bouttons
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorButtonNormal/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentNormalColor_Button;
		// Synchronisation de la couleur des highlighted
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorHighlighted/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentHighlightedColor;
		// Synchronisation de la couleur des pressed
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPressed/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentPressedColor;
		// Synchronisation de la couleur des selected
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorSelected/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentSelectedColor;
		// Synchronisation de la couleur des disabled
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorDisabled/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentDisabledColor;
		// Synchronisation de la couleur des bouttons icônes
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorIcon/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentColor_Icon;
		// Synchronisation de la couleur des panels
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanel1/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentColor_Panel1;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanel2/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentColor_Panel2;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanel3/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentColor_Panel3;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanelTexture/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentColor_PanelTexture;
		// Synchronisation des propriétés de bordure
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorBorder/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentColor_Border;
		settingsContent.Find("SectionColor/GridContainer/Grid/ThicknessBorder").GetComponentInChildren<TMP_Dropdown>().value = cs.values.currentBorderThickness - 1;
		// Synchronisation de la couleur des scrollbars
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorScrollbarNormal/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentNormalColor_Scrollbar;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorScrollbarBackground/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentBackgroundColor_Scrollbar;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorScrollview/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentBackgroundColor_Scrollview;
		// Synchronisation de la couleur des toggles
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorToggleNormal/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentNormalColor_Toggle;
		// Synchronisation de la couleur des tooltip
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorTooltip/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentColor_Tooltip;

		// Synchronisation des couleurs du player
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorPlayButton/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentPlayButtonColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorPauseButton/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentPauseButtonColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorStopButton/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentStopButtonColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorActionBlock/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentActionBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorControlBlock/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentControlBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorOperatorBlock/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentOperatorBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorCaptorBlock/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentCaptorBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorDropArea/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentDropAreaColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorHighlighting/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentHighlightingColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorCaptorTrue/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentCaptorTrueColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorCaptorFalse/ButtonWithBorder").GetComponent<Image>().color = cs.values.currentCaptorFalseColor;
	}

	// See CloseSettings button in SettingsWindows prefab
	public void saveParameters()
	{
		// Voir SyncLocalization pour la gestion de la langue
		PlayerPrefs.SetInt("quality", cs.values.currentQuality);
		PlayerPrefs.SetInt("interaction", cs.values.currentInteractionMode);
		PlayerPrefs.SetFloat("UIScale", cs.values.currentUIScale);
		PlayerPrefs.SetInt("wallTransparency", cs.values.currentWallTransparency);
		PlayerPrefs.SetInt("cameraTracking", cs.values.currentCameraTracking);
		PlayerPrefs.SetInt("orthographicView", cs.values.currentGameView);
		PlayerPrefs.SetInt("tooltipView", cs.values.currentTooltipView);
		PlayerPrefs.SetInt("font", cs.values.currentFont);
		PlayerPrefs.SetInt("caretWidth", cs.values.currentCaretWidth);
		PlayerPrefs.SetInt("caretHeight", cs.values.currentCaretHeight);
		PlayerPrefs.SetInt("charSpacing", cs.values.currentCharSpacing);
		PlayerPrefs.SetInt("wordSpacing", cs.values.currentWordSpacing);
		PlayerPrefs.SetInt("lineSpacing", cs.values.currentLineSpacing);
		PlayerPrefs.SetInt("paragraphSpacing", cs.values.currentParagraphSpacing);
		PlayerPrefs.SetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(cs.values.currentNormalColor_Text));
		PlayerPrefs.SetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(cs.values.currentSelectedColor_Text));
		PlayerPrefs.SetString("PlaceholderColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentPlaceholderColor));
		PlayerPrefs.SetString("DropdownColorNormal", ColorUtility.ToHtmlStringRGBA(cs.values.currentNormalColor_Dropdown));
		PlayerPrefs.SetString("InputfieldColorNormal", ColorUtility.ToHtmlStringRGBA(cs.values.currentNormalColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelected", ColorUtility.ToHtmlStringRGBA(cs.values.currentSelectedColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelection", ColorUtility.ToHtmlStringRGBA(cs.values.currentSelectionColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorCaret", ColorUtility.ToHtmlStringRGBA(cs.values.currentCaretColor_Inputfield));
		PlayerPrefs.SetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(cs.values.currentNormalColor_Button));
		PlayerPrefs.SetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentHighlightedColor));
		PlayerPrefs.SetString("PressedColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentPressedColor));
		PlayerPrefs.SetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentSelectedColor));
		PlayerPrefs.SetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentDisabledColor));
		PlayerPrefs.SetString("IconColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentColor_Icon));
		PlayerPrefs.SetString("Panel1Color", ColorUtility.ToHtmlStringRGBA(cs.values.currentColor_Panel1));
		PlayerPrefs.SetString("Panel2Color", ColorUtility.ToHtmlStringRGBA(cs.values.currentColor_Panel2));
		PlayerPrefs.SetString("Panel3Color", ColorUtility.ToHtmlStringRGBA(cs.values.currentColor_Panel3));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(cs.values.currentColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentColor_Border));
		PlayerPrefs.SetInt("BorderThickness", cs.values.currentBorderThickness);
		PlayerPrefs.SetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(cs.values.currentNormalColor_Scrollbar));
		PlayerPrefs.SetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(cs.values.currentBackgroundColor_Scrollbar));
		PlayerPrefs.SetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentBackgroundColor_Scrollview));
		PlayerPrefs.SetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(cs.values.currentNormalColor_Toggle));
		PlayerPrefs.SetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentColor_Tooltip));
		PlayerPrefs.SetString("PlayColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentPlayButtonColor));
		PlayerPrefs.SetString("PauseColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentPauseButtonColor));
		PlayerPrefs.SetString("StopColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentStopButtonColor));
		PlayerPrefs.SetString("ActionBlockColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentActionBlockColor));
		PlayerPrefs.SetString("ControlBlockColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentControlBlockColor));
		PlayerPrefs.SetString("OperatorBlockColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentOperatorBlockColor));
		PlayerPrefs.SetString("CaptorBlockColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentCaptorBlockColor));
		PlayerPrefs.SetString("DropAreaColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentDropAreaColor));
		PlayerPrefs.SetString("HighlightingColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentHighlightingColor));
		PlayerPrefs.SetString("CaptorTrueColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentCaptorTrueColor));
		PlayerPrefs.SetString("CaptorFalseColor", ColorUtility.ToHtmlStringRGBA(cs.values.currentCaptorFalseColor));
		PlayerPrefs.Save();
	}

	// See CloseSettings button in SettingsWindows prefab
	public void sendUserData()
	{
		settingsUpdated = true;
		if (GBL_Interface.playerName != "") // Au cas où le joueur serait en train de jouer avec les paramètres avant de s'être identifié
			GameObjectManager.addComponent<SendUserData>(MainLoop.instance.gameObject);
	}

	// See ExportSettings button in SettingsWindows prefab
	public void exportSettings()
	{
		string export = JsonUtility.ToJson(cs.values);
		if (Application.platform == RuntimePlatform.WebGLPlayer)
			Save(export, "SPY_settings.json");
		else
			Debug.Log(export);
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
	public void importSettingsFromJS(string content)
	{
		importSettings(JsonUtility.FromJson<Utility.JavaScriptData>(content).content);
	}

	public void importSettings(string content)
    {
		RawSettingsValues newSettings = JsonUtility.FromJson<RawSettingsValues>(content);
		if (newSettings != null)
		{
			settingsUpdated = true;
			cs.values = newSettings;
			saveParameters();
			applySettings();
		}
	}

	public void resetParameters()
	{
		// Volontairement on ne reset pas la langue
		cs.values.currentQuality = ds.defaultQuality;
		cs.values.currentInteractionMode = ds.defaultInteractionMode;
		float uiWidth = f_canvasScaler.Count > 0 ? (f_canvasScaler.First().transform as RectTransform).rect.width : Screen.currentResolution.width;
		cs.values.currentUIScale = (float)Math.Max(ds.defaultUIScale, Math.Round(uiWidth / 1280, 2));
		cs.values.currentWallTransparency = ds.defaultWallTransparency;
		cs.values.currentCameraTracking = ds.defaultCameraTracking;
		cs.values.currentGameView = ds.defaultGameView;
		cs.values.currentTooltipView = ds.defaultTooltipView;
		cs.values.currentFont = ds.defaultFont;
		cs.values.currentCaretWidth = ds.defaultCaretWidth;
		cs.values.currentCaretHeight = ds.defaultCaretHeight;
		cs.values.currentCharSpacing = ds.defaultCharSpacing;
		cs.values.currentWordSpacing = ds.defaultWordSpacing;
		cs.values.currentLineSpacing = ds.defaultLineSpacing;
		cs.values.currentParagraphSpacing = ds.defaultParagraphSpacing;
		cs.values.currentNormalColor_Text = ds.defaultNormalColor_Text;
		cs.values.currentSelectedColor_Text = ds.defaultSelectedColor_Text;
		cs.values.currentPlaceholderColor = ds.defaultPlaceholderColor;
		cs.values.currentNormalColor_Dropdown = ds.defaultNormalColor_Dropdown;
		cs.values.currentNormalColor_Inputfield = ds.defaultNormalColor_Inputfield;
		cs.values.currentSelectedColor_Inputfield = ds.defaultSelectedColor_Inputfield;
		cs.values.currentSelectionColor_Inputfield = ds.defaultSelectionColor_Inputfield;
		cs.values.currentCaretColor_Inputfield = ds.defaultCaretColor_Inputfield;
		cs.values.currentNormalColor_Button = ds.defaultNormalColor_Button;
		cs.values.currentHighlightedColor = ds.defaultHighlightedColor;
		cs.values.currentPressedColor = ds.defaultPressedColor;
		cs.values.currentSelectedColor = ds.defaultSelectedColor;
		cs.values.currentDisabledColor = ds.defaultDisabledColor;
		cs.values.currentColor_Icon = ds.defaultColor_Icon;
		cs.values.currentColor_Panel1 = ds.defaultColor_Panel1;
		cs.values.currentColor_Panel2 = ds.defaultColor_Panel2;
		cs.values.currentColor_Panel3 = ds.defaultColor_Panel3;
		cs.values.currentColor_PanelTexture = ds.defaultColor_PanelTexture;
		cs.values.currentColor_Border = ds.defaultColor_Border;
		cs.values.currentBorderThickness = ds.defaultBorderThickness;
		cs.values.currentNormalColor_Scrollbar = ds.defaultNormalColor_Scrollbar;
		cs.values.currentBackgroundColor_Scrollbar = ds.defaultBackgroundColor_Scrollbar;
		cs.values.currentBackgroundColor_Scrollview = ds.defaultBackgroundColor_Scrollview;
		cs.values.currentNormalColor_Toggle = ds.defaultNormalColor_Toggle;
		cs.values.currentColor_Tooltip = ds.defaultColor_Tooltip;
		cs.values.currentPlayButtonColor = ds.defaultPlayButtonColor;
		cs.values.currentPauseButtonColor = ds.defaultPauseButtonColor;
		cs.values.currentStopButtonColor = ds.defaultStopButtonColor;
		cs.values.currentActionBlockColor = ds.defaultActionBlockColor;
		cs.values.currentControlBlockColor = ds.defaultControlBlockColor;
		cs.values.currentOperatorBlockColor = ds.defaultOperatorBlockColor;
		cs.values.currentCaptorBlockColor = ds.defaultCaptorBlockColor;
		cs.values.currentDropAreaColor = ds.defaultDropAreaColor;
		cs.values.currentHighlightingColor = ds.defaultHighlightingColor;
		cs.values.currentCaptorTrueColor = ds.defaultCaptorTrueColor;
		cs.values.currentCaptorFalseColor = ds.defaultCaptorFalseColor;

		saveParameters();
		syncSettingsUI(); // synchronise les menus des settings avec les bons paramètres
		applySettings(); // applique les paramètres à tout le jeu
	}

	public void hookListener(string key)
	{
		flexibleColorPicker.onColorChange.RemoveAllListeners();
		switch (key)
		{
			case "TextColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentNormalColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "TextColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentSelectedColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "PlaceholderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentPlaceholderColor = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "DropdownColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentNormalColor_Dropdown = c;
					syncColor(f_dropdown, syncColor_Dropdown);
				});
				break;
			case "InputfieldColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentNormalColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentSelectedColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelection":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentSelectionColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorCaret":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentCaretColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "ButtonColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentNormalColor_Button = c;
					syncColor(f_buttons, syncNormalColor, cs.values.currentNormalColor_Button);
				});
				break;
			case "HighlightedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentHighlightedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "PressedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentPressedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "SelectedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentSelectedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
					syncColor(f_SyncSelectedColor, syncGraphicColor, cs.values.currentSelectedColor);
				});
				break;
			case "DisabledColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentDisabledColor = c;
					syncColor(f_allTexts, syncColor_Text);
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "IconColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentColor_Icon = c;
					syncColor(f_icons, syncIconColor, cs.values.currentColor_Icon);
				});
				break;
			case "Panel1Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentColor_Panel1 = c;
					syncColor(f_panels1, syncColor_Panel);
				});
				break;
			case "Panel2Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentColor_Panel2 = c;
					syncColor(f_panels2, syncGraphicColor, cs.values.currentColor_Panel2);
				});
				break;
			case "Panel3Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentColor_Panel3 = c;
					syncColor(f_panels3, syncGraphicColor, cs.values.currentColor_Panel3);
				});
				break;
			case "PanelColorTexture":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentColor_PanelTexture = c;
					syncColor(f_panels1, syncColor_Panel);
				});
				break;
			case "BorderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentColor_Border = c;
					syncColor(f_borders, syncBorderProperties, cs.values.currentColor_Border);
				});
				break;
			case "ScrollbarColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentNormalColor_Scrollbar = c;
					syncColor(f_scrollbar, syncNormalColor, cs.values.currentNormalColor_Scrollbar);
				});
				break;
			case "ScrollbarColorBackground":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentBackgroundColor_Scrollbar = c;
					syncColor(f_scrollbar, syncGraphicColor, cs.values.currentBackgroundColor_Scrollbar);
				});
				break;
			case "ScrollviewColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentBackgroundColor_Scrollview = c;
					syncColor(f_scrollview, syncGraphicColor, cs.values.currentBackgroundColor_Scrollview);
				});
				break;
			case "ToggleColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentNormalColor_Toggle = c;
					syncColor(f_toggle, syncNormalColor, cs.values.currentNormalColor_Toggle);
				});
				break;
			case "TooltipColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentColor_Tooltip = c;
					syncColor(f_tooltip, syncGraphicColor, cs.values.currentColor_Tooltip);
				});
				break;
			case "PlayColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentPlayButtonColor = c;
					syncColor(f_buttonsPlay, syncNormalColor, cs.values.currentPlayButtonColor);
				});
				break;
			case "PauseColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentPauseButtonColor = c;
					syncColor(f_buttonsPause, syncNormalColor, cs.values.currentPauseButtonColor);
				});
				break;
			case "StopColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentStopButtonColor = c;
					syncColor(f_buttonsStop, syncNormalColor, cs.values.currentStopButtonColor);
				});
				break;
			case "ActionBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentActionBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "ControlBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentControlBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "OperatorBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentOperatorBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "CaptorBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentCaptorBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "DropAreaColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentDropAreaColor = c;
					syncColor(f_dropArea, syncDropAreaColor);
				});
				break; 
			case "HighlightingColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentHighlightingColor = c;
					syncColor(f_highlightable, setHighlightingColor);
					syncColor(f_tileSelection, syncTileColor);
				});
				break;
			case "CaptorTrueColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentCaptorTrueColor = c;
					syncColor(f_conditionNotif, syncConditionNotif);
				});
				break;
			case "CaptorFalseColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					cs.values.currentCaptorFalseColor = c;
					syncColor(f_conditionNotif, syncConditionNotif);
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
		cs.values.currentQuality = value;
	}

	public void setInteraction(int value)
	{
		cs.values.currentInteractionMode = value;
	}

	public void increaseUISize()
	{
		cs.values.currentUIScale += 0.25f;
		syncCanvasScaler();
		ds.UIScale.text = cs.values.currentUIScale + "";
	}
	public void decreaseUISize()
	{
		if (cs.values.currentUIScale >= 0.5f)
			cs.values.currentUIScale -= 0.25f;
		syncCanvasScaler();
		ds.UIScale.text = cs.values.currentUIScale + "";
	}
	
	public void syncCanvasScaler()
	{
		foreach (GameObject scalerGo in f_canvasScaler)
			scalerGo.GetComponent<CanvasScaler>().scaleFactor = cs.values.currentUIScale;
	}

	public void setWallTransparency(int value)
	{
		if (ObstableTransparencySystem.instance != null)
			ObstableTransparencySystem.instance.Pause = value == 0;
		cs.values.currentWallTransparency = value;
	}

	public void setCameraTracking(int value)
	{
		cs.values.currentCameraTracking = value;
	}

	public void setGameView(int value)
	{
		if (CameraSystem.instance != null)
			CameraSystem.instance.setOrthographicView(value == 1);
		cs.values.currentGameView = value;
	}

	public void setTooltipView(int value)
	{
		cs.values.currentTooltipView = value;
	}

	public void syncFonts(int value)
	{
		cs.values.currentFont = value;
		foreach (GameObject go in f_modifiableFonts)
			syncFont(go);
	}

	private void syncFont(GameObject go)
	{
		TMP_InputField inputField = go.GetComponent<TMP_InputField>();
		if (inputField != null)
			inputField.fontAsset = ds.fonts[cs.values.currentFont];
		else
			go.GetComponent<TextMeshProUGUI>().font = ds.fonts[cs.values.currentFont];
	}

	// Fonction utilisée pour définir la font dans la liste déroulante de sélection de la font dans les paramètres
	private void fixFont(GameObject go)
	{
		TextMeshProUGUI option = go.GetComponent<TextMeshProUGUI>();
		switch (option.text)
		{
			case "Arial": option.font = ds.fonts[0];
				break;
			case "Comic Sans MS":
				option.font = ds.fonts[1];
				break;
			case "Liberation Sans SDF":
				option.font = ds.fonts[2];
				break;
			case "Luciole":
				option.font = ds.fonts[3];
				break;
			case "Open Dyslexic":
				option.font = ds.fonts[4];
				break;
			case "Orbitron":
				option.font = ds.fonts[5];
				break;
			case "Roboto":
				option.font = ds.fonts[6];
				break;
			case "Tahoma":
				option.font = ds.fonts[7];
				break;
			case "Verdana":
				option.font = ds.fonts[8];
				break;
		}
	}

	public void setCaretWidth(int value)
	{
		cs.values.currentCaretWidth = value;
		foreach (GameObject inputGO in f_inputfield)
			sync_Inputfield(inputGO);
	}

	public void setCaretHeight(int value)
	{
		cs.values.currentCaretHeight = value;
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
			currentColor.normalColor = cs.values.currentNormalColor_Text;
			currentColor.highlightedColor = cs.values.currentNormalColor_Text;
			currentColor.pressedColor = cs.values.currentNormalColor_Text;
			currentColor.selectedColor = cs.values.currentSelectedColor_Text;
			currentColor.disabledColor = cs.values.currentDisabledColor;
			textSel.colors = currentColor;

		}
		// Pour les textes qui sont enfant d'un LangOption => Mettre à jour la couleur du bouton et le LangOption
		else if (text.GetComponentInParent<LangOption>(true))
		{
			Button langBtn = text.GetComponentInParent<Button>(true);
			ColorBlock currentColor = langBtn.colors;
			currentColor.normalColor = cs.values.currentNormalColor_Text;
			langBtn.colors = currentColor;
			LangOption langOpt = text.GetComponentInParent<LangOption>(true);
			langOpt.on = cs.values.currentSelectedColor_Text;
			langOpt.off = cs.values.currentNormalColor_Text;
		}
		else
		{
			Selectable parentSel = text.GetComponentInParent<Selectable>(true);
			// Pour les textes qui sont enfant d'un Selectable et dont le Selectable contrôle la couleur du texte => Mettre à jour la couleur du Selectable
			if (parentSel != null && parentSel.targetGraphic == text.GetComponent<Graphic>())
			{
				ColorBlock currentColor = parentSel.colors;
				currentColor.normalColor = cs.values.currentNormalColor_Text;
				parentSel.colors = currentColor;
			}
			// Sinon tous les autres textes, on change simplement leur couleur sauf si on est sur un placeholder d'un input field (pour ce cas voir syncColor_Inputfield)
			else
			{
				TMP_InputField inputField = text.GetComponentInParent<TMP_InputField>(true);
				if (inputField == null || inputField.placeholder != text.GetComponent<Graphic>())
					syncGraphicColor(text, cs.values.currentNormalColor_Text);
			}
		}
	}

	private void syncColor_Dropdown(GameObject go, Color? unused = null)
	{
		syncNormalColor(go, cs.values.currentNormalColor_Dropdown);
		syncGraphicColor(go.transform.Find("Template").gameObject, cs.values.currentNormalColor_Dropdown);
	}

	private void sync_Inputfield(GameObject go, Color? unused = null)
	{
		TMP_InputField input = go.GetComponent<TMP_InputField>();
		ColorBlock currentColor = input.colors;
		currentColor.normalColor = cs.values.currentNormalColor_Inputfield;
		currentColor.pressedColor = cs.values.currentSelectedColor_Inputfield;
		input.colors = currentColor;
		input.placeholder.color = cs.values.currentPlaceholderColor;
		input.selectionColor = cs.values.currentSelectionColor_Inputfield;
		input.caretColor = cs.values.currentCaretColor_Inputfield;
		input.caretWidth = (cs.values.currentCaretWidth + 1) * 2;
	}

	private void sync_CaretHeight(GameObject go)
	{
		go.transform.localScale = new Vector3(1f, cs.values.currentCaretHeight + 1, 1f);
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
		currentColor.highlightedColor = cs.values.currentHighlightedColor;
		currentColor.pressedColor = cs.values.currentPressedColor;
		currentColor.selectedColor = cs.values.currentSelectedColor;
		currentColor.disabledColor = cs.values.currentDisabledColor;
		select.colors = currentColor;
	}

	private void syncColor_Panel(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, cs.values.currentColor_Panel1);
		Transform texture = go.transform.Find("Texture");
		if (texture != null)
			syncGraphicColor(texture.gameObject, cs.values.currentColor_PanelTexture);
	}

	public void setBorderTickness(int value)
	{
		cs.values.currentBorderThickness = value + 1;
		syncColor(f_borders, syncBorderProperties);
	}

	private void syncBorderProperties(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, cs.values.currentColor_Border);
		go.GetComponent<Image>().pixelsPerUnitMultiplier = 1f / cs.values.currentBorderThickness;
	}

	private void syncGraphicColor(GameObject go, Color? color)
	{
		go.GetComponent<Graphic>().color = color ?? Color.magenta;
	}

	private void syncIconColor(GameObject go, Color? color)
	{
		if (go.GetComponent<Selectable>() != null)
			syncNormalColor(go, color);
		else
			syncGraphicColor(go, color);
	}

	public void setCharSpacing(int value)
	{
		cs.values.currentCharSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setWordSpacing(int value)
	{
		cs.values.currentWordSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setLineSpacing(int value)
	{
		cs.values.currentLineSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setParagraphSpacing(int value)
	{
		cs.values.currentParagraphSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	private void syncSpacing_Text(GameObject textGO)
	{
		TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
		// Transformation des numéro d'item sélectionné en valeurs d'espacement
		text.characterSpacing = (cs.values.currentCharSpacing - 2) * 10;
		text.wordSpacing = (cs.values.currentWordSpacing - 2) * 10;
		text.lineSpacing = (cs.values.currentLineSpacing - 2) * 10;
		text.paragraphSpacing = (cs.values.currentParagraphSpacing - 2) * 10;
	}
	
	private void syncBlockColor(GameObject go, Color? unused = null)
	{
		switch (go.tag)
		{
			case "UI_Action":
				syncNormalColor(go, cs.values.currentActionBlockColor);
				break;
			case "UI_Control":
				syncNormalColor(go, cs.values.currentControlBlockColor);
				break;
			case "UI_Operator":
				syncNormalColor(go, cs.values.currentOperatorBlockColor);
				break;
			case "UI_Captor":
				syncNormalColor(go, cs.values.currentCaptorBlockColor);
				break;
		}
	}

	private void syncDropAreaColor(GameObject go, Color? unused = null)
	{
		if (go.GetComponent<DropZone>())
			syncGraphicColor(go.transform.Find("PositionBar").gameObject, cs.values.currentDropAreaColor);
        else
			go.GetComponentInChildren<Outline>().effectColor = cs.values.currentDropAreaColor;
	}

	private void setHighlightingColor(GameObject go, Color? unused = null)
	{
		if (go.GetComponent<Highlightable>())
		{
			go.GetComponent<Highlightable>().highlightedColor = cs.values.currentHighlightingColor;
			if (go.CompareTag("Player") || go.CompareTag("Drone"))
			{
				go.transform.Find("HaloSelection").GetComponent<Renderer>().material.color = cs.values.currentHighlightingColor;
				go.transform.Find("HaloSelection/ArrowPivot/Arrow").GetComponent<Renderer>().material.color = cs.values.currentHighlightingColor;
			}
		}
		else
			go.GetComponent<BasicAction>().highlightedColor = cs.values.currentHighlightingColor;
	}

	private void syncTileColor(GameObject go, Color? unused = null)
	{
		go.GetComponent<SpriteRenderer>().color = new Color(cs.values.currentHighlightingColor.r, cs.values.currentHighlightingColor.g, cs.values.currentHighlightingColor.b, cs.values.currentHighlightingColor.a * 0.6f);
	}

	private void syncConditionNotif(GameObject go, Color? unused = null)
	{
		if (go.name == "true")
			syncGraphicColor(go, cs.values.currentCaptorTrueColor);
		else
			syncGraphicColor(go, cs.values.currentCaptorFalseColor);
	}

	// See avatar selector in ConnexionScene and TitleScreen
	public void selectAvatar(Image src)
    {
		foreach (GameObject go in f_avatarTarget)
			go.GetComponent<Image>().sprite = src.sprite;
		userData.avatarSelected = src.transform.parent.GetSiblingIndex();
	}

	// see InputField in MiddleSetYear un ConnexionScene
	public void setBirthYear(string year)
    {
		userData.birthYear = year;
    }

	// see Toggle in MiddleSetYear un ConnexionScene
	public void setIsTeacher(bool state)
	{
		userData.isTeacher = state;
	}
}
