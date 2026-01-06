using TMPro;
using UnityEngine;

public class DefaultSettingsValues : MonoBehaviour
{
	public TMP_FontAsset[] fonts;
	public int defaultQuality = 2;
	public int defaultInteractionMode = 0;
	public float defaultUIScale = 1;
	public int defaultWallTransparency = 1;
	public int defaultGameView = 0;
	public int defaultTooltipView = 1;
	public int defaultFont = 6;
	public int defaultCaretWidth = 0;
	public int defaultCaretHeight = 0;
	public int defaultCharSpacing = 2;
	public int defaultWordSpacing = 2;
	public int defaultLineSpacing = 2;
	public int defaultParagraphSpacing = 2;
	public Color defaultNormalColor_Text = Color.black; // black
	public Color defaultSelectedColor_Text = new Color(131f / 255f, 71f / 255f, 2f / 255f, 1f); // brown dark
	public Color defaultPlaceholderColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 128f / 255f); // grey dark transparent
	public Color defaultNormalColor_Dropdown = Color.white;
	public Color defaultNormalColor_Inputfield = Color.white;
	public Color defaultSelectedColor_Inputfield = new Color(200f / 255f, 200f / 255f, 200f / 255f, 1f); // grey light
	public Color defaultSelectionColor_Inputfield = new Color(168f / 255f, 206f / 255f, 1f, 192f / 255f); // blue light transparent
	public Color defaultCaretColor_Inputfield = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f); // frey dark
	public Color defaultNormalColor_Button = new Color(1f, 1f, 1f, 0f); // transparent
	public Color defaultHighlightedColor = new Color(1f, 178f / 255f, 56f / 255f, 1f); // orange light
	public Color defaultPressedColor = new Color(194f / 255f, 94f / 255f, 0f, 1f); // brown
	public Color defaultSelectedColor = new Color(223f / 255f, 127f / 255f, 2f / 255f, 1f); // orange
	public Color defaultDisabledColor = new Color(187f / 255f, 187f / 255f, 187f / 255f, 128f / 255f); // grey transparent
	public Color defaultColor_Icon = Color.black; // black
	public Color defaultColor_Panel1 = Color.white;
	public Color defaultColor_Panel2 = new Color(1f, 178f / 255f, 56f / 255f, 1f); // orange light
	public Color defaultColor_Panel3 = new Color(179f / 255f, 179f / 255f, 179f / 255f, 1f); // grey light
	public Color defaultColor_PanelTexture = new Color(0f, 0f, 0f, 7f / 255f); // black transparent
	public Color defaultColor_Border = new Color(77f / 255f, 77f / 255f, 77f / 255f, 1f); // grey dark
	public int defaultBorderThickness = 1;
	public Color defaultNormalColor_Scrollbar = new Color(223f / 255f, 127f / 255f, 2f / 255f, 1f); // orange
	public Color defaultBackgroundColor_Scrollbar = new Color(1f, 178f / 255f, 56f / 255f, 1f); // orange light
	public Color defaultBackgroundColor_Scrollview = new Color(1f, 1f, 1f, 0f); // transparent
	public Color defaultNormalColor_Toggle = Color.white;
	public Color defaultColor_Tooltip = Color.white;
	public Color defaultPlayButtonColor = new Color(30f / 255f, 93f / 255f, 19f / 255f, 1f); // dark green
	public Color defaultPauseButtonColor = new Color(10f / 255f, 76f / 255f, 199f / 255f, 1f); // dark blue
	public Color defaultStopButtonColor = new Color(173f / 255f, 11f / 255f, 11f / 255f, 1f); // dark red
	public Color defaultActionBlockColor = new Color(170f / 255f, 128f / 255f, 1f, 1f); // light purple
	public Color defaultControlBlockColor = new Color(253f / 255f, 99f / 255f, 195f / 255f, 1f); // pink
	public Color defaultOperatorBlockColor = new Color(89f / 255f, 192f / 255f, 89f / 255f, 1f); // green
	public Color defaultCaptorBlockColor = new Color(92f / 255f, 177f / 255f, 214f / 255f, 1f); // light blue
	public Color defaultDropAreaColor = new Color(114f / 255f, 1f, 121f / 255f, 1f); // green
	public Color defaultHighlightingColor = new Color(1f, 1f, 0f, 1f); // yellow



	public int currentQuality;
	public int currentInteractionMode;
	public float currentUIScale;
	public int currentWallTransparency;
	public int currentGameView;
	public int currentTooltipView;
	public int currentFont;
	public int currentCaretWidth;
	public int currentCaretHeight;
	public int currentCharSpacing;
	public int currentWordSpacing;
	public int currentLineSpacing;
	public int currentParagraphSpacing;
	public Color currentNormalColor_Text;
	public Color currentSelectedColor_Text;
	public Color currentPlaceholderColor;
	public Color currentNormalColor_Dropdown;
	public Color currentNormalColor_Inputfield;
	public Color currentSelectedColor_Inputfield;
	public Color currentSelectionColor_Inputfield;
	public Color currentCaretColor_Inputfield;
	public Color currentNormalColor_Button;
	public Color currentHighlightedColor;
	public Color currentPressedColor;
	public Color currentSelectedColor;
	public Color currentDisabledColor;
	public Color currentColor_Icon;
	public Color currentColor_Panel1;
	public Color currentColor_Panel2;
	public Color currentColor_Panel3;
	public Color currentColor_PanelTexture;
	public Color currentColor_Border;
	public int currentBorderThickness;
	public Color currentNormalColor_Scrollbar;
	public Color currentBackgroundColor_Scrollbar;
	public Color currentBackgroundColor_Scrollview;
	public Color currentNormalColor_Toggle;
	public Color currentColor_Tooltip;
	public Color currentPlayButtonColor;
	public Color currentPauseButtonColor;
	public Color currentStopButtonColor;
	public Color currentActionBlockColor;
	public Color currentControlBlockColor;
	public Color currentOperatorBlockColor;
	public Color currentCaptorBlockColor;
	public Color currentDropAreaColor; 
	public Color currentHighlightingColor;

	public TMP_Text currentSizeText;
}
