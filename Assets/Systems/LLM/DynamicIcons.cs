using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class InlineActorTag : MonoBehaviour
{
    [System.Serializable]
    public struct ActorPlacement
    {
        public string tagName;
        public GameObject prefab;
    }

    public TextMeshProUGUI tmpText;
    public ActorPlacement[] actors;

    // Maps tag name → char index in the stripped string
    private Dictionary<int, string> _tagPositions = new();
    private List<GameObject> _spawned = new();

    void Start()
    {
        string raw = tmpText.text;
        Debug.Log($"Raw text: {raw}");

        string stripped = ParseTextAndRecordTags(raw, out _tagPositions);
        Debug.Log($"Processed text: {stripped}");

        Debug.Log($"Stripped text: {stripped}");
        Debug.Log($"Tags found: {_tagPositions.Count}");
        foreach (var kvp in _tagPositions)
        {
            Debug.Log($"Tag '{kvp.Value}' at char index {kvp.Key}");
        }   

        tmpText.text = stripped;
        tmpText.ForceMeshUpdate();

        StartCoroutine(PlaceActors());
    }

    string ParseTextAndRecordTags(string input, out Dictionary<int, string> positions)
    {
        positions = new Dictionary<int, string>();

        // This regex captures either standard TMP rich text tags OR your custom upper-case actor tags
        // Group 1: Rich text tags like <b>, </b>, <color=red>
        // Group 2: Your custom tags like <HERO>, <ENEMY_1>
        var regex = new Regex(@"(<\/?[a-z#][^>]*>)|<([A-Z][^>]*)>");

        var resultBuilder = new StringBuilder();
        int visualCharacterIndex = 0;
        int lastEnd = 0;

        foreach (Match match in regex.Matches(input))
        {
            // Process the plain text before this tag
            string textBefore = input.Substring(lastEnd, match.Index - lastEnd);
            resultBuilder.Append(textBefore);
            visualCharacterIndex += textBefore.Length;

            if (match.Groups[1].Success)
            {
                // It's a standard rich text tag (e.g., <b>)
                // Append it to the final text, but DO NOT increase visualCharacterIndex 
                // because TMP won't count this as a rendered character.
                resultBuilder.Append(match.Groups[1].Value);
            }
            else if (match.Groups[2].Success)
            {
                // It's your custom actor tag (e.g., <HERO>)
                string tagName = match.Groups[2].Value;

                // Record the location where the icon will go based on the true visual index
                positions[visualCharacterIndex] = tagName;

                // Replace the tag with non-breaking spaces so TMP leaves a gap
                string placeholder = new string('\u00A0', match.Length);
                resultBuilder.Append(placeholder);
                visualCharacterIndex += placeholder.Length;
            }

            lastEnd = match.Index + match.Length;
        }

        // Append any remaining trailing text
        resultBuilder.Append(input.Substring(lastEnd));
        return resultBuilder.ToString();
    }
    string StripTagsAndRecord(string input, out Dictionary<int, string> positions)
    {
        positions = new Dictionary<int, string>();
        var result = new System.Text.StringBuilder();
        int charIndex = 0;

        var regex = new Regex(@"<([A-Z][^>]*)>");
        int lastEnd = 0;

        foreach (Match match in regex.Matches(input))
        {
            string before = input.Substring(lastEnd, match.Index - lastEnd);
            result.Append(before);
            charIndex += before.Length;

            positions[charIndex] = match.Groups[1].Value;

            string placeholder = new string('\u00A0', match.Length);
            result.Append(placeholder);
            charIndex += placeholder.Length;

            lastEnd = match.Index + match.Length;
        }

        result.Append(input.Substring(lastEnd));
        return result.ToString();
    }

    string GetRelativePath(Transform root, Transform target)
    {
        var parts = new List<string>();
        var current = target;
        while (current != root)
        {
            parts.Insert(0, current.name);
            current = current.parent;
        }
        return string.Join("/", parts);
    }

    public void showCharPos()
    {
        TMP_CharacterInfo charInfo2 = tmpText.textInfo.characterInfo[0];
        // Coins du caractère
        Vector3 bottomLeft = charInfo2.bottomLeft;
        Vector3 topRight = charInfo2.topRight;
        Debug.Log($"First character corners: bottomLeft=({bottomLeft.x}, {bottomLeft.y}), topRight=({topRight.x}, {topRight.y})");
        Vector3 center = (bottomLeft + topRight) / 2f;
        Vector3 worldPos = tmpText.transform.TransformPoint(center);
        Debug.Log("First character in world space: " + worldPos);
        Debug.Log("World position: " + worldPos);
    }

    private Transform FindChildDeep(Transform parent, string targetName)
    {
        // Check if the current parent has a direct child with the target name
        Transform result = parent.Find(targetName);
        if (result != null) return result;

        // If not found, loop through all children and search deeper
        for (int i = 0; i < parent.childCount; i++)
        {
            result = FindChildDeep(parent.GetChild(i), targetName);
            if (result != null) return result;
        }

        return null; // Not found in this branch
    }

    private IEnumerator PlaceActors()
    {
        yield return null; 
        tmpText.ForceMeshUpdate();
        var textInfo = tmpText.textInfo;

        string gridPath = "MainCanvas/LeftPanel/LibraryCanvas/LibraryScrollView/Viewport/LibraryPanel";
        GameObject gridObject = GameObject.Find(gridPath);

        if (gridObject == null)
        {
            Debug.LogError($"Could not find the Grid at path: {gridPath}");
            yield break;
        }

        Transform gridTransform = gridObject.transform;

        foreach (var (charIndex, tagName) in _tagPositions)
        {
            // Validate that the character index actually exists in TMP's current text info layout
            if (charIndex >= textInfo.characterCount)
            {
                Debug.LogWarning($"Char index {charIndex} is out of bounds for current textInfo (Count: {textInfo.characterCount}).");
                continue;
            }

            GameObject prefab = null;
            foreach (var a in actors)
            {
                if (a.tagName == tagName) { prefab = a.prefab; break; }
            }

            if (prefab == null)
            {
                // 1. Get all Transform components in the entire hierarchy
                Transform[] allChildren = gridTransform.GetComponentsInChildren<Transform>(true);

                // 2. Look for the one with the matching name
                foreach (Transform child in allChildren)
                {
                    if (child.name == tagName)
                    {
                        prefab = child.gameObject;
                        break;
                    }
                }
            }

            if (prefab == null)
            {
                Debug.LogWarning($"Actor prefab or scene object for tag '{tagName}' could not be found.");
                continue;
            }

            var charInfo = textInfo.characterInfo[charIndex];
            
            // Double check if the character is actually visible/rendered (handles unexpected whitespaces smoothly)
            if (!charInfo.isVisible && charInfo.character != '\u00A0') continue;

            var ActionIcon = Instantiate(prefab, tmpText.transform);

            ActionIcon.SetActive(true);

            // Clean up Amount UI if exists
            Transform amountChild = ActionIcon.transform.Find("Amount");
            if (amountChild != null) Destroy(amountChild.gameObject);

            var rect = ActionIcon.GetComponent<RectTransform>();

            // Calculate exact middle point of the space character
            Vector3 center = (charInfo.bottomLeft + charInfo.topRight) / 2f;
            Vector3 worldPos = tmpText.transform.TransformPoint(center);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f); // Changed pivot to 0.5, 0.5 for perfect centering over the gap
            rect.anchoredPosition = tmpText.transform.InverseTransformPoint(worldPos);
            rect.anchoredPosition += new Vector2(15f, 5f);

            rect.sizeDelta = new Vector2(25f, 25f);

            // Sync selectables, UI text elements, and block events (rest of your original logic)
            var srcSelectable = prefab.GetComponent<Selectable>();
            var dstSelectable = ActionIcon.GetComponent<Selectable>();
            if (srcSelectable != null && dstSelectable != null)
            {
                dstSelectable.interactable = false;
                ColorBlock colors = srcSelectable.colors;
                colors.disabledColor = colors.normalColor;
                dstSelectable.colors = colors;
            }

            var dstEventTrigger = ActionIcon.GetComponent<EventTrigger>();
            if (dstEventTrigger != null) dstEventTrigger.enabled = false;

            rect.SetAsLastSibling();
            _spawned.Add(ActionIcon);
        }
    }
}