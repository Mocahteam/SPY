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


	public static SettingsManager instance;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern bool ClearPlayerPrefs(); // call javascript

	private DefaultSettingsValues dsf;
	private CurrentSettingsValues csf;
	public Transform settingsWindow;
	private Transform settingsContent;
	private FlexibleColorPicker flexibleColorPicker;
	public Selectable LoadingLogs;

	public SettingsManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		settingsContent = settingsWindow.Find("BackgroundPanel/Scroll View/Viewport/Content");
		flexibleColorPicker = settingsWindow.GetComponentInChildren<FlexibleColorPicker>(true);
		dsf = settingsWindow.GetComponent<DefaultSettingsValues>();
		csf = settingsWindow.GetComponent<CurrentSettingsValues>();

		if (Application.platform == RuntimePlatform.WebGLPlayer && ClearPlayerPrefs())
			PlayerPrefs.DeleteAll();

		loadPlayerPrefs();
		saveParameters();

		f_allTexts.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_allTexts.addEntryCallback(syncSpacing_Text);
		f_textsSelectable.addEntryCallback(delegate (GameObject go) { syncColor_Text(go); });
		f_modifiableFonts.addEntryCallback(syncFont);
		f_fixedFonts.addEntryCallback(fixFont);
		f_dropdown.addEntryCallback(delegate (GameObject go) { syncColor_Dropdown(go); });
		f_inputfield.addEntryCallback(delegate (GameObject go) { sync_Inputfield(go); });
		f_inputfieldCaret.addEntryCallback(sync_CaretHeight);
		f_buttons.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, csf.currentNormalColor_Button); });
		f_icons.addEntryCallback(delegate (GameObject go) { syncIconColor(go, csf.currentColor_Icon); });
		f_buttonsPlay.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, csf.currentPlayButtonColor); });
		f_buttonsPause.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, csf.currentPauseButtonColor); });
		f_buttonsStop.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, csf.currentStopButtonColor); });
		f_selectable.addEntryCallback(delegate (GameObject go) { syncHighlightedColor(go); });
		f_SyncSelectedColor.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, csf.currentSelectedColor); });
		f_panels1.addEntryCallback(delegate (GameObject go) { syncColor_Panel(go); });
		f_panels2.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, csf.currentColor_Panel2); });
		f_panels3.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, csf.currentColor_Panel3); });
		f_borders.addEntryCallback(delegate (GameObject go) { syncBorderProperties(go); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, csf.currentNormalColor_Scrollbar); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, csf.currentBackgroundColor_Scrollbar); });
		f_scrollview.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, csf.currentBackgroundColor_Scrollview); });
		f_toggle.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, csf.currentNormalColor_Toggle); });
		f_tooltip.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, csf.currentColor_Tooltip); });
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
		syncColors();
		foreach (GameObject caretGO in f_inputfieldCaret)
			sync_CaretHeight(caretGO);
		foreach (GameObject textGO in f_allTexts)
			syncSpacing_Text(textGO);
	}

	private void syncColors()
	{
		syncColor(f_allTexts, syncColor_Text);
		SyncLocalization.instance.syncLocale(); // because it change colors
		syncColor(f_dropdown, syncColor_Dropdown);
		syncColor(f_inputfield, sync_Inputfield);
		syncColor(f_buttons, syncNormalColor, csf.currentNormalColor_Button);
		syncColor(f_icons, syncIconColor, csf.currentColor_Icon);
		syncColor(f_buttonsPlay, syncNormalColor, csf.currentPlayButtonColor);
		syncColor(f_buttonsPause, syncNormalColor, csf.currentPauseButtonColor);
		syncColor(f_buttonsStop, syncNormalColor, csf.currentStopButtonColor);
		syncColor(f_selectable, syncHighlightedColor);
		syncColor(f_SyncSelectedColor, syncGraphicColor, csf.currentSelectedColor);
		syncColor(f_panels1, syncColor_Panel);
		syncColor(f_panels2, syncGraphicColor, csf.currentColor_Panel2);
		syncColor(f_panels3, syncGraphicColor, csf.currentColor_Panel3);
		syncColor(f_borders, syncBorderProperties);
		syncColor(f_scrollbar, syncNormalColor, csf.currentNormalColor_Scrollbar);
		syncColor(f_scrollbar, syncGraphicColor, csf.currentBackgroundColor_Scrollbar);
		syncColor(f_scrollview, syncGraphicColor, csf.currentBackgroundColor_Scrollview);
		syncColor(f_toggle, syncNormalColor, csf.currentNormalColor_Toggle);
		syncColor(f_tooltip, syncGraphicColor, csf.currentColor_Tooltip);
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
		csf.currentQuality = PlayerPrefs.GetInt("quality", dsf.defaultQuality);
		csf.currentInteractionMode = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : dsf.defaultInteractionMode);
		// définition de la taille de l'interface
		float uiWidth = f_canvasScaler.Count > 0 ? (f_canvasScaler.First().transform as RectTransform).rect.width : Screen.currentResolution.width;
		csf.currentUIScale = PlayerPrefs.GetFloat("UIScale", (float)Math.Max(dsf.defaultUIScale, Math.Round(uiWidth / 1280, 2))*(Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 2 : 1)); // do not reduce scale under defaultUIScale and multiply scale for definition higher than 1280. Sur mobile on multiplie l'echelle par deux
		foreach (GameObject scalerGo in f_canvasScaler)
			scalerGo.GetComponent<CanvasScaler>().scaleFactor = csf.currentUIScale;
		csf.currentWallTransparency = PlayerPrefs.GetInt("wallTransparency", dsf.defaultWallTransparency);
		csf.currentCameraTracking = PlayerPrefs.GetInt("cameraTracking", dsf.defaultCameraTracking);
		csf.currentGameView = PlayerPrefs.GetInt("orthographicView", dsf.defaultGameView);
		csf.currentTooltipView = PlayerPrefs.GetInt("tooltipView", dsf.defaultTooltipView);

		csf.currentFont = PlayerPrefs.GetInt("font", dsf.defaultFont);
		csf.currentCaretWidth = PlayerPrefs.GetInt("caretWidth", dsf.defaultCaretWidth);
		csf.currentCaretHeight = PlayerPrefs.GetInt("caretHeight", dsf.defaultCaretHeight);
		csf.currentCharSpacing = PlayerPrefs.GetInt("charSpacing", dsf.defaultCharSpacing);
		csf.currentWordSpacing = PlayerPrefs.GetInt("wordSpacing", dsf.defaultWordSpacing);
		csf.currentLineSpacing = PlayerPrefs.GetInt("lineSpacing", dsf.defaultLineSpacing);
		csf.currentParagraphSpacing = PlayerPrefs.GetInt("paragraphSpacing", dsf.defaultParagraphSpacing);

		// Synchronisation de la couleur des textes
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.defaultNormalColor_Text)), out csf.currentNormalColor_Text);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(dsf.defaultSelectedColor_Text)), out csf.currentSelectedColor_Text);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PlaceholderColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultPlaceholderColor)), out csf.currentPlaceholderColor);
		// Synchronisation de la couleur des dropdown
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("DropdownColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.defaultNormalColor_Dropdown)), out csf.currentNormalColor_Dropdown);
		// Synchronisation de la couleur des inputfield
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.defaultNormalColor_Inputfield)), out csf.currentNormalColor_Inputfield);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorSelected", ColorUtility.ToHtmlStringRGBA(dsf.defaultSelectedColor_Inputfield)), out csf.currentSelectedColor_Inputfield);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorSelection", ColorUtility.ToHtmlStringRGBA(dsf.defaultSelectionColor_Inputfield)), out csf.currentSelectionColor_Inputfield);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("InputfieldColorCaret", ColorUtility.ToHtmlStringRGBA(dsf.defaultCaretColor_Inputfield)), out csf.currentCaretColor_Inputfield);
		// Synchronisation de la couleur des bouttons
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.defaultNormalColor_Button)), out csf.currentNormalColor_Button);
		// Synchronisation de la couleur des highlighted
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultHighlightedColor)), out csf.currentHighlightedColor);
		// Synchronisation de la couleur des pressed
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PressedColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultPressedColor)), out csf.currentPressedColor);
		// Synchronisation de la couleur des selected
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultSelectedColor)), out csf.currentSelectedColor);
		// Synchronisation de la couleur des disabled
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultDisabledColor)), out csf.currentDisabledColor);
		// Synchronisation de la couleur des bouttons icônes
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("IconColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultColor_Icon)), out csf.currentColor_Icon);
		// Synchronisation de la couleur des panels
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("Panel1Color", ColorUtility.ToHtmlStringRGBA(dsf.defaultColor_Panel1)), out csf.currentColor_Panel1);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("Panel2Color", ColorUtility.ToHtmlStringRGBA(dsf.defaultColor_Panel2)), out csf.currentColor_Panel2);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("Panel3Color", ColorUtility.ToHtmlStringRGBA(dsf.defaultColor_Panel3)), out csf.currentColor_Panel3);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(dsf.defaultColor_PanelTexture)), out csf.currentColor_PanelTexture);
		// Synchronisation des propriétés de bordure
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("BorderColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultColor_Border)), out csf.currentColor_Border);
		csf.currentBorderThickness = PlayerPrefs.GetInt("BorderThickness", dsf.defaultBorderThickness);
		// Synchronisation de la couleur des scrollbars
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.defaultNormalColor_Scrollbar)), out csf.currentNormalColor_Scrollbar);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(dsf.defaultBackgroundColor_Scrollbar)), out csf.currentBackgroundColor_Scrollbar);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultBackgroundColor_Scrollview)), out csf.currentBackgroundColor_Scrollview);
		// Synchronisation de la couleur des toggles
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.defaultNormalColor_Toggle)), out csf.currentNormalColor_Toggle);
		// Synchronisation de la couleur des tooltip
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultColor_Tooltip)), out csf.currentColor_Tooltip);

		// Synchronisation des couleurs du player
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PlayColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultPlayButtonColor)), out csf.currentPlayButtonColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("PauseColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultPauseButtonColor)), out csf.currentPauseButtonColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("StopColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultStopButtonColor)), out csf.currentStopButtonColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ActionBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultActionBlockColor)), out csf.currentActionBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("ControlBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultControlBlockColor)), out csf.currentControlBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("OperatorBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultOperatorBlockColor)), out csf.currentOperatorBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("CaptorBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultCaptorBlockColor)), out csf.currentCaptorBlockColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("DropAreaColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultDropAreaColor)), out csf.currentDropAreaColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("HighlightingColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultHighlightingColor)), out csf.currentHighlightingColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("CaptorTrueColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultCaptorTrueColor)), out csf.currentCaptorTrueColor);
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("CaptorFalseColor", ColorUtility.ToHtmlStringRGBA(dsf.defaultCaptorFalseColor)), out csf.currentCaptorFalseColor);
	}

	// initialise l'UI des settings à partir des currentSettingsValues
	private void syncSettingsUI()
	{
		// Voir SyncLocalization pour la gestion de la langue
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Quality").GetComponentInChildren<TMP_Dropdown>().value = csf.currentQuality;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = csf.currentInteractionMode;
		dsf.UIScale.text = csf.currentUIScale + "";
		settingsContent.Find("SectionGraphic/GridContainer/Grid/WallTransparency").GetComponentInChildren<TMP_Dropdown>().value = csf.currentWallTransparency;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/CameraTracking").GetComponentInChildren<TMP_Dropdown>().value = csf.currentCameraTracking;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/GameView").GetComponentInChildren<TMP_Dropdown>().value = csf.currentGameView;
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Tooltip").GetComponentInChildren<TMP_Dropdown>().value = csf.currentTooltipView;

		settingsContent.Find("SectionText/GridContainer/Grid/FontDropdown").GetComponentInChildren<TMP_Dropdown>().value = csf.currentFont;
		settingsContent.Find("SectionText/GridContainer/Grid/CaretWidth").GetComponentInChildren<TMP_Dropdown>().value = csf.currentCaretWidth;
		settingsContent.Find("SectionText/GridContainer/Grid/CaretHeight").GetComponentInChildren<TMP_Dropdown>().value = csf.currentCaretHeight;
		settingsContent.Find("SectionText/GridContainer/Grid/CharSpacing").GetComponentInChildren<TMP_Dropdown>().value = csf.currentCharSpacing;
		settingsContent.Find("SectionText/GridContainer/Grid/WordSpacing").GetComponentInChildren<TMP_Dropdown>().value = csf.currentWordSpacing;
		settingsContent.Find("SectionText/GridContainer/Grid/LineSpacing").GetComponentInChildren<TMP_Dropdown>().value = csf.currentLineSpacing;
		settingsContent.Find("SectionText/GridContainer/Grid/ParagraphSpacing").GetComponentInChildren<TMP_Dropdown>().value = csf.currentParagraphSpacing;

		// Synchronisation de la couleur des textes
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorTextNormal/ButtonWithBorder").GetComponent<Image>().color = csf.currentNormalColor_Text;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorTextSelected/ButtonWithBorder").GetComponent<Image>().color = csf.currentSelectedColor_Text;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPlaceholder/ButtonWithBorder").GetComponent<Image>().color = csf.currentPlaceholderColor;
		// Synchronisation de la couleur des dropdown
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorDropdownNormal/ButtonWithBorder").GetComponent<Image>().color = csf.currentNormalColor_Dropdown;
		// Synchronisation de la couleur des inputfield
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldNormal/ButtonWithBorder").GetComponent<Image>().color = csf.currentNormalColor_Inputfield;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldSelected/ButtonWithBorder").GetComponent<Image>().color = csf.currentSelectedColor_Inputfield;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldSelection/ButtonWithBorder").GetComponent<Image>().color = csf.currentSelectionColor_Inputfield;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorInputfieldCaret/ButtonWithBorder").GetComponent<Image>().color = csf.currentCaretColor_Inputfield;
		// Synchronisation de la couleur des bouttons
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorButtonNormal/ButtonWithBorder").GetComponent<Image>().color = csf.currentNormalColor_Button;
		// Synchronisation de la couleur des highlighted
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorHighlighted/ButtonWithBorder").GetComponent<Image>().color = csf.currentHighlightedColor;
		// Synchronisation de la couleur des pressed
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPressed/ButtonWithBorder").GetComponent<Image>().color = csf.currentPressedColor;
		// Synchronisation de la couleur des selected
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorSelected/ButtonWithBorder").GetComponent<Image>().color = csf.currentSelectedColor;
		// Synchronisation de la couleur des disabled
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorDisabled/ButtonWithBorder").GetComponent<Image>().color = csf.currentDisabledColor;
		// Synchronisation de la couleur des bouttons icônes
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorIcon/ButtonWithBorder").GetComponent<Image>().color = csf.currentColor_Icon;
		// Synchronisation de la couleur des panels
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanel1/ButtonWithBorder").GetComponent<Image>().color = csf.currentColor_Panel1;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanel2/ButtonWithBorder").GetComponent<Image>().color = csf.currentColor_Panel2;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanel3/ButtonWithBorder").GetComponent<Image>().color = csf.currentColor_Panel3;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorPanelTexture/ButtonWithBorder").GetComponent<Image>().color = csf.currentColor_PanelTexture;
		// Synchronisation des propriétés de bordure
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorBorder/ButtonWithBorder").GetComponent<Image>().color = csf.currentColor_Border;
		settingsContent.Find("SectionColor/GridContainer/Grid/ThicknessBorder").GetComponentInChildren<TMP_Dropdown>().value = csf.currentBorderThickness - 1;
		// Synchronisation de la couleur des scrollbars
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorScrollbarNormal/ButtonWithBorder").GetComponent<Image>().color = csf.currentNormalColor_Scrollbar;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorScrollbarBackground/ButtonWithBorder").GetComponent<Image>().color = csf.currentBackgroundColor_Scrollbar;
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorScrollview/ButtonWithBorder").GetComponent<Image>().color = csf.currentBackgroundColor_Scrollview;
		// Synchronisation de la couleur des toggles
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorToggleNormal/ButtonWithBorder").GetComponent<Image>().color = csf.currentNormalColor_Toggle;
		// Synchronisation de la couleur des tooltip
		settingsContent.Find("SectionColor/GridContainer/Grid/ColorTooltip/ButtonWithBorder").GetComponent<Image>().color = csf.currentColor_Tooltip;

		// Synchronisation des couleurs du player
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorPlayButton/ButtonWithBorder").GetComponent<Image>().color = csf.currentPlayButtonColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorPauseButton/ButtonWithBorder").GetComponent<Image>().color = csf.currentPauseButtonColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorStopButton/ButtonWithBorder").GetComponent<Image>().color = csf.currentStopButtonColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorActionBlock/ButtonWithBorder").GetComponent<Image>().color = csf.currentActionBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorControlBlock/ButtonWithBorder").GetComponent<Image>().color = csf.currentControlBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorOperatorBlock/ButtonWithBorder").GetComponent<Image>().color = csf.currentOperatorBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorCaptorBlock/ButtonWithBorder").GetComponent<Image>().color = csf.currentCaptorBlockColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorDropArea/ButtonWithBorder").GetComponent<Image>().color = csf.currentDropAreaColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorHighlighting/ButtonWithBorder").GetComponent<Image>().color = csf.currentHighlightingColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorCaptorTrue/ButtonWithBorder").GetComponent<Image>().color = csf.currentCaptorTrueColor;
		settingsContent.Find("PlayerColors/GridContainer/Grid/ColorCaptorFalse/ButtonWithBorder").GetComponent<Image>().color = csf.currentCaptorFalseColor;
	}

	public void saveParameters()
	{
		// Voir SyncLocalization pour la gestion de la langue
		PlayerPrefs.SetInt("quality", csf.currentQuality);
		PlayerPrefs.SetInt("interaction", csf.currentInteractionMode);
		PlayerPrefs.SetFloat("UIScale", csf.currentUIScale);
		PlayerPrefs.SetInt("wallTransparency", csf.currentWallTransparency);
		PlayerPrefs.SetInt("cameraTracking", csf.currentCameraTracking);
		PlayerPrefs.SetInt("orthographicView", csf.currentGameView);
		PlayerPrefs.SetInt("tooltipView", csf.currentTooltipView);
		PlayerPrefs.SetInt("font", csf.currentFont);
		PlayerPrefs.SetInt("caretWidth", csf.currentCaretWidth);
		PlayerPrefs.SetInt("caretHeight", csf.currentCaretHeight);
		PlayerPrefs.SetInt("charSpacing", csf.currentCharSpacing);
		PlayerPrefs.SetInt("wordSpacing", csf.currentWordSpacing);
		PlayerPrefs.SetInt("lineSpacing", csf.currentLineSpacing);
		PlayerPrefs.SetInt("paragraphSpacing", csf.currentParagraphSpacing);
		PlayerPrefs.SetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(csf.currentNormalColor_Text));
		PlayerPrefs.SetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(csf.currentSelectedColor_Text));
		PlayerPrefs.SetString("PlaceholderColor", ColorUtility.ToHtmlStringRGBA(csf.currentPlaceholderColor));
		PlayerPrefs.SetString("DropdownColorNormal", ColorUtility.ToHtmlStringRGBA(csf.currentNormalColor_Dropdown));
		PlayerPrefs.SetString("InputfieldColorNormal", ColorUtility.ToHtmlStringRGBA(csf.currentNormalColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelected", ColorUtility.ToHtmlStringRGBA(csf.currentSelectedColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelection", ColorUtility.ToHtmlStringRGBA(csf.currentSelectionColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorCaret", ColorUtility.ToHtmlStringRGBA(csf.currentCaretColor_Inputfield));
		PlayerPrefs.SetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(csf.currentNormalColor_Button));
		PlayerPrefs.SetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(csf.currentHighlightedColor));
		PlayerPrefs.SetString("PressedColor", ColorUtility.ToHtmlStringRGBA(csf.currentPressedColor));
		PlayerPrefs.SetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(csf.currentSelectedColor));
		PlayerPrefs.SetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(csf.currentDisabledColor));
		PlayerPrefs.SetString("IconColor", ColorUtility.ToHtmlStringRGBA(csf.currentColor_Icon));
		PlayerPrefs.SetString("Panel1Color", ColorUtility.ToHtmlStringRGBA(csf.currentColor_Panel1));
		PlayerPrefs.SetString("Panel2Color", ColorUtility.ToHtmlStringRGBA(csf.currentColor_Panel2));
		PlayerPrefs.SetString("Panel3Color", ColorUtility.ToHtmlStringRGBA(csf.currentColor_Panel3));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(csf.currentColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(csf.currentColor_Border));
		PlayerPrefs.SetInt("BorderThickness", csf.currentBorderThickness);
		PlayerPrefs.SetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(csf.currentNormalColor_Scrollbar));
		PlayerPrefs.SetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(csf.currentBackgroundColor_Scrollbar));
		PlayerPrefs.SetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(csf.currentBackgroundColor_Scrollview));
		PlayerPrefs.SetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(csf.currentNormalColor_Toggle));
		PlayerPrefs.SetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(csf.currentColor_Tooltip));
		PlayerPrefs.SetString("PlayColor", ColorUtility.ToHtmlStringRGBA(csf.currentPlayButtonColor));
		PlayerPrefs.SetString("PauseColor", ColorUtility.ToHtmlStringRGBA(csf.currentPauseButtonColor));
		PlayerPrefs.SetString("StopColor", ColorUtility.ToHtmlStringRGBA(csf.currentStopButtonColor));
		PlayerPrefs.SetString("ActionBlockColor", ColorUtility.ToHtmlStringRGBA(csf.currentActionBlockColor));
		PlayerPrefs.SetString("ControlBlockColor", ColorUtility.ToHtmlStringRGBA(csf.currentControlBlockColor));
		PlayerPrefs.SetString("OperatorBlockColor", ColorUtility.ToHtmlStringRGBA(csf.currentOperatorBlockColor));
		PlayerPrefs.SetString("CaptorBlockColor", ColorUtility.ToHtmlStringRGBA(csf.currentCaptorBlockColor));
		PlayerPrefs.SetString("DropAreaColor", ColorUtility.ToHtmlStringRGBA(csf.currentDropAreaColor));
		PlayerPrefs.SetString("HighlightingColor", ColorUtility.ToHtmlStringRGBA(csf.currentHighlightingColor));
		PlayerPrefs.SetString("CaptorTrueColor", ColorUtility.ToHtmlStringRGBA(csf.currentCaptorTrueColor));
		PlayerPrefs.SetString("CaptorFalseColor", ColorUtility.ToHtmlStringRGBA(csf.currentCaptorFalseColor));
		PlayerPrefs.Save();
	}

	private string settingsValuesToJson()
    {
		return JsonUtility.ToJson(csf);
	}

	private void jsonToSettingsValues(string json)
    {
		DefaultSettingsValues test = JsonUtility.FromJson<DefaultSettingsValues>(json);
	}

	public void resetParameters()
	{
		// Volontairement on ne reset pas la langue
		csf.currentQuality = dsf.defaultQuality;
		csf.currentInteractionMode = dsf.defaultInteractionMode;
		float uiWidth = f_canvasScaler.Count > 0 ? (f_canvasScaler.First().transform as RectTransform).rect.width : Screen.currentResolution.width;
		csf.currentUIScale = (float)Math.Max(dsf.defaultUIScale, Math.Round(uiWidth / 1280, 2));
		csf.currentWallTransparency = dsf.defaultWallTransparency;
		csf.currentCameraTracking = dsf.defaultCameraTracking;
		csf.currentGameView = dsf.defaultGameView;
		csf.currentTooltipView = dsf.defaultTooltipView;
		csf.currentFont = dsf.defaultFont;
		csf.currentCaretWidth = dsf.defaultCaretWidth;
		csf.currentCaretHeight = dsf.defaultCaretHeight;
		csf.currentCharSpacing = dsf.defaultCharSpacing;
		csf.currentWordSpacing = dsf.defaultWordSpacing;
		csf.currentLineSpacing = dsf.defaultLineSpacing;
		csf.currentParagraphSpacing = dsf.defaultParagraphSpacing;
		csf.currentNormalColor_Text = dsf.defaultNormalColor_Text;
		csf.currentSelectedColor_Text = dsf.defaultSelectedColor_Text;
		csf.currentPlaceholderColor = dsf.defaultPlaceholderColor;
		csf.currentNormalColor_Dropdown = dsf.defaultNormalColor_Dropdown;
		csf.currentNormalColor_Inputfield = dsf.defaultNormalColor_Inputfield;
		csf.currentSelectedColor_Inputfield = dsf.defaultSelectedColor_Inputfield;
		csf.currentSelectionColor_Inputfield = dsf.defaultSelectionColor_Inputfield;
		csf.currentCaretColor_Inputfield = dsf.defaultCaretColor_Inputfield;
		csf.currentNormalColor_Button = dsf.defaultNormalColor_Button;
		csf.currentHighlightedColor = dsf.defaultHighlightedColor;
		csf.currentPressedColor = dsf.defaultPressedColor;
		csf.currentSelectedColor = dsf.defaultSelectedColor;
		csf.currentDisabledColor = dsf.defaultDisabledColor;
		csf.currentColor_Icon = dsf.defaultColor_Icon;
		csf.currentColor_Panel1 = dsf.defaultColor_Panel1;
		csf.currentColor_Panel2 = dsf.defaultColor_Panel2;
		csf.currentColor_Panel3 = dsf.defaultColor_Panel3;
		csf.currentColor_PanelTexture = dsf.defaultColor_PanelTexture;
		csf.currentColor_Border = dsf.defaultColor_Border;
		csf.currentBorderThickness = dsf.defaultBorderThickness;
		csf.currentNormalColor_Scrollbar = dsf.defaultNormalColor_Scrollbar;
		csf.currentBackgroundColor_Scrollbar = dsf.defaultBackgroundColor_Scrollbar;
		csf.currentBackgroundColor_Scrollview = dsf.defaultBackgroundColor_Scrollview;
		csf.currentNormalColor_Toggle = dsf.defaultNormalColor_Toggle;
		csf.currentColor_Tooltip = dsf.defaultColor_Tooltip;
		csf.currentPlayButtonColor = dsf.defaultPlayButtonColor;
		csf.currentPauseButtonColor = dsf.defaultPauseButtonColor;
		csf.currentStopButtonColor = dsf.defaultStopButtonColor;
		csf.currentActionBlockColor = dsf.defaultActionBlockColor;
		csf.currentControlBlockColor = dsf.defaultControlBlockColor;
		csf.currentOperatorBlockColor = dsf.defaultOperatorBlockColor;
		csf.currentCaptorBlockColor = dsf.defaultCaptorBlockColor;
		csf.currentDropAreaColor = dsf.defaultDropAreaColor;
		csf.currentHighlightingColor = dsf.defaultHighlightingColor;
		csf.currentCaptorTrueColor = dsf.defaultCaptorTrueColor;
		csf.currentCaptorFalseColor = dsf.defaultCaptorFalseColor;

		saveParameters();
		syncSettingsUI();

		syncColors();
	}

	public void hookListener(string key)
	{
		flexibleColorPicker.onColorChange.RemoveAllListeners();
		switch (key)
		{
			case "TextColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentNormalColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "TextColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentSelectedColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "PlaceholderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentPlaceholderColor = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "DropdownColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentNormalColor_Dropdown = c;
					syncColor(f_dropdown, syncColor_Dropdown);
				});
				break;
			case "InputfieldColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentNormalColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentSelectedColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelection":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentSelectionColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorCaret":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentCaretColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "ButtonColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentNormalColor_Button = c;
					syncColor(f_buttons, syncNormalColor, csf.currentNormalColor_Button);
				});
				break;
			case "HighlightedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentHighlightedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "PressedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentPressedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "SelectedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentSelectedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
					syncColor(f_SyncSelectedColor, syncGraphicColor, csf.currentSelectedColor);
				});
				break;
			case "DisabledColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentDisabledColor = c;
					syncColor(f_allTexts, syncColor_Text);
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "IconColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentColor_Icon = c;
					syncColor(f_icons, syncIconColor, csf.currentColor_Icon);
				});
				break;
			case "Panel1Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentColor_Panel1 = c;
					syncColor(f_panels1, syncColor_Panel);
				});
				break;
			case "Panel2Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentColor_Panel2 = c;
					syncColor(f_panels2, syncGraphicColor, csf.currentColor_Panel2);
				});
				break;
			case "Panel3Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentColor_Panel3 = c;
					syncColor(f_panels3, syncGraphicColor, csf.currentColor_Panel3);
				});
				break;
			case "PanelColorTexture":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentColor_PanelTexture = c;
					syncColor(f_panels1, syncColor_Panel);
				});
				break;
			case "BorderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentColor_Border = c;
					syncColor(f_borders, syncBorderProperties, csf.currentColor_Border);
				});
				break;
			case "ScrollbarColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentNormalColor_Scrollbar = c;
					syncColor(f_scrollbar, syncNormalColor, csf.currentNormalColor_Scrollbar);
				});
				break;
			case "ScrollbarColorBackground":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentBackgroundColor_Scrollbar = c;
					syncColor(f_scrollbar, syncGraphicColor, csf.currentBackgroundColor_Scrollbar);
				});
				break;
			case "ScrollviewColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentBackgroundColor_Scrollview = c;
					syncColor(f_scrollview, syncGraphicColor, csf.currentBackgroundColor_Scrollview);
				});
				break;
			case "ToggleColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentNormalColor_Toggle = c;
					syncColor(f_toggle, syncNormalColor, csf.currentNormalColor_Toggle);
				});
				break;
			case "TooltipColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentColor_Tooltip = c;
					syncColor(f_tooltip, syncGraphicColor, csf.currentColor_Tooltip);
				});
				break;
			case "PlayColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentPlayButtonColor = c;
					syncColor(f_buttonsPlay, syncNormalColor, csf.currentPlayButtonColor);
				});
				break;
			case "PauseColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentPauseButtonColor = c;
					syncColor(f_buttonsPause, syncNormalColor, csf.currentPauseButtonColor);
				});
				break;
			case "StopColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentStopButtonColor = c;
					syncColor(f_buttonsStop, syncNormalColor, csf.currentStopButtonColor);
				});
				break;
			case "ActionBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentActionBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "ControlBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentControlBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "OperatorBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentOperatorBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "CaptorBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentCaptorBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "DropAreaColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentDropAreaColor = c;
					syncColor(f_dropArea, syncDropAreaColor);
				});
				break; 
			case "HighlightingColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentHighlightingColor = c;
					syncColor(f_highlightable, setHighlightingColor);
					syncColor(f_tileSelection, syncTileColor);
				});
				break;
			case "CaptorTrueColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentCaptorTrueColor = c;
					syncColor(f_conditionNotif, syncConditionNotif);
				});
				break;
			case "CaptorFalseColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					csf.currentCaptorFalseColor = c;
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
		csf.currentQuality = value;
	}

	public void setInteraction(int value)
	{
		csf.currentInteractionMode = value;
	}

	public void increaseUISize()
	{
		csf.currentUIScale += 0.25f;
		foreach (GameObject scalerGo in f_canvasScaler)
			scalerGo.GetComponent<CanvasScaler>().scaleFactor = csf.currentUIScale;
		dsf.UIScale.text = csf.currentUIScale + "";
	}
	public void decreaseUISize()
	{
		if (csf.currentUIScale >= 0.5f)
			csf.currentUIScale -= 0.25f;
		foreach (GameObject scalerGo in f_canvasScaler)
			scalerGo.GetComponent<CanvasScaler>().scaleFactor = csf.currentUIScale;
		dsf.UIScale.text = csf.currentUIScale + "";
	}

	public void setWallTransparency(int value)
	{
		if (ObstableTransparencySystem.instance != null)
			ObstableTransparencySystem.instance.Pause = value == 0;
		csf.currentWallTransparency = value;
	}

	public void setCameraTracking(int value)
	{
		csf.currentCameraTracking = value;
	}

	public void setGameView(int value)
	{
		if (CameraSystem.instance != null)
			CameraSystem.instance.setOrthographicView(value == 1);
		csf.currentGameView = value;
	}

	public void setTooltipView(int value)
	{
		csf.currentTooltipView = value;
	}

	public void syncFonts(int value)
	{
		csf.currentFont = value;
		foreach (GameObject go in f_modifiableFonts)
			syncFont(go);
	}

	private void syncFont(GameObject go)
	{
		TMP_InputField inputField = go.GetComponent<TMP_InputField>();
		if (inputField != null)
			inputField.fontAsset = dsf.fonts[csf.currentFont];
		else
			go.GetComponent<TextMeshProUGUI>().font = dsf.fonts[csf.currentFont];
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
		csf.currentCaretWidth = value;
		foreach (GameObject inputGO in f_inputfield)
			sync_Inputfield(inputGO);
	}

	public void setCaretHeight(int value)
	{
		csf.currentCaretHeight = value;
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
			currentColor.normalColor = csf.currentNormalColor_Text;
			currentColor.highlightedColor = csf.currentNormalColor_Text;
			currentColor.pressedColor = csf.currentNormalColor_Text;
			currentColor.selectedColor = csf.currentSelectedColor_Text;
			currentColor.disabledColor = csf.currentDisabledColor;
			textSel.colors = currentColor;

		}
		// Pour les textes qui sont enfant d'un LangOption => Mettre à jour la couleur du bouton et le LangOption
		else if (text.GetComponentInParent<LangOption>(true))
		{
			Button langBtn = text.GetComponentInParent<Button>(true);
			ColorBlock currentColor = langBtn.colors;
			currentColor.normalColor = csf.currentNormalColor_Text;
			langBtn.colors = currentColor;
			LangOption langOpt = text.GetComponentInParent<LangOption>(true);
			langOpt.on = csf.currentSelectedColor_Text;
			langOpt.off = csf.currentNormalColor_Text;
		}
		else
		{
			Selectable parentSel = text.GetComponentInParent<Selectable>(true);
			// Pour les textes qui sont enfant d'un Selectable et dont le Selectable contrôle la couleur du texte => Mettre à jour la couleur du Selectable
			if (parentSel != null && parentSel.targetGraphic == text.GetComponent<Graphic>())
			{
				ColorBlock currentColor = parentSel.colors;
				currentColor.normalColor = csf.currentNormalColor_Text;
				parentSel.colors = currentColor;
			}
			// Sinon tous les autres textes, on change simplement leur couleur sauf si on est sur un placeholder d'un input field (pour ce cas voir syncColor_Inputfield)
			else
			{
				TMP_InputField inputField = text.GetComponentInParent<TMP_InputField>(true);
				if (inputField == null || inputField.placeholder != text.GetComponent<Graphic>())
					syncGraphicColor(text, csf.currentNormalColor_Text);
			}
		}
	}

	private void syncColor_Dropdown(GameObject go, Color? unused = null)
	{
		syncNormalColor(go, csf.currentNormalColor_Dropdown);
		syncGraphicColor(go.transform.Find("Template").gameObject, csf.currentNormalColor_Dropdown);
	}

	private void sync_Inputfield(GameObject go, Color? unused = null)
	{
		TMP_InputField input = go.GetComponent<TMP_InputField>();
		ColorBlock currentColor = input.colors;
		currentColor.normalColor = csf.currentNormalColor_Inputfield;
		currentColor.pressedColor = csf.currentSelectedColor_Inputfield;
		input.colors = currentColor;
		input.placeholder.color = csf.currentPlaceholderColor;
		input.selectionColor = csf.currentSelectionColor_Inputfield;
		input.caretColor = csf.currentCaretColor_Inputfield;
		input.caretWidth = (csf.currentCaretWidth + 1) * 2;
	}

	private void sync_CaretHeight(GameObject go)
	{
		go.transform.localScale = new Vector3(1f, csf.currentCaretHeight + 1, 1f);
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
		currentColor.highlightedColor = csf.currentHighlightedColor;
		currentColor.pressedColor = csf.currentPressedColor;
		currentColor.selectedColor = csf.currentSelectedColor;
		currentColor.disabledColor = csf.currentDisabledColor;
		select.colors = currentColor;
	}

	private void syncColor_Panel(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, csf.currentColor_Panel1);
		Transform texture = go.transform.Find("Texture");
		if (texture != null)
			syncGraphicColor(texture.gameObject, csf.currentColor_PanelTexture);
	}

	public void setBorderTickness(int value)
	{
		csf.currentBorderThickness = value + 1;
		syncColor(f_borders, syncBorderProperties);
	}

	private void syncBorderProperties(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, csf.currentColor_Border);
		go.GetComponent<Image>().pixelsPerUnitMultiplier = 1f / csf.currentBorderThickness;
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
		csf.currentCharSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setWordSpacing(int value)
	{
		csf.currentWordSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setLineSpacing(int value)
	{
		csf.currentLineSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setParagraphSpacing(int value)
	{
		csf.currentParagraphSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	private void syncSpacing_Text(GameObject textGO)
	{
		TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
		// Transformation des numéro d'item sélectionné en valeurs d'espacement
		text.characterSpacing = (csf.currentCharSpacing - 2) * 10;
		text.wordSpacing = (csf.currentWordSpacing - 2) * 10;
		text.lineSpacing = (csf.currentLineSpacing - 2) * 10;
		text.paragraphSpacing = (csf.currentParagraphSpacing - 2) * 10;
	}
	
	private void syncBlockColor(GameObject go, Color? unused = null)
	{
		switch (go.tag)
		{
			case "UI_Action":
				syncNormalColor(go, csf.currentActionBlockColor);
				break;
			case "UI_Control":
				syncNormalColor(go, csf.currentControlBlockColor);
				break;
			case "UI_Operator":
				syncNormalColor(go, csf.currentOperatorBlockColor);
				break;
			case "UI_Captor":
				syncNormalColor(go, csf.currentCaptorBlockColor);
				break;
		}
	}

	private void syncDropAreaColor(GameObject go, Color? unused = null)
	{
		if (go.GetComponent<DropZone>())
			syncGraphicColor(go.transform.Find("PositionBar").gameObject, csf.currentDropAreaColor);
        else
			go.GetComponentInChildren<Outline>().effectColor = csf.currentDropAreaColor;
	}

	private void setHighlightingColor(GameObject go, Color? unused = null)
	{
		if (go.GetComponent<Highlightable>())
		{
			go.GetComponent<Highlightable>().highlightedColor = csf.currentHighlightingColor;
			if (go.CompareTag("Player") || go.CompareTag("Drone"))
			{
				go.transform.Find("HaloSelection").GetComponent<Renderer>().material.color = csf.currentHighlightingColor;
				go.transform.Find("HaloSelection/ArrowPivot/Arrow").GetComponent<Renderer>().material.color = csf.currentHighlightingColor;
			}
		}
		else
			go.GetComponent<BasicAction>().highlightedColor = csf.currentHighlightingColor;
	}

	private void syncTileColor(GameObject go, Color? unused = null)
	{
		go.GetComponent<SpriteRenderer>().color = new Color(csf.currentHighlightingColor.r, csf.currentHighlightingColor.g, csf.currentHighlightingColor.b, csf.currentHighlightingColor.a * 0.6f);
	}

	private void syncConditionNotif(GameObject go, Color? unused = null)
	{
		if (go.name == "true")
			syncGraphicColor(go, csf.currentCaptorTrueColor);
		else
			syncGraphicColor(go, csf.currentCaptorFalseColor);
	}


}
