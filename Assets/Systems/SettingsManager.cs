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
	private Family f_icons = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(Image)), new AnyOfTags("UI_Icon"));
	private Family f_buttonsPlay = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("PlayButton"));
	private Family f_buttonsPause = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("PauseButton"));
	private Family f_buttonsStop = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("StopButton"));
	private Family f_selectable = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)));
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
	private Family f_conditionNotif = FamilyManager.getFamily(new AnyOfComponents(typeof(Image)), new AnyOfTags("ConditionNotif"));


	public static SettingsManager instance;

	[DllImport("__Internal")]
	private static extern bool IsMobileBrowser(); // call javascript

	[DllImport("__Internal")]
	private static extern bool ClearPlayerPrefs(); // call javascript

	private DefaultSettingsValues dsf;
	public Transform settingsWindow;
	private Transform settingsContent;
	private FlexibleColorPicker flexibleColorPicker;
	public CanvasScaler[] canvasScaler;
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
		f_buttons.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, dsf.currentNormalColor_Button); });
		f_icons.addEntryCallback(delegate (GameObject go) { syncIconColor(go, dsf.currentColor_Icon); });
		f_buttonsPlay.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, dsf.currentPlayButtonColor); });
		f_buttonsPause.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, dsf.currentPauseButtonColor); });
		f_buttonsStop.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, dsf.currentStopButtonColor); });
		f_selectable.addEntryCallback(delegate (GameObject go) { syncHighlightedColor(go); });
		f_SyncSelectedColor.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, dsf.currentSelectedColor); });
		f_panels1.addEntryCallback(delegate (GameObject go) { syncColor_Panel(go); });
		f_panels2.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, dsf.currentColor_Panel2); });
		f_panels3.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, dsf.currentColor_Panel3); });
		f_borders.addEntryCallback(delegate (GameObject go) { syncBorderProperties(go); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, dsf.currentNormalColor_Scrollbar); });
		f_scrollbar.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, dsf.currentBackgroundColor_Scrollbar); });
		f_scrollview.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, dsf.currentBackgroundColor_Scrollview); });
		f_toggle.addEntryCallback(delegate (GameObject go) { syncNormalColor(go, dsf.currentNormalColor_Toggle); });
		f_tooltip.addEntryCallback(delegate (GameObject go) { syncGraphicColor(go, dsf.currentColor_Tooltip); });
		f_blocks.addEntryCallback(delegate (GameObject go) { syncBlockColor(go); });
		f_dropArea.addEntryCallback(delegate (GameObject go) { syncDropAreaColor(go); });
		f_highlightable.addEntryCallback(delegate (GameObject go) { setHighlightingColor(go); });
		f_conditionNotif.addEntryCallback(delegate (GameObject go) { syncConditionNotif(go); });

		f_settingsOpened.addEntryCallback(delegate (GameObject unused) { loadPlayerPrefs(); });
		
		loadPlayerPrefs();
		saveParameters();

		MainLoop.instance.StartCoroutine(waitLocalizationLoaded());
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
		SyncLocalization.instance.syncLocale();
		syncColor(f_dropdown, syncColor_Dropdown);
		syncColor(f_inputfield, sync_Inputfield);
		syncColor(f_buttons, syncNormalColor, dsf.currentNormalColor_Button);
		syncColor(f_icons, syncIconColor, dsf.currentColor_Icon);
		syncColor(f_buttonsPlay, syncNormalColor, dsf.currentPlayButtonColor);
		syncColor(f_buttonsPause, syncNormalColor, dsf.currentPauseButtonColor);
		syncColor(f_buttonsStop, syncNormalColor, dsf.currentStopButtonColor);
		syncColor(f_selectable, syncHighlightedColor);
		syncColor(f_SyncSelectedColor, syncGraphicColor, dsf.currentSelectedColor);
		syncColor(f_panels1, syncColor_Panel);
		syncColor(f_panels2, syncGraphicColor, dsf.currentColor_Panel2);
		syncColor(f_panels3, syncGraphicColor, dsf.currentColor_Panel3);
		syncColor(f_borders, syncBorderProperties);
		syncColor(f_scrollbar, syncNormalColor, dsf.currentNormalColor_Scrollbar);
		syncColor(f_scrollbar, syncGraphicColor, dsf.currentBackgroundColor_Scrollbar);
		syncColor(f_scrollview, syncGraphicColor, dsf.currentBackgroundColor_Scrollview);
		syncColor(f_toggle, syncNormalColor, dsf.currentNormalColor_Toggle);
		syncColor(f_tooltip, syncGraphicColor, dsf.currentColor_Tooltip);
		syncColor(f_blocks, syncBlockColor);
		syncColor(f_dropArea, syncDropAreaColor);
		syncColor(f_highlightable, setHighlightingColor);
		syncColor(f_conditionNotif, syncConditionNotif);
	}

	// lit les PlayerPrefs et initialise les UI en conséquence
	private void loadPlayerPrefs()
	{
		dsf.currentQuality = PlayerPrefs.GetInt("quality", dsf.defaultQuality);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Quality").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentQuality;

		dsf.currentInteractionMode = PlayerPrefs.GetInt("interaction", Application.platform == RuntimePlatform.WebGLPlayer && IsMobileBrowser() ? 1 : dsf.defaultInteractionMode);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/InteractionMode").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentInteractionMode;

		// définition de la taille de l'interface
		dsf.currentSizeText = settingsContent.Find("SectionGraphic/GridContainer/Grid/UISize").Find("CurrentSize").GetComponent<TMP_Text>();
		dsf.currentUIScale = PlayerPrefs.GetFloat("UIScale", (float)Math.Max(dsf.defaultUIScale, Math.Round((double)Screen.currentResolution.width / 2048, 2))); // do not reduce scale under defaultUIScale and multiply scale for definition higher than 2048
		dsf.currentSizeText.text = dsf.currentUIScale + "";
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = dsf.currentUIScale;

		dsf.currentWallTransparency = PlayerPrefs.GetInt("wallTransparency", dsf.defaultWallTransparency);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/WallTransparency").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentWallTransparency;

		dsf.currentGameView = PlayerPrefs.GetInt("orthographicView", dsf.defaultGameView);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/GameView").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentGameView;

		dsf.currentTooltipView = PlayerPrefs.GetInt("tooltipView", dsf.defaultTooltipView);
		settingsContent.Find("SectionGraphic/GridContainer/Grid/Tooltip").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentTooltipView;

		dsf.currentFont = PlayerPrefs.GetInt("font", dsf.defaultFont);
		settingsContent.Find("SectionText/GridContainer/Grid/FontDropdown").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentFont;

		dsf.currentCaretWidth = PlayerPrefs.GetInt("caretWidth", dsf.defaultCaretWidth);
		settingsContent.Find("SectionText/GridContainer/Grid/CaretWidth").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentCaretWidth;
		dsf.currentCaretHeight = PlayerPrefs.GetInt("caretHeight", dsf.defaultCaretHeight);
		settingsContent.Find("SectionText/GridContainer/Grid/CaretHeight").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentCaretHeight;

		dsf.currentCharSpacing = PlayerPrefs.GetInt("charSpacing", dsf.defaultCharSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/CharSpacing").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentCharSpacing;
		dsf.currentWordSpacing = PlayerPrefs.GetInt("wordSpacing", dsf.defaultWordSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/WordSpacing").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentWordSpacing;
		dsf.currentLineSpacing = PlayerPrefs.GetInt("lineSpacing", dsf.defaultLineSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/LineSpacing").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentLineSpacing;
		dsf.currentParagraphSpacing = PlayerPrefs.GetInt("paragraphSpacing", dsf.defaultParagraphSpacing);
		settingsContent.Find("SectionText/GridContainer/Grid/ParagraphSpacing").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentParagraphSpacing;

		// Synchronisation de la couleur des textes
		syncPlayerPrefColor("TextColorNormal", dsf.defaultNormalColor_Text, out dsf.currentNormalColor_Text, "SectionColor/GridContainer/Grid/ColorTextNormal");
		syncPlayerPrefColor("TextColorSelected", dsf.defaultSelectedColor_Text, out dsf.currentSelectedColor_Text, "SectionColor/GridContainer/Grid/ColorTextSelected");
		syncPlayerPrefColor("PlaceholderColor", dsf.defaultPlaceholderColor, out dsf.currentPlaceholderColor, "SectionColor/GridContainer/Grid/ColorPlaceholder");
		// Synchronisation de la couleur des dropdown
		syncPlayerPrefColor("DropdownColorNormal", dsf.defaultNormalColor_Dropdown, out dsf.currentNormalColor_Dropdown, "SectionColor/GridContainer/Grid/ColorDropdownNormal");
		// Synchronisation de la couleur des inputfield
		syncPlayerPrefColor("InputfieldColorNormal", dsf.defaultNormalColor_Inputfield, out dsf.currentNormalColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldNormal");
		syncPlayerPrefColor("InputfieldColorSelected", dsf.defaultSelectedColor_Inputfield, out dsf.currentSelectedColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldSelected");
		syncPlayerPrefColor("InputfieldColorSelection", dsf.defaultSelectionColor_Inputfield, out dsf.currentSelectionColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldSelection");
		syncPlayerPrefColor("InputfieldColorCaret", dsf.defaultCaretColor_Inputfield, out dsf.currentCaretColor_Inputfield, "SectionColor/GridContainer/Grid/ColorInputfieldCaret");
		// Synchronisation de la couleur des bouttons
		syncPlayerPrefColor("ButtonColorNormal", dsf.defaultNormalColor_Button, out dsf.currentNormalColor_Button, "SectionColor/GridContainer/Grid/ColorButtonNormal");
		// Synchronisation de la couleur des highlighted
		syncPlayerPrefColor("HighlightedColor", dsf.defaultHighlightedColor, out dsf.currentHighlightedColor, "SectionColor/GridContainer/Grid/ColorHighlighted");
		// Synchronisation de la couleur des pressed
		syncPlayerPrefColor("PressedColor", dsf.defaultPressedColor, out dsf.currentPressedColor, "SectionColor/GridContainer/Grid/ColorPressed");
		// Synchronisation de la couleur des selected
		syncPlayerPrefColor("SelectedColor", dsf.defaultSelectedColor, out dsf.currentSelectedColor, "SectionColor/GridContainer/Grid/ColorSelected");
		// Synchronisation de la couleur des disabled
		syncPlayerPrefColor("DisabledColor", dsf.defaultDisabledColor, out dsf.currentDisabledColor, "SectionColor/GridContainer/Grid/ColorDisabled");
		// Synchronisation de la couleur des bouttons icônes
		syncPlayerPrefColor("IconColor", dsf.defaultColor_Icon, out dsf.currentColor_Icon, "SectionColor/GridContainer/Grid/ColorIcon");
		// Synchronisation de la couleur des panels
		syncPlayerPrefColor("Panel1Color", dsf.defaultColor_Panel1, out dsf.currentColor_Panel1, "SectionColor/GridContainer/Grid/ColorPanel1");
		syncPlayerPrefColor("Panel2Color", dsf.defaultColor_Panel2, out dsf.currentColor_Panel2, "SectionColor/GridContainer/Grid/ColorPanel2");
		syncPlayerPrefColor("Panel3Color", dsf.defaultColor_Panel3, out dsf.currentColor_Panel3, "SectionColor/GridContainer/Grid/ColorPanel3");
		syncPlayerPrefColor("PanelColorTexture", dsf.defaultColor_PanelTexture, out dsf.currentColor_PanelTexture, "SectionColor/GridContainer/Grid/ColorPanelTexture");
		// Synchronisation des propriétés de bordure
		syncPlayerPrefColor("BorderColor", dsf.defaultColor_Border, out dsf.currentColor_Border, "SectionColor/GridContainer/Grid/ColorBorder");
		dsf.currentBorderThickness = PlayerPrefs.GetInt("BorderThickness", dsf.defaultBorderThickness);
		settingsContent.Find("SectionColor/GridContainer/Grid/ThicknessBorder").GetComponentInChildren<TMP_Dropdown>().value = dsf.currentBorderThickness - 1;
		// Synchronisation de la couleur des scrollbars
		syncPlayerPrefColor("ScrollbarColorNormal", dsf.defaultNormalColor_Scrollbar, out dsf.currentNormalColor_Scrollbar, "SectionColor/GridContainer/Grid/ColorScrollbarNormal");
		syncPlayerPrefColor("ScrollbarColorBackground", dsf.defaultBackgroundColor_Scrollbar, out dsf.currentBackgroundColor_Scrollbar, "SectionColor/GridContainer/Grid/ColorScrollbarBackground");
		syncPlayerPrefColor("ScrollviewColor", dsf.defaultBackgroundColor_Scrollview, out dsf.currentBackgroundColor_Scrollview, "SectionColor/GridContainer/Grid/ColorScrollview");
		// Synchronisation de la couleur des toggles
		syncPlayerPrefColor("ToggleColorNormal", dsf.defaultNormalColor_Toggle, out dsf.currentNormalColor_Toggle, "SectionColor/GridContainer/Grid/ColorToggleNormal");
		// Synchronisation de la couleur des tooltip
		syncPlayerPrefColor("TooltipColor", dsf.defaultColor_Tooltip, out dsf.currentColor_Tooltip, "SectionColor/GridContainer/Grid/ColorTooltip");
		// Synchronisation des couleurs du player
		syncPlayerPrefColor("PlayColor", dsf.defaultPlayButtonColor, out dsf.currentPlayButtonColor, "PlayerColors/GridContainer/Grid/ColorPlayButton");
		syncPlayerPrefColor("PauseColor", dsf.defaultPauseButtonColor, out dsf.currentPauseButtonColor, "PlayerColors/GridContainer/Grid/ColorPauseButton");
		syncPlayerPrefColor("StopColor", dsf.defaultStopButtonColor, out dsf.currentStopButtonColor, "PlayerColors/GridContainer/Grid/ColorStopButton");
		syncPlayerPrefColor("ActionBlockColor", dsf.defaultActionBlockColor, out dsf.currentActionBlockColor, "PlayerColors/GridContainer/Grid/ColorActionBlock");
		syncPlayerPrefColor("ControlBlockColor", dsf.defaultControlBlockColor, out dsf.currentControlBlockColor, "PlayerColors/GridContainer/Grid/ColorControlBlock");
		syncPlayerPrefColor("OperatorBlockColor", dsf.defaultOperatorBlockColor, out dsf.currentOperatorBlockColor, "PlayerColors/GridContainer/Grid/ColorOperatorBlock");
		syncPlayerPrefColor("CaptorBlockColor", dsf.defaultCaptorBlockColor, out dsf.currentCaptorBlockColor, "PlayerColors/GridContainer/Grid/ColorCaptorBlock");
		syncPlayerPrefColor("DropAreaColor", dsf.defaultDropAreaColor, out dsf.currentDropAreaColor, "PlayerColors/GridContainer/Grid/ColorDropArea");
		syncPlayerPrefColor("HighlightingColor", dsf.defaultHighlightingColor, out dsf.currentHighlightingColor, "PlayerColors/GridContainer/Grid/ColorHighlighting");
		syncPlayerPrefColor("CaptorTrueColor", dsf.defaultCaptorTrueColor, out dsf.currentCaptorTrueColor, "PlayerColors/GridContainer/Grid/ColorCaptorTrue");
		syncPlayerPrefColor("CaptorFalseColor", dsf.defaultCaptorFalseColor, out dsf.currentCaptorFalseColor, "PlayerColors/GridContainer/Grid/ColorCaptorFalse");
	}

	private void syncPlayerPrefColor(string playerPrefKey, Color defaultColor, out Color currentColor, string goName)
	{
		ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString(playerPrefKey, ColorUtility.ToHtmlStringRGBA(defaultColor)), out currentColor);
		settingsContent.Find(goName + "/ButtonWithBorder").GetComponent<Image>().color = currentColor;
	}

	public void saveParameters()
	{
		// TODO : voir comment gérer la sauvegarde des PlayerPref dissiminés dans le code, cas du orthographicView
		PlayerPrefs.SetInt("quality", dsf.currentQuality);
		PlayerPrefs.SetInt("interaction", dsf.currentInteractionMode);
		PlayerPrefs.SetFloat("UIScale", dsf.currentUIScale);
		PlayerPrefs.SetInt("wallTransparency", dsf.currentWallTransparency);
		PlayerPrefs.SetInt("orthographicView", dsf.currentGameView);
		PlayerPrefs.SetInt("tooltipView", dsf.currentTooltipView);
		PlayerPrefs.SetInt("font", dsf.currentFont);
		PlayerPrefs.SetInt("caretWidth", dsf.currentCaretWidth);
		PlayerPrefs.SetInt("caretHeight", dsf.currentCaretHeight);
		PlayerPrefs.SetInt("charSpacing", dsf.currentCharSpacing);
		PlayerPrefs.SetInt("wordSpacing", dsf.currentWordSpacing);
		PlayerPrefs.SetInt("lineSpacing", dsf.currentLineSpacing);
		PlayerPrefs.SetInt("paragraphSpacing", dsf.currentParagraphSpacing);
		PlayerPrefs.SetString("TextColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.currentNormalColor_Text));
		PlayerPrefs.SetString("TextColorSelected", ColorUtility.ToHtmlStringRGBA(dsf.currentSelectedColor_Text));
		PlayerPrefs.SetString("PlaceholderColor", ColorUtility.ToHtmlStringRGBA(dsf.currentPlaceholderColor));
		PlayerPrefs.SetString("DropdownColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.currentNormalColor_Dropdown));
		PlayerPrefs.SetString("InputfieldColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.currentNormalColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelected", ColorUtility.ToHtmlStringRGBA(dsf.currentSelectedColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorSelection", ColorUtility.ToHtmlStringRGBA(dsf.currentSelectionColor_Inputfield));
		PlayerPrefs.SetString("InputfieldColorCaret", ColorUtility.ToHtmlStringRGBA(dsf.currentCaretColor_Inputfield));
		PlayerPrefs.SetString("ButtonColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.currentNormalColor_Button));
		PlayerPrefs.SetString("HighlightedColor", ColorUtility.ToHtmlStringRGBA(dsf.currentHighlightedColor));
		PlayerPrefs.SetString("PressedColor", ColorUtility.ToHtmlStringRGBA(dsf.currentPressedColor));
		PlayerPrefs.SetString("SelectedColor", ColorUtility.ToHtmlStringRGBA(dsf.currentSelectedColor));
		PlayerPrefs.SetString("DisabledColor", ColorUtility.ToHtmlStringRGBA(dsf.currentDisabledColor));
		PlayerPrefs.SetString("IconColor", ColorUtility.ToHtmlStringRGBA(dsf.currentColor_Icon));
		PlayerPrefs.SetString("Panel1Color", ColorUtility.ToHtmlStringRGBA(dsf.currentColor_Panel1));
		PlayerPrefs.SetString("Panel2Color", ColorUtility.ToHtmlStringRGBA(dsf.currentColor_Panel2));
		PlayerPrefs.SetString("Panel3Color", ColorUtility.ToHtmlStringRGBA(dsf.currentColor_Panel3));
		PlayerPrefs.SetString("PanelColorTexture", ColorUtility.ToHtmlStringRGBA(dsf.currentColor_PanelTexture));
		PlayerPrefs.SetString("BorderColor", ColorUtility.ToHtmlStringRGBA(dsf.currentColor_Border));
		PlayerPrefs.SetInt("BorderThickness", dsf.currentBorderThickness);
		PlayerPrefs.SetString("ScrollbarColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.currentNormalColor_Scrollbar));
		PlayerPrefs.SetString("ScrollbarColorBackground", ColorUtility.ToHtmlStringRGBA(dsf.currentBackgroundColor_Scrollbar));
		PlayerPrefs.SetString("ScrollviewColor", ColorUtility.ToHtmlStringRGBA(dsf.currentBackgroundColor_Scrollview));
		PlayerPrefs.SetString("ToggleColorNormal", ColorUtility.ToHtmlStringRGBA(dsf.currentNormalColor_Toggle));
		PlayerPrefs.SetString("TooltipColor", ColorUtility.ToHtmlStringRGBA(dsf.currentColor_Tooltip));
		PlayerPrefs.SetString("PlayColor", ColorUtility.ToHtmlStringRGBA(dsf.currentPlayButtonColor));
		PlayerPrefs.SetString("PauseColor", ColorUtility.ToHtmlStringRGBA(dsf.currentPauseButtonColor));
		PlayerPrefs.SetString("StopColor", ColorUtility.ToHtmlStringRGBA(dsf.currentStopButtonColor));
		PlayerPrefs.SetString("ActionBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.currentActionBlockColor));
		PlayerPrefs.SetString("ControlBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.currentControlBlockColor));
		PlayerPrefs.SetString("OperatorBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.currentOperatorBlockColor));
		PlayerPrefs.SetString("CaptorBlockColor", ColorUtility.ToHtmlStringRGBA(dsf.currentCaptorBlockColor));
		PlayerPrefs.SetString("DropAreaColor", ColorUtility.ToHtmlStringRGBA(dsf.currentDropAreaColor));
		PlayerPrefs.SetString("HighlightingColor", ColorUtility.ToHtmlStringRGBA(dsf.currentHighlightingColor));
		PlayerPrefs.SetString("CaptorTrueColor", ColorUtility.ToHtmlStringRGBA(dsf.currentCaptorTrueColor));
		PlayerPrefs.SetString("CaptorFalseColor", ColorUtility.ToHtmlStringRGBA(dsf.currentCaptorFalseColor));
		PlayerPrefs.Save();
		// TODO : Penser à sauvegarder dans la BD le choix de la langue

	}

	public void resetParameters()
	{
		dsf.currentQuality = dsf.defaultQuality;
		dsf.currentInteractionMode = dsf.defaultInteractionMode;
		dsf.currentUIScale = dsf.defaultUIScale;
		dsf.currentWallTransparency = dsf.defaultWallTransparency;
		dsf.currentGameView = dsf.defaultGameView;
		dsf.currentTooltipView = dsf.defaultTooltipView;
		dsf.currentFont = dsf.defaultFont;
		dsf.currentCaretWidth = dsf.defaultCaretWidth;
		dsf.currentCaretHeight = dsf.defaultCaretHeight;
		dsf.currentCharSpacing = dsf.defaultCharSpacing;
		dsf.currentWordSpacing = dsf.defaultWordSpacing;
		dsf.currentLineSpacing = dsf.defaultLineSpacing;
		dsf.currentParagraphSpacing = dsf.defaultParagraphSpacing;
		dsf.currentNormalColor_Text = dsf.defaultNormalColor_Text;
		dsf.currentSelectedColor_Text = dsf.defaultSelectedColor_Text;
		dsf.currentPlaceholderColor = dsf.defaultPlaceholderColor;
		dsf.currentNormalColor_Dropdown = dsf.defaultNormalColor_Dropdown;
		dsf.currentNormalColor_Inputfield = dsf.defaultNormalColor_Inputfield;
		dsf.currentSelectedColor_Inputfield = dsf.defaultSelectedColor_Inputfield;
		dsf.currentSelectionColor_Inputfield = dsf.defaultSelectionColor_Inputfield;
		dsf.currentCaretColor_Inputfield = dsf.defaultCaretColor_Inputfield;
		dsf.currentNormalColor_Button = dsf.defaultNormalColor_Button;
		dsf.currentHighlightedColor = dsf.defaultHighlightedColor;
		dsf.currentPressedColor = dsf.defaultPressedColor;
		dsf.currentSelectedColor = dsf.defaultSelectedColor;
		dsf.currentDisabledColor = dsf.defaultDisabledColor;
		dsf.currentColor_Icon = dsf.defaultColor_Icon;
		dsf.currentColor_Panel1 = dsf.defaultColor_Panel1;
		dsf.currentColor_Panel2 = dsf.defaultColor_Panel2;
		dsf.currentColor_Panel3 = dsf.defaultColor_Panel3;
		dsf.currentColor_PanelTexture = dsf.defaultColor_PanelTexture;
		dsf.currentColor_Border = dsf.defaultColor_Border;
		dsf.currentBorderThickness = dsf.defaultBorderThickness;
		dsf.currentNormalColor_Scrollbar = dsf.defaultNormalColor_Scrollbar;
		dsf.currentBackgroundColor_Scrollbar = dsf.defaultBackgroundColor_Scrollbar;
		dsf.currentBackgroundColor_Scrollview = dsf.defaultBackgroundColor_Scrollview;
		dsf.currentNormalColor_Toggle = dsf.defaultNormalColor_Toggle;
		dsf.currentColor_Tooltip = dsf.defaultColor_Tooltip;
		dsf.currentPlayButtonColor = dsf.defaultPlayButtonColor;
		dsf.currentPauseButtonColor = dsf.defaultPauseButtonColor;
		dsf.currentStopButtonColor = dsf.defaultStopButtonColor;
		dsf.currentActionBlockColor = dsf.defaultActionBlockColor;
		dsf.currentControlBlockColor = dsf.defaultControlBlockColor;
		dsf.currentOperatorBlockColor = dsf.defaultOperatorBlockColor;
		dsf.currentCaptorBlockColor = dsf.defaultCaptorBlockColor;
		dsf.currentDropAreaColor = dsf.defaultDropAreaColor;
		dsf.currentHighlightingColor = dsf.defaultHighlightingColor;
		dsf.currentCaptorTrueColor = dsf.defaultCaptorTrueColor;
		dsf.currentCaptorFalseColor = dsf.defaultCaptorFalseColor;

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
					dsf.currentNormalColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "TextColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentSelectedColor_Text = c;
					syncColor(f_allTexts, syncColor_Text);
					SyncLocalization.instance.syncLocale();
				});
				break;
			case "PlaceholderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentPlaceholderColor = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "DropdownColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentNormalColor_Dropdown = c;
					syncColor(f_dropdown, syncColor_Dropdown);
				});
				break;
			case "InputfieldColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentNormalColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelected":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentSelectedColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorSelection":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentSelectionColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "InputfieldColorCaret":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentCaretColor_Inputfield = c;
					syncColor(f_inputfield, sync_Inputfield);
				});
				break;
			case "ButtonColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentNormalColor_Button = c;
					syncColor(f_buttons, syncNormalColor, dsf.currentNormalColor_Button);
				});
				break;
			case "HighlightedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentHighlightedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "PressedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentPressedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "SelectedColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentSelectedColor = c;
					syncColor(f_selectable, syncHighlightedColor);
					syncColor(f_SyncSelectedColor, syncGraphicColor, dsf.currentSelectedColor);
				});
				break;
			case "DisabledColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentDisabledColor = c;
					syncColor(f_allTexts, syncColor_Text);
					syncColor(f_selectable, syncHighlightedColor);
				});
				break;
			case "IconColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentColor_Icon = c;
					syncColor(f_icons, syncIconColor, dsf.currentColor_Icon);
				});
				break;
			case "Panel1Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentColor_Panel1 = c;
					syncColor(f_panels1, syncColor_Panel);
				});
				break;
			case "Panel2Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentColor_Panel2 = c;
					syncColor(f_panels2, syncGraphicColor, dsf.currentColor_Panel2);
				});
				break;
			case "Panel3Color":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentColor_Panel3 = c;
					syncColor(f_panels3, syncGraphicColor, dsf.currentColor_Panel3);
				});
				break;
			case "PanelColorTexture":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentColor_PanelTexture = c;
					syncColor(f_panels1, syncColor_Panel);
				});
				break;
			case "BorderColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentColor_Border = c;
					syncColor(f_borders, syncBorderProperties, dsf.currentColor_Border);
				});
				break;
			case "ScrollbarColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentNormalColor_Scrollbar = c;
					syncColor(f_scrollbar, syncNormalColor, dsf.currentNormalColor_Scrollbar);
				});
				break;
			case "ScrollbarColorBackground":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentBackgroundColor_Scrollbar = c;
					syncColor(f_scrollbar, syncGraphicColor, dsf.currentBackgroundColor_Scrollbar);
				});
				break;
			case "ScrollviewColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentBackgroundColor_Scrollview = c;
					syncColor(f_scrollview, syncGraphicColor, dsf.currentBackgroundColor_Scrollview);
				});
				break;
			case "ToggleColorNormal":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentNormalColor_Toggle = c;
					syncColor(f_toggle, syncNormalColor, dsf.currentNormalColor_Toggle);
				});
				break;
			case "TooltipColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentColor_Tooltip = c;
					syncColor(f_tooltip, syncGraphicColor, dsf.currentColor_Tooltip);
				});
				break;
			case "PlayColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentPlayButtonColor = c;
					syncColor(f_buttonsPlay, syncNormalColor, dsf.currentPlayButtonColor);
				});
				break;
			case "PauseColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentPauseButtonColor = c;
					syncColor(f_buttonsPause, syncNormalColor, dsf.currentPauseButtonColor);
				});
				break;
			case "StopColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentStopButtonColor = c;
					syncColor(f_buttonsStop, syncNormalColor, dsf.currentStopButtonColor);
				});
				break;
			case "ActionBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentActionBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "ControlBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentControlBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "OperatorBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentOperatorBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "CaptorBlockColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentCaptorBlockColor = c;
					syncColor(f_blocks, syncBlockColor);
				});
				break;
			case "DropAreaColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentDropAreaColor = c;
					syncColor(f_dropArea, syncDropAreaColor);
				});
				break; 
			case "HighlightingColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentHighlightingColor = c;
					syncColor(f_highlightable, setHighlightingColor);
				});
				break;
			case "CaptorTrueColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentCaptorTrueColor = c;
					syncColor(f_conditionNotif, syncConditionNotif);
				});
				break;
			case "CaptorFalseColor":
				flexibleColorPicker.onColorChange.AddListener(delegate (Color c) {
					dsf.currentCaptorFalseColor = c;
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
		dsf.currentQuality = value;
	}

	public void setInteraction(int value)
	{
		dsf.currentInteractionMode = value;
	}

	public void increaseUISize()
	{
		dsf.currentUIScale += 0.25f;
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = dsf.currentUIScale;
		dsf.currentSizeText.text = dsf.currentUIScale + "";
	}
	public void decreaseUISize()
	{
		if (dsf.currentUIScale >= 0.5f)
			dsf.currentUIScale -= 0.25f;
		foreach (CanvasScaler canvas in canvasScaler)
			canvas.scaleFactor = dsf.currentUIScale;
		dsf.currentSizeText.text = dsf.currentUIScale + "";
	}

	public void setWallTransparency(int value)
	{
		if (ObstableTransparencySystem.instance != null)
			ObstableTransparencySystem.instance.Pause = value == 0;
		dsf.currentWallTransparency = value;
	}

	public void setGameView(int value)
	{
		if (CameraSystem.instance != null)
			CameraSystem.instance.setOrthographicView(value == 1);
		dsf.currentGameView = value;
	}

	public void setTooltipView(int value)
	{
		dsf.currentTooltipView = value;
	}

	public void syncFonts(int value)
	{
		dsf.currentFont = value;
		foreach (GameObject go in f_modifiableFonts)
			syncFont(go);
	}

	private void syncFont(GameObject go)
	{
		TMP_InputField inputField = go.GetComponent<TMP_InputField>();
		if (inputField != null)
			inputField.fontAsset = dsf.fonts[dsf.currentFont];
		else
			go.GetComponent<TextMeshProUGUI>().font = dsf.fonts[dsf.currentFont];
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
		dsf.currentCaretWidth = value;
		foreach (GameObject inputGO in f_inputfield)
			sync_Inputfield(inputGO);
	}

	public void setCaretHeight(int value)
	{
		dsf.currentCaretHeight = value;
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
			currentColor.normalColor = dsf.currentNormalColor_Text;
			currentColor.highlightedColor = dsf.currentNormalColor_Text;
			currentColor.pressedColor = dsf.currentNormalColor_Text;
			currentColor.selectedColor = dsf.currentSelectedColor_Text;
			currentColor.disabledColor = dsf.currentDisabledColor;
			textSel.colors = currentColor;

		}
		// Pour les textes qui sont enfant d'un LangOption => Mettre à jour la couleur du bouton et le LangOption
		else if (text.GetComponentInParent<LangOption>(true))
		{
			Button langBtn = text.GetComponentInParent<Button>(true);
			ColorBlock currentColor = langBtn.colors;
			currentColor.normalColor = dsf.currentNormalColor_Text;
			langBtn.colors = currentColor;
			LangOption langOpt = text.GetComponentInParent<LangOption>(true);
			langOpt.on = dsf.currentSelectedColor_Text;
			langOpt.off = dsf.currentNormalColor_Text;
		}
		else
		{
			Selectable parentSel = text.GetComponentInParent<Selectable>(true);
			// Pour les textes qui sont enfant d'un Selectable et dont le Selectable contrôle la couleur du texte => Mettre à jour la couleur du Selectable
			if (parentSel != null && parentSel.targetGraphic == text.GetComponent<Graphic>())
			{
				ColorBlock currentColor = parentSel.colors;
				currentColor.normalColor = dsf.currentNormalColor_Text;
				parentSel.colors = currentColor;
			}
			// Sinon tous les autres textes, on change simplement leur couleur sauf si on est sur un placeholder d'un input field (pour ce cas voir syncColor_Inputfield)
			else
			{
				TMP_InputField inputField = text.GetComponentInParent<TMP_InputField>(true);
				if (inputField == null || inputField.placeholder != text.GetComponent<Graphic>())
					syncGraphicColor(text, dsf.currentNormalColor_Text);
			}
		}
	}

	private void syncColor_Dropdown(GameObject go, Color? unused = null)
	{
		syncNormalColor(go, dsf.currentNormalColor_Dropdown);
		syncGraphicColor(go.transform.Find("Template").gameObject, dsf.currentNormalColor_Dropdown);
	}

	private void sync_Inputfield(GameObject go, Color? unused = null)
	{
		TMP_InputField input = go.GetComponent<TMP_InputField>();
		ColorBlock currentColor = input.colors;
		currentColor.normalColor = dsf.currentNormalColor_Inputfield;
		currentColor.pressedColor = dsf.currentSelectedColor_Inputfield;
		input.colors = currentColor;
		input.placeholder.color = dsf.currentPlaceholderColor;
		input.selectionColor = dsf.currentSelectionColor_Inputfield;
		input.caretColor = dsf.currentCaretColor_Inputfield;
		input.caretWidth = (dsf.currentCaretWidth + 1) * 2;
	}

	private void sync_CaretHeight(GameObject go)
	{
		go.transform.localScale = new Vector3(1f, dsf.currentCaretHeight + 1, 1f);
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
		currentColor.highlightedColor = dsf.currentHighlightedColor;
		currentColor.pressedColor = dsf.currentPressedColor;
		currentColor.selectedColor = dsf.currentSelectedColor;
		currentColor.disabledColor = dsf.currentDisabledColor;
		select.colors = currentColor;
	}

	private void syncColor_Panel(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, dsf.currentColor_Panel1);
		Transform texture = go.transform.Find("Texture");
		if (texture != null)
			syncGraphicColor(texture.gameObject, dsf.currentColor_PanelTexture);
	}

	public void setBorderTickness(int value)
	{
		dsf.currentBorderThickness = value + 1;
		syncColor(f_borders, syncBorderProperties);
	}

	private void syncBorderProperties(GameObject go, Color? unused = null)
	{
		syncGraphicColor(go, dsf.currentColor_Border);
		go.GetComponent<Image>().pixelsPerUnitMultiplier = 1f / dsf.currentBorderThickness;
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
		dsf.currentCharSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setWordSpacing(int value)
	{
		dsf.currentWordSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setLineSpacing(int value)
	{
		dsf.currentLineSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	public void setParagraphSpacing(int value)
	{
		dsf.currentParagraphSpacing = value;
		foreach (GameObject go in f_allTexts)
			syncSpacing_Text(go);
	}

	private void syncSpacing_Text(GameObject textGO)
	{
		TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
		// Transformation des numéro d'item sélectionné en valeurs d'espacement
		text.characterSpacing = (dsf.currentCharSpacing - 2) * 10;
		text.wordSpacing = (dsf.currentWordSpacing - 2) * 10;
		text.lineSpacing = (dsf.currentLineSpacing - 2) * 10;
		text.paragraphSpacing = (dsf.currentParagraphSpacing - 2) * 10;
	}
	
	private void syncBlockColor(GameObject go, Color? unused = null)
	{
		switch (go.tag)
		{
			case "UI_Action":
				syncNormalColor(go, dsf.currentActionBlockColor);
				break;
			case "UI_Control":
				syncNormalColor(go, dsf.currentControlBlockColor);
				break;
			case "UI_Operator":
				syncNormalColor(go, dsf.currentOperatorBlockColor);
				break;
			case "UI_Captor":
				syncNormalColor(go, dsf.currentCaptorBlockColor);
				break;
		}
	}

	private void syncDropAreaColor(GameObject go, Color? unused = null)
	{
		if (go.GetComponent<DropZone>())
			syncGraphicColor(go.transform.Find("PositionBar").gameObject, dsf.currentDropAreaColor);
        else
			go.GetComponent<Outline>().effectColor = dsf.currentDropAreaColor;
	}

	private void setHighlightingColor(GameObject go, Color? unused = null)
	{
		if (go.GetComponent<Highlightable>())
			go.GetComponent<Highlightable>().highlightedColor = dsf.currentHighlightingColor;
		else
			go.GetComponent<BasicAction>().highlightedColor = dsf.currentHighlightingColor;
	}
	
	private void syncConditionNotif(GameObject go, Color? unused = null)
	{
		if (go.name == "true")
			syncGraphicColor(go, dsf.currentCaptorTrueColor);
		else
			syncGraphicColor(go, dsf.currentCaptorFalseColor);
	}


}
