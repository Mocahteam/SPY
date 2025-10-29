using UnityEngine;
using FYFY;

public class SettingsManager_wrapper : BaseWrapper
{
	public UnityEngine.Transform settingsContent;
	public FlexibleColorPicker flexibleColorPicker;
	public UnityEngine.UI.CanvasScaler[] canvasScaler;
	public System.Int32 defaultQuality;
	public System.Int32 defaultInteractionMode;
	public System.Single defaultUIScale;
	public System.Int32 defaultWallTransparency;
	public System.Int32 defaultGameView;
	public UnityEngine.Color defaultNormalColor_Text;
	public UnityEngine.Color defaultSelectedColor_Text;
	public UnityEngine.Color defaultNormalColor_Button;
	public UnityEngine.Color defaultNormalColor_ButtonIcon;
	public UnityEngine.Color defaultHighlightedColor;
	public UnityEngine.Color defaultPressedColor;
	public UnityEngine.Color defaultSelectedColor;
	public UnityEngine.Color defaultDisabledColor;
	public UnityEngine.Color defaultColor_Panel;
	public UnityEngine.Color defaultColor_PanelTexture;
	public UnityEngine.Color defaultColor_Border;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "settingsContent", settingsContent);
		MainLoop.initAppropriateSystemField (system, "flexibleColorPicker", flexibleColorPicker);
		MainLoop.initAppropriateSystemField (system, "canvasScaler", canvasScaler);
		MainLoop.initAppropriateSystemField (system, "defaultQuality", defaultQuality);
		MainLoop.initAppropriateSystemField (system, "defaultInteractionMode", defaultInteractionMode);
		MainLoop.initAppropriateSystemField (system, "defaultUIScale", defaultUIScale);
		MainLoop.initAppropriateSystemField (system, "defaultWallTransparency", defaultWallTransparency);
		MainLoop.initAppropriateSystemField (system, "defaultGameView", defaultGameView);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Text", defaultNormalColor_Text);
		MainLoop.initAppropriateSystemField (system, "defaultSelectedColor_Text", defaultSelectedColor_Text);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_Button", defaultNormalColor_Button);
		MainLoop.initAppropriateSystemField (system, "defaultNormalColor_ButtonIcon", defaultNormalColor_ButtonIcon);
		MainLoop.initAppropriateSystemField (system, "defaultHighlightedColor", defaultHighlightedColor);
		MainLoop.initAppropriateSystemField (system, "defaultPressedColor", defaultPressedColor);
		MainLoop.initAppropriateSystemField (system, "defaultSelectedColor", defaultSelectedColor);
		MainLoop.initAppropriateSystemField (system, "defaultDisabledColor", defaultDisabledColor);
		MainLoop.initAppropriateSystemField (system, "defaultColor_Panel", defaultColor_Panel);
		MainLoop.initAppropriateSystemField (system, "defaultColor_PanelTexture", defaultColor_PanelTexture);
		MainLoop.initAppropriateSystemField (system, "defaultColor_Border", defaultColor_Border);
	}

	public void saveParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveParameters", null);
	}

	public void resetParameters()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetParameters", null);
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

	public void hookListener(System.String key)
	{
		MainLoop.callAppropriateSystemMethod (system, "hookListener", key);
	}

}
