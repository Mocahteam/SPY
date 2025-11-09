using UnityEngine;
using FYFY;

public class SettingsManager_wrapper : BaseWrapper
{
	public UnityEngine.Transform settingsContent;
	public FlexibleColorPicker flexibleColorPicker;
	public UnityEngine.UI.CanvasScaler[] canvasScaler;
	public TMPro.TMP_FontAsset[] fonts;
	public UnityEngine.UI.Selectable LoadingLogs;
	public System.Int32 defaultQuality;
	public System.Int32 defaultInteractionMode;
	public System.Single defaultUIScale;
	public System.Int32 defaultWallTransparency;
	public System.Int32 defaultGameView;
	public System.Int32 defaultFont;
	public UnityEngine.Color defaultNormalColor_Text;
	public UnityEngine.Color defaultSelectedColor_Text;
	public UnityEngine.Color defaultPlaceholderColor;
	public UnityEngine.Color defaultNormalColor_Dropdown;
	public UnityEngine.Color defaultNormalColor_Inputfield;
	public UnityEngine.Color defaultSelectedColor_Inputfield;
	public UnityEngine.Color defaultSelectionColor_Inputfield;
	public UnityEngine.Color defaultNormalColor_Button;
	public UnityEngine.Color defaultHighlightedColor;
	public UnityEngine.Color defaultPressedColor;
	public UnityEngine.Color defaultSelectedColor;
	public UnityEngine.Color defaultDisabledColor;
	public UnityEngine.Color defaultColor_Icon;
	public UnityEngine.Color defaultColor_Panel;
	public UnityEngine.Color defaultColor_PanelTexture;
	public UnityEngine.Color defaultColor_Border;
	public System.Int32 defaultBorderThickness;
	public UnityEngine.Color defaultNormalColor_Scrollbar;
	public UnityEngine.Color defaultBackgroundColor_Scrollbar;
	public UnityEngine.Color defaultBackgroundColor_Scrollview;
	public UnityEngine.Color defaultNormalColor_Toggle;
	public UnityEngine.Color defaultColor_Tooltip;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "settingsContent", settingsContent);
		MainLoop.initAppropriateSystemField (system, "flexibleColorPicker", flexibleColorPicker);
		MainLoop.initAppropriateSystemField (system, "canvasScaler", canvasScaler);
		MainLoop.initAppropriateSystemField (system, "fonts", fonts);
		MainLoop.initAppropriateSystemField (system, "LoadingLogs", LoadingLogs);
		MainLoop.initAppropriateSystemField (system, "defaultQuality", defaultQuality);
		MainLoop.initAppropriateSystemField (system, "defaultInteractionMode", defaultInteractionMode);
		MainLoop.initAppropriateSystemField (system, "defaultUIScale", defaultUIScale);
		MainLoop.initAppropriateSystemField (system, "defaultWallTransparency", defaultWallTransparency);
		MainLoop.initAppropriateSystemField (system, "defaultGameView", defaultGameView);
		MainLoop.initAppropriateSystemField (system, "defaultFont", defaultFont);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Text", defaultNormalColor_Text);
		MainLoop.initAppropriateSystemField (system, "defaultSelectedColor_Text", defaultSelectedColor_Text);
		MainLoop.initAppropriateSystemField (system, "defaultPlaceholderColor", defaultPlaceholderColor);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Dropdown", defaultNormalColor_Dropdown);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Inputfield", defaultNormalColor_Inputfield);
		MainLoop.initAppropriateSystemField (system, "defaultSelectedColor_Inputfield", defaultSelectedColor_Inputfield);
		MainLoop.initAppropriateSystemField (system, "defaultSelectionColor_Inputfield", defaultSelectionColor_Inputfield);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Button", defaultNormalColor_Button);
		MainLoop.initAppropriateSystemField (system, "defaultHighlightedColor", defaultHighlightedColor);
		MainLoop.initAppropriateSystemField (system, "defaultPressedColor", defaultPressedColor);
		MainLoop.initAppropriateSystemField (system, "defaultSelectedColor", defaultSelectedColor);
		MainLoop.initAppropriateSystemField (system, "defaultDisabledColor", defaultDisabledColor);
		MainLoop.initAppropriateSystemField (system, "defaultColor_Icon", defaultColor_Icon);
		MainLoop.initAppropriateSystemField (system, "defaultColor_Panel", defaultColor_Panel);
		MainLoop.initAppropriateSystemField (system, "defaultColor_PanelTexture", defaultColor_PanelTexture);
		MainLoop.initAppropriateSystemField (system, "defaultColor_Border", defaultColor_Border);
		MainLoop.initAppropriateSystemField (system, "defaultBorderThickness", defaultBorderThickness);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Scrollbar", defaultNormalColor_Scrollbar);
		MainLoop.initAppropriateSystemField (system, "defaultBackgroundColor_Scrollbar", defaultBackgroundColor_Scrollbar);
		MainLoop.initAppropriateSystemField (system, "defaultBackgroundColor_Scrollview", defaultBackgroundColor_Scrollview);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Toggle", defaultNormalColor_Toggle);
		MainLoop.initAppropriateSystemField (system, "defaultColor_Tooltip", defaultColor_Tooltip);
	}

	public void saveParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveParameters", null);
	}

	public void resetParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetParameters", null);
	}

	public void hookListener(System.String key)
	{
		MainLoop.callAppropriateSystemMethod (system, "hookListener", key);
	}

	public void setQualitySetting(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setQualitySetting", value);
	}

	public void setInteraction(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setInteraction", value);
	}

	public void increaseUISize()
	{
		MainLoop.callAppropriateSystemMethod (system, "increaseUISize", null);
	}

	public void decreaseUISize()
	{
		MainLoop.callAppropriateSystemMethod (system, "decreaseUISize", null);
	}

	public void setWallTransparency(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setWallTransparency", value);
	}

	public void setGameView(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setGameView", value);
	}

	public void syncFonts(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "syncFonts", value);
	}

	public void setBorderTickness(System.Int32 value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setBorderTickness", value);
	}

}
