/*
 * Flexible Color Picker
 * Free asset by Unity
 * Made by Ward "WARdd" Dehairs
 * contact at info@WARddDev.com
 * More info at www.WARddDev.com
 *
 * Last Updated 20/04/2022
 *
 * Additonal contributions by
 * Taha Sanli, ibrahimtahasanli@gmail.com
 *
 */

// Uncomment this line to switch from using UnityEngin.UI inputfield and dropdown to TMPro version
// #define TMPro

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// Main controller script for the flexible color picker system
/// </summary>
public class FlexibleColorPicker : MonoBehaviour
{

    /*----------------------------------------------------------
    * ----------------------- PARAMETERS -----------------------
    * ----------------------------------------------------------
    */

    //Unity connections
    [Tooltip("Connections to the FCP's picker images, this should not be adjusted unless in advanced use cases.")]
    [SerializeField]
    private Picker[] pickers;
    [Serializable]
    private struct Picker
    {
        public Image image;
    }
    private enum PickerType
    {
        Main, R, G, B, H, S, V, A, Preview, PreviewAlpha
    }

    [Tooltip("Connection to the FCP's hexadecimal input field.")]
    public TMP_InputField hexInput;

    private Canvas canvas;

    //private state
    private BufferedColor bufferedColor;
    private Picker focusedPicker;
    private PickerType focusedPickerType;
    private bool typeUpdate;
    private Image srcColorSelector;

    public ColorUpdateEvent onColorChange;

    [Serializable]
    public class ColorUpdateEvent : UnityEvent<Color> { }

    //constants
    private const float HUE_LOOP = 5.9999f;

    //advanced settings
    [Tooltip("More specific settings for color picker. Changes are not applied immediately, but require an FCP update to trigger.")]
    public AdvancedSettings advancedSettings;
    [Serializable]
    public class AdvancedSettings
    {

        public bool mainStatic = true;

        public PSettings r;
        public PSettings g;
        public PSettings b;
        public PSettings h;
        public PSettings s;
        public PSettings v;
        public PSettings a;

        [Serializable]
        public class PSettings
        {
            [Tooltip("Value can be used to restrict slider range")]
            public Vector2 range = new Vector2(0f, 1f);
            [Tooltip("Make the picker associated with this value act static, even in a dynamic color picker setup")]
            public bool overrideStatic = false;
        }

        /// <summary>
        /// Get PSettings for value associated with the given picker index.
        /// Returns default PSettings if none exists.
        /// </summary>
        public PSettings Get(int i)
        {
            if (i <= 0 | i > 7)
                return new PSettings();
            PSettings[] p = new PSettings[] { r, g, b, h, s, v, a };
            return p[i - 1];
        }
    }
    private AdvancedSettings avs => advancedSettings;







    /*----------------------------------------------------------
    * ------------------- MAIN COLOR GET/SET -------------------
    * ----------------------------------------------------------
    */

    public Color color
    {
        /* if called before init in Start(), the color state
         * is equivalent to the starting color parameter.
         */
        get
        {
            return bufferedColor.color;
        }
        set
        {
            bufferedColor.Set(value);
            UpdateMarkers();
            UpdateTextures();
            UpdateHex();
            typeUpdate = true;
            onColorChange.Invoke(value);
        }
    }

    // See ColorSelectors in SettingsWindow prefab
    public void SetColorSelector(Image colorSelector)
    {
        srcColorSelector = colorSelector;
    }

    // See OKButton in FlexibleColorPicker prefab
    public void CloseFCB()
    {
        if (srcColorSelector != null)
        {
            srcColorSelector.GetComponentInParent<CanvasGroup>().interactable = true;
            EventSystem.current.SetSelectedGameObject(srcColorSelector.gameObject);
        }
    }






    /*----------------------------------------------------------
    * ------------------------- UPKEEP -------------------------
    * ----------------------------------------------------------
    */

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        if (this.bufferedColor == null)
            this.bufferedColor = new BufferedColor();
        
        EventSystem.current.SetSelectedGameObject(hexInput.gameObject);

        if (srcColorSelector != null)
            color = srcColorSelector.color;

        UpdateTextures();
        UpdateMarkers();
        UpdateHex();
    }
    private void Update()
    {
        typeUpdate = false;
    }

    /// <summary>
    /// Change picker that is being focused (and edited) using the pointer.
    /// </summary>
    /// <param name="i">Index of the picker image.</param>
    public void SetPointerFocus(int i)
    {
        if (i < 0 || i >= pickers.Length)
            Debug.LogWarning("No picker image available of type " + (PickerType)i +
                ". Did you assign all the picker images in the editor?");
        else
            focusedPicker = pickers[i];
        focusedPickerType = (PickerType)i;
    }

    /// <summary>
    /// Update color based on the pointer position in the currently focused picker.
    /// </summary>
    /// <param name="e">Pointer event</param>
    public void PointerUpdate(BaseEventData e)
    {
        Vector2 v = GetNormalizedPointerPosition(canvas, focusedPicker.image.rectTransform, e);
        this.bufferedColor = PickColor(this.bufferedColor, focusedPickerType, v);

        UpdateMarkers();
        UpdateTextures();

        typeUpdate = true;
        UpdateHex();
        onColorChange.Invoke(bufferedColor.color);
    }

    /// <summary>
    /// Softly sanitize hex color input and apply it
    /// </summary>
    public void TypeHex(string input)
    {
        TypeHex(input, false);

        UpdateTextures();
        UpdateMarkers();
    }

    /// <summary>
    /// Strongly sanitize hex color input and apply it.
    /// (appends zeroes to fit proper length in the text box).
    /// </summary>
    public void FinishTypeHex(string input)
    {
        TypeHex(input, true);

        UpdateTextures();
        UpdateMarkers();
    }








    /*----------------------------------------------------------
    * --------------------- COLOR PICKING ----------------------
    * ----------------------------------------------------------
    * 
    * Get a new color that is the currently selected color but with 
    * one or two values changed. This is the core functionality of 
    * the picking images and the entire color picker script.
    */

    /// <summary>
    /// Get a color that is the current color, but changed by the given picker and value.
    /// </summary>
    /// <param name="type">Picker type to base change on</param>
    /// <param name="v">normalized x and y values (both values may not be used)</param>
    private BufferedColor PickColor(BufferedColor color, PickerType type, Vector2 v)
    {
        switch (type)
        {
            case PickerType.Main: return PickColorMain(color, v);

            case PickerType.Preview:
            case PickerType.PreviewAlpha:
                return color;

            default: return PickColor1D(color, type, v);
        }
    }

    private BufferedColor PickColorMain(BufferedColor color, Vector2 v)
    {
        return PickColor2D(color, PickerType.S, v.x, PickerType.V, v.y);
    }

    private BufferedColor PickColor1D(BufferedColor color, PickerType type, Vector2 v)
    {
        bool horizontal = IsHorizontal(pickers[(int)type]);
        float value = horizontal ? v.x : v.y;
        return PickColor1D(color, type, value);
    }

    private BufferedColor PickColor2D(BufferedColor color, PickerType type1, float value1, PickerType type2, float value2)
    {
        color = PickColor1D(color, type1, value1);
        color = PickColor1D(color, type2, value2);
        return color;
    }

    private BufferedColor PickColor1D(BufferedColor color, PickerType type, float value)
    {
        var ps = avs.Get((int)type);
        value = Mathf.Lerp(ps.range.x, ps.range.y, value);

        switch (type)
        {
            case PickerType.R: return color.PickR(value);
            case PickerType.G: return color.PickG(value);
            case PickerType.B: return color.PickB(value);
            case PickerType.H: return color.PickH(value * HUE_LOOP);
            case PickerType.S: return color.PickS(value);
            case PickerType.V: return color.PickV(value);
            case PickerType.A: return color.PickA(value);
            default:
                throw new Exception("Picker type " + type + " is not associated with a single color value.");
        }
    }










    /*----------------------------------------------------------
    * -------------------- MARKER UPDATING ---------------------
    * ----------------------------------------------------------
    * 
    * Update positions of markers on each picking texture, 
    * indicating the currently selected values.
    */


    private void UpdateMarkers()
    {
        for (int i = 0; i < pickers.Length; i++)
        {
            if (IsPickerAvailable(i))
            {
                PickerType type = (PickerType)i;
                Vector2 v = GetValue(type);
                UpdateMarker(pickers[i], type, v);
            }
        }
    }

    private void UpdateMarker(Picker picker, PickerType type, Vector2 v)
    {
        switch (type)
        {
            case PickerType.Main:
                SetMarker(picker.image, v, true, true);
                break;

            case PickerType.Preview:
            case PickerType.PreviewAlpha:
                break;

            default:
                bool horizontal = IsHorizontal(picker);
                SetMarker(picker.image, v, horizontal, !horizontal);
                break;
        }
    }

    private void SetMarker(Image picker, Vector2 v, bool setX, bool setY)
    {
        RectTransform marker = null;
        RectTransform offMarker = null;
        if (setX && setY)
            marker = GetMarker(picker, null);
        else if (setX)
        {
            marker = GetMarker(picker, "hor");
            offMarker = GetMarker(picker, "ver");
        }
        else if (setY)
        {
            marker = GetMarker(picker, "ver");
            offMarker = GetMarker(picker, "hor");
        }
        if (offMarker != null)
            offMarker.gameObject.SetActive(false);

        if (marker == null)
            return;

        marker.gameObject.SetActive(true);
        RectTransform parent = picker.rectTransform;
        Vector2 parentSize = parent.rect.size;
        Vector2 localPos = marker.localPosition;

        if (setX)
            localPos.x = (v.x - parent.pivot.x) * parentSize.x;
        if (setY)
            localPos.y = (v.y - parent.pivot.y) * parentSize.y;
        marker.localPosition = localPos;
    }

    private RectTransform GetMarker(Image picker, string search)
    {
        for (int i = 0; i < picker.transform.childCount; i++)
        {
            RectTransform candidate = picker.transform.GetChild(i).GetComponent<RectTransform>();
            string candidateName = candidate.name.ToLower();
            bool match = candidateName.Contains("marker");
            match &= string.IsNullOrEmpty(search)
                || candidateName.Contains(search);
            if (match)
                return candidate;
        }
        return null;
    }











    /*----------------------------------------------------------
    * -------------------- VALUE RETRIEVAL ---------------------
    * ----------------------------------------------------------
    * 
    * Get individual values associated with a picker image from the 
    * currently selected color.
    * This is needed to properly update markers.
    */

    private Vector2 GetValue(PickerType type)
    {
        switch (type)
        {

            case PickerType.Main: return new Vector2(GetValue1D(PickerType.S), GetValue1D(PickerType.V));

            case PickerType.Preview:
            case PickerType.PreviewAlpha:
                return Vector2.zero;

            default:
                float value = GetValue1D(type);
                return new Vector2(value, value);

        }
    }

    /// <summary>
    /// Get normalized value of the current color according to the given picker.
    /// This value can be used to adjust the position of the marker on a slider.
    /// </summary>
    private float GetValue1D(PickerType type)
    {
        var ps = avs.Get((int)type);
        float min = ps.range.x;
        float max = ps.range.y;

        switch (type)
        {
            case PickerType.R: return Mathf.InverseLerp(min, max, bufferedColor.r);
            case PickerType.G: return Mathf.InverseLerp(min, max, bufferedColor.g);
            case PickerType.B: return Mathf.InverseLerp(min, max, bufferedColor.b);
            case PickerType.H: return Mathf.InverseLerp(min, max, bufferedColor.h / HUE_LOOP);
            case PickerType.S: return Mathf.InverseLerp(min, max, bufferedColor.s);
            case PickerType.V: return Mathf.InverseLerp(min, max, bufferedColor.v);
            case PickerType.A: return Mathf.InverseLerp(min, max, bufferedColor.a);
            default:
                throw new Exception("Picker type " + type + " is not associated with a single color value.");
        }
    }









    /*----------------------------------------------------------
    * -------------------- TEXTURE UPDATING --------------------
    * ----------------------------------------------------------
    * 
    * Update picker image textures that show gradients of colors 
    * that the user can pick.
    */

    private void UpdateTextures()
    {
        foreach (PickerType type in Enum.GetValues(typeof(PickerType)))
        {
            UpdateStatic(type);
        }
    }

    private void UpdateStatic(PickerType type)
    {
        if (!IsPickerAvailable(type))
            return;
        Picker p = pickers[(int)type];

        Color prvw = color;

        switch (type)
        {

            case PickerType.Preview:
                prvw.a = 1f;
                p.image.color = prvw;
                break;

            case PickerType.Main:
            case PickerType.S:
            case PickerType.V:
                p.image.color = HSVToRGB(GetValue1D(PickerType.H) * HUE_LOOP, 1f, 1f);
                break;

            case PickerType.A:
            case PickerType.PreviewAlpha:
                p.image.color = prvw;
                break;
        }

        if (srcColorSelector != null)
            srcColorSelector.color = color;
    }

    private bool IsPickerAvailable(PickerType type)
    {
        return IsPickerAvailable((int)type);
    }

    private bool IsPickerAvailable(int index)
    {
        if (index < 0 || index >= pickers.Length)
            return false;
        Picker p = pickers[index];
        if (p.image == null || !p.image.gameObject.activeInHierarchy)
            return false;
        return true;
    }







    /*----------------------------------------------------------
    * ------------------ HEX INPUT UPDATING --------------------
    * ----------------------------------------------------------
    * 
    * Provides an input field for hexadecimal color values.
    * The user can type new values, or use this field to copy 
    * values picked via the picker images.
    */

    private void UpdateHex()
    {
        if (hexInput == null || !hexInput.gameObject.activeInHierarchy)
            return;
        hexInput.SetTextWithoutNotify("#" + ColorUtility.ToHtmlStringRGBA(this.color));
    }

    private void TypeHex(string input, bool finish)
    {
        if (typeUpdate)
            return;
        else
            typeUpdate = true;

        string newText = GetSanitizedHex(input, finish);
        string parseText = GetSanitizedHex(input, true);

        int cp = hexInput.caretPosition;
        hexInput.SetTextWithoutNotify(newText);
        if (hexInput.caretPosition == 0)
            hexInput.caretPosition = 1;
        else if (newText.Length == 2)
            hexInput.caretPosition = 2;
        else if (input.Length > newText.Length && cp < input.Length)
            hexInput.caretPosition = cp - input.Length + newText.Length;

        Color newColor;
        if (ColorUtility.TryParseHtmlString(parseText, out newColor))
        {
            bufferedColor.Set(newColor);
            UpdateMarkers();
            UpdateTextures();
            onColorChange.Invoke(newColor);
        }
    }






    /*----------------------------------------------------------
    * ---------------- STATIC HELPER FUNCTIONS -----------------
    * ----------------------------------------------------------
    */

    /// <summary>
    /// Should given picker image be controlled horizontally?
    /// Returns true if the image is bigger in the horizontal direction.
    /// </summary>
    private static bool IsHorizontal(Picker p)
    {
        Vector2 size = p.image.rectTransform.rect.size;
        return size.x >= size.y;
    }

    /// <summary>
    /// Sanitive a given string so that it encodes a valid hex color string
    /// </summary>
    /// <param name="input">Input string</param>
    /// <param name="full">Insert zeroes to match #RRGGBBAA format </param>
    public static string GetSanitizedHex(string input, bool full)
    {
        if (string.IsNullOrEmpty(input))
            return "#";

        List<char> toReturn = new List<char>();
        toReturn.Add('#');
        int i = 0;
        char[] chars = input.ToCharArray();
        while (toReturn.Count < 9 && i < input.Length)
        {
            char nextChar = char.ToUpper(chars[i++]);
            if (IsValidHexChar(nextChar))
                toReturn.Add(nextChar);
        }

        while (full && toReturn.Count < 9)
            //toReturn.Insert(1, '0');
            toReturn.Add('0');

        return new string(toReturn.ToArray());
    }

    private static bool IsValidHexChar(char c)
    {
        bool valid = char.IsNumber(c);
        valid |= c >= 'A' & c <= 'F';
        return valid;
    }

    /// <summary>
    /// Get normalized position of the given pointer event relative to the given rect.
    /// (e.g. return [0,1] for top left corner). This method correctly takes into 
    /// account relative positions, canvas render mode and general transformations, 
    /// including rotations and scale.
    /// </summary>
    /// <param name="canvas">parent canvas of the rect (and therefore the FCP)</param>
    /// <param name="rect">Rect to find relative position to</param>
    /// <param name="e">Pointer event for which to find the relative position</param>
    private static Vector2 GetNormalizedPointerPosition(Canvas canvas, RectTransform rect, BaseEventData e)
    {
        switch (canvas.renderMode)
        {

            case RenderMode.ScreenSpaceCamera:
                if (canvas.worldCamera == null)
                    return GetNormScreenSpace(rect, e);
                else
                    return GetNormWorldSpace(canvas, rect, e);

            case RenderMode.ScreenSpaceOverlay:
                return GetNormScreenSpace(rect, e);

            case RenderMode.WorldSpace:
                if (canvas.worldCamera == null)
                {
                    Debug.LogError("FCP in world space render mode requires an event camera to be set up on the parent canvas!");
                    return Vector2.zero;
                }
                return GetNormWorldSpace(canvas, rect, e);

            default: return Vector2.zero;

        }
    }

    /// <summary>
    /// Get normalized position in the case of a screen space (overlay) 
    /// type canvas render mode
    /// </summary>
    private static Vector2 GetNormScreenSpace(RectTransform rect, BaseEventData e)
    {
        Vector2 screenPoint = ((PointerEventData)e).position;
        Vector2 localPos = rect.worldToLocalMatrix.MultiplyPoint(screenPoint);
        float x = Mathf.Clamp01((localPos.x / rect.rect.size.x) + rect.pivot.x);
        float y = Mathf.Clamp01((localPos.y / rect.rect.size.y) + rect.pivot.y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Get normalized position in the case of a world space (or screen space camera) 
    /// type cavnvas render mode.
    /// </summary>
    private static Vector2 GetNormWorldSpace(Canvas canvas, RectTransform rect, BaseEventData e)
    {
        Vector2 screenPoint = ((PointerEventData)e).position;
        Ray pointerRay = canvas.worldCamera.ScreenPointToRay(screenPoint);
        Plane canvasPlane = new Plane(canvas.transform.forward, canvas.transform.position);
        float enter;
        canvasPlane.Raycast(pointerRay, out enter);
        Vector3 worldPoint = pointerRay.origin + enter * pointerRay.direction;
        Vector2 localPoint = rect.worldToLocalMatrix.MultiplyPoint(worldPoint);

        float x = Mathf.Clamp01((localPoint.x / rect.rect.size.x) + rect.pivot.x);
        float y = Mathf.Clamp01((localPoint.y / rect.rect.size.y) + rect.pivot.y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Get color from hue, saturation, value format
    /// </summary>
    /// <param name="h">hue value, ranging from 0 to 6; red to red</param>
    /// <param name="s">saturation, 0 to 1; gray to colored</param>
    /// <param name="v">value, 0 to 1; black to white</param>
    public static Color HSVToRGB(float h, float s, float v)
    {
        float c = s * v;
        float m = v - c;
        float x = c * (1f - Mathf.Abs(h % 2f - 1f)) + m;
        c += m;

        int range = Mathf.FloorToInt(h % 6f);

        switch (range)
        {
            case 0: return new Color(c, x, m);
            case 1: return new Color(x, c, m);
            case 2: return new Color(m, c, x);
            case 3: return new Color(m, x, c);
            case 4: return new Color(x, m, c);
            case 5: return new Color(c, m, x);
            default: return Color.black;
        }
    }

    /// <summary>
    /// Get hue, saturation and value of a color.
    /// Complementary to HSVToRGB
    /// </summary>
    public static Vector3 RGBToHSV(Color color)
    {
        float r = color.r;
        float g = color.g;
        float b = color.b;
        return RGBToHSV(r, g, b);
    }

    /// <summary>
    /// Get hue, saturation and value of a color.
    /// Complementary to HSVToRGB
    /// </summary>
    public static Vector3 RGBToHSV(float r, float g, float b)
    {
        float cMax = Mathf.Max(r, g, b);
        float cMin = Mathf.Min(r, g, b);
        float delta = cMax - cMin;
        float h = 0f;
        if (delta > 0f)
        {
            if (r >= b && r >= g)
                h = Mathf.Repeat((g - b) / delta, 6f);
            else if (g >= r && g >= b)
                h = (b - r) / delta + 2f;
            else if (b >= r && b >= g)
                h = (r - g) / delta + 4f;
        }
        float s = cMax == 0f ? 0f : delta / cMax;
        float v = cMax;
        return new Vector3(h, s, v);
    }








    /*----------------------------------------------------------
    * --------------------- HELPER CLASSES ---------------------
    * ----------------------------------------------------------
    */


    /// <summary>
    /// Encodes a color while buffering hue and saturation values.
    /// This is necessary since these values are singular for some 
    /// colors like unsaturated grays and would lead to undesirable 
    /// behaviour when moving sliders towards such colors.
    /// </summary>
    [Serializable]
    private class BufferedColor
    {
        public Color color;
        private float bufferedHue;
        private float bufferedSaturation;

        public float r { get { return color.r; } }
        public float g { get { return color.g; } }
        public float b { get { return color.b; } }
        public float a { get { return color.a; } }
        public float h { get { return bufferedHue; } }
        public float s { get { return bufferedSaturation; } }
        public float v { get { return RGBToHSV(color).z; } }


        public BufferedColor()
        {
            this.bufferedHue = 0f;
            this.bufferedSaturation = 0f;
            this.color = Color.black;
        }

        public BufferedColor(Color color) : this()
        {
            this.Set(color);
        }

        public BufferedColor(Color color, float hue, float sat) : this(color)
        {
            this.bufferedHue = hue;
            this.bufferedSaturation = sat;
        }

        public BufferedColor(Color color, BufferedColor source) :
            this(color, source.bufferedHue, source.bufferedSaturation)
        {
            this.Set(color);
        }

        public void Set(Color color)
        {
            this.Set(color, this.bufferedHue, this.bufferedSaturation);
        }

        public void Set(Color color, float bufferedHue, float bufferedSaturation)
        {
            this.color = color;
            Vector3 hsv = RGBToHSV(color);

            bool hueSingularity = hsv.y == 0f || hsv.z == 0f;
            if (hueSingularity)
                this.bufferedHue = bufferedHue;
            else
                this.bufferedHue = hsv.x;

            bool saturationSingularity = hsv.z == 0f;
            if (saturationSingularity)
                this.bufferedSaturation = bufferedSaturation;
            else
                this.bufferedSaturation = hsv.y;
        }

        public BufferedColor PickR(float value)
        {
            Color toReturn = this.color;
            toReturn.r = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickG(float value)
        {
            Color toReturn = this.color;
            toReturn.g = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickB(float value)
        {
            Color toReturn = this.color;
            toReturn.b = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickA(float value)
        {
            Color toReturn = this.color;
            toReturn.a = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickH(float value)
        {
            Vector3 hsv = RGBToHSV(this.color);
            Color toReturn = HSVToRGB(value, hsv.y, hsv.z);
            toReturn.a = this.color.a;
            return new BufferedColor(toReturn, value, bufferedSaturation);
        }

        public BufferedColor PickS(float value)
        {
            Vector3 hsv = RGBToHSV(this.color);
            Color toReturn = HSVToRGB(bufferedHue, value, hsv.z);
            toReturn.a = this.color.a;
            return new BufferedColor(toReturn, bufferedHue, value);
        }

        public BufferedColor PickV(float value)
        {
            Color toReturn = HSVToRGB(bufferedHue, bufferedSaturation, value);
            toReturn.a = this.color.a;
            return new BufferedColor(toReturn, bufferedHue, bufferedSaturation);
        }
    }
}
