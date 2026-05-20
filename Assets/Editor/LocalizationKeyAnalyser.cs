using UnityEngine;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

public class LocalizationKeyAnalyzer : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> unusedKeys = new List<string>();
    private List<string> usedKeys = new List<string>();
    private bool isAnalyzing = false;
    private float progress = 0f;
    private string statusMessage = "";
    private int totalKeys = 0;
    private int totalScenes = 0;
    private int totalPrefabs = 0;

    [MenuItem("Tools/Localization/Analyze Unused Keys")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationKeyAnalyzer>("Localization Analyzer");
    }

    void OnGUI()
    {
        GUILayout.Label("Localization Key Analyzer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (isAnalyzing)
        {
            EditorGUI.ProgressBar(
                new Rect(10, 40, position.width - 20, 20),
                progress,
                statusMessage
            );
            EditorGUILayout.Space(30);
            GUI.enabled = false;
        }

        if (GUILayout.Button("Analyze All Keys", GUILayout.Height(30)))
        {
            AnalyzeKeys();
        }

        GUI.enabled = true;
        EditorGUILayout.Space();

        if (totalKeys > 0)
        {
            EditorGUILayout.LabelField($"Total Keys: {totalKeys}");
            EditorGUILayout.LabelField($"Used Keys: {usedKeys.Count}");
            EditorGUILayout.LabelField($"Unused Keys: {unusedKeys.Count}");
            EditorGUILayout.LabelField($"Scenes Analyzed: {totalScenes}");
            EditorGUILayout.LabelField($"Prefabs Analyzed: {totalPrefabs}");
        }

        EditorGUILayout.Space();

        if (unusedKeys.Count > 0)
        {
            if (GUILayout.Button("Export Unused Keys to CSV"))
            {
                ExportToCSV();
            }

            if (GUILayout.Button("Copy Unused Keys to Clipboard"))
            {
                CopyToClipboard();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Unused Keys:", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (string key in unusedKeys)
            {
                EditorGUILayout.SelectableLabel(key, GUILayout.Height(18));
            }
            EditorGUILayout.EndScrollView();
        }
    }

    async void AnalyzeKeys()
    {
        isAnalyzing = true;
        progress = 0f;
        unusedKeys.Clear();
        usedKeys.Clear();

        try
        {
            await AnalyzeKeysAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during analysis: {e.Message}");
        }
        finally
        {
            isAnalyzing = false;
            Repaint();
        }
    }

    async Task AnalyzeKeysAsync()
    {
        // 1. Récupérer toutes les clés de la table de localisation
        statusMessage = "Loading localization tables...";
        Repaint();
        await Task.Delay(10);

        HashSet<string> allKeys = GetAllLocalizationKeys();
        totalKeys = allKeys.Count;

        if (allKeys.Count == 0)
        {
            statusMessage = "No localization keys found!";
            Debug.LogWarning("No localization keys found in the project.");
            return;
        }

        HashSet<string> keysFoundInProject = new HashSet<string>();

        // 2. Analyser toutes les scènes
        progress = 0.1f;
        statusMessage = "Analyzing scenes...";
        Repaint();

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        totalScenes = sceneGuids.Length;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            statusMessage = $"Analyzing scene {i + 1}/{sceneGuids.Length}: {System.IO.Path.GetFileName(scenePath)}";
            progress = 0.1f + (0.4f * i / sceneGuids.Length);
            Repaint();

            await AnalyzeSceneAsync(scenePath, keysFoundInProject);
        }

        // 3. Analyser tous les prefabs
        progress = 0.5f;
        statusMessage = "Analyzing prefabs...";
        Repaint();

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        totalPrefabs = prefabGuids.Length;

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            statusMessage = $"Analyzing prefab {i + 1}/{prefabGuids.Length}: {System.IO.Path.GetFileName(prefabPath)}";
            progress = 0.5f + (0.4f * i / prefabGuids.Length);
            Repaint();

            await AnalyzePrefabAsync(prefabPath, keysFoundInProject);

            // Yield pour éviter de bloquer l'éditeur
            if (i % 50 == 0)
            {
                await Task.Delay(1);
            }
        }

        // 4. Analyser les scripts C#
        progress = 0.9f;
        statusMessage = "Analyzing C# scripts...";
        Repaint();

        await AnalyzeScriptsAsync(keysFoundInProject);

        // 5. Calculer les clés inutilisées
        progress = 0.95f;
        statusMessage = "Computing unused keys...";
        Repaint();

        usedKeys = keysFoundInProject.ToList();
        unusedKeys = allKeys.Except(keysFoundInProject).OrderBy(k => k).ToList();

        progress = 1f;
        statusMessage = $"Analysis complete! Found {unusedKeys.Count} unused keys.";

        Debug.Log($"Analysis complete:");
        Debug.Log($"- Total keys: {totalKeys}");
        Debug.Log($"- Used keys: {usedKeys.Count}");
        Debug.Log($"- Unused keys: {unusedKeys.Count}");
        Debug.Log($"- Scenes analyzed: {totalScenes}");
        Debug.Log($"- Prefabs analyzed: {totalPrefabs}");
    }

    HashSet<string> GetAllLocalizationKeys()
    {
        HashSet<string> keys = new HashSet<string>();

        // Méthode 1 : Récupérer toutes les String Table Collections
        var collections = LocalizationEditorSettings.GetStringTableCollections();

        if (collections == null || collections.Count == 0)
        {
            Debug.LogWarning("No String Table Collections found!");
            return keys;
        }

        foreach (var collection in collections)
        {
            if (collection == null) continue;

            // Récupérer les clés depuis SharedData
            var sharedData = collection.SharedData;
            if (sharedData != null)
            {
                foreach (var entry in sharedData.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Key))
                    {
                        keys.Add(entry.Key);
                    }
                }
            }
        }

        return keys;
    }

    async Task AnalyzeSceneAsync(string scenePath, HashSet<string> foundKeys)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        await Task.Delay(1); // Yield

        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            AnalyzeGameObject(root, foundKeys);
        }

        EditorSceneManager.CloseScene(scene, true);
    }

    async Task AnalyzePrefabAsync(string prefabPath, HashSet<string> foundKeys)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab != null)
        {
            AnalyzeGameObject(prefab, foundKeys);
        }

        await Task.Yield();
    }

    void AnalyzeGameObject(GameObject obj, HashSet<string> foundKeys)
    {
        // LocalizeStringEvent (le plus courant)
        var localizeStringEvents = obj.GetComponentsInChildren<LocalizeStringEvent>(true);
        foreach (var localizeEvent in localizeStringEvents)
        {
            if (localizeEvent.StringReference != null &&
                !string.IsNullOrEmpty(localizeEvent.StringReference.TableEntryReference.Key))
            {
                foundKeys.Add(localizeEvent.StringReference.TableEntryReference.Key);
            }
        }

        // LocalizedString dans les TextMeshPro
        var tmpTexts = obj.GetComponentsInChildren<TMP_Text>(true);
        foreach (var tmp in tmpTexts)
        {
            // Chercher les composants de localisation attachés
            var localizers = tmp.GetComponents<Component>();
            foreach (var component in localizers)
            {
                if (component == null) continue;

                var serializedObject = new SerializedObject(component);
                var property = serializedObject.GetIterator();

                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.String)
                    {
                        string value = property.stringValue;
                        if (!string.IsNullOrEmpty(value) && IsLikelyLocalizationKey(value))
                        {
                            foundKeys.Add(value);
                        }
                    }
                }
            }
        }

        // Unity UI Text
        var uiTexts = obj.GetComponentsInChildren<Text>(true);
        foreach (var text in uiTexts)
        {
            var localizers = text.GetComponents<Component>();
            foreach (var component in localizers)
            {
                if (component == null) continue;

                var serializedObject = new SerializedObject(component);
                var property = serializedObject.GetIterator();

                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.String)
                    {
                        string value = property.stringValue;
                        if (!string.IsNullOrEmpty(value) && IsLikelyLocalizationKey(value))
                        {
                            foundKeys.Add(value);
                        }
                    }
                }
            }
        }

        // Recherche générique dans tous les components
        var allComponents = obj.GetComponentsInChildren<Component>(true);
        foreach (var component in allComponents)
        {
            if (component == null) continue;

            // Vérifier si c'est un composant de localisation
            var type = component.GetType();
            if (type.Namespace != null && type.Namespace.Contains("Localization"))
            {
                ExtractKeysFromComponent(component, foundKeys);
            }
        }
    }

    void ExtractKeysFromComponent(Component component, HashSet<string> foundKeys)
    {
        var serializedObject = new SerializedObject(component);
        var property = serializedObject.GetIterator();

        while (property.Next(true))
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                string value = property.stringValue;
                if (!string.IsNullOrEmpty(value) && IsLikelyLocalizationKey(value))
                {
                    foundKeys.Add(value);
                }
            }
            else if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Vérifier les références aux tables de localisation
                var obj = property.objectReferenceValue;
                if (obj != null && obj.GetType().Name.Contains("LocalizedString"))
                {
                    // Extraire la clé via réflexion
                    var keyField = obj.GetType().GetField("m_TableEntryReference",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (keyField != null)
                    {
                        var keyValue = keyField.GetValue(obj);
                        if (keyValue != null)
                        {
                            var keyProp = keyValue.GetType().GetProperty("Key");
                            if (keyProp != null)
                            {
                                string key = keyProp.GetValue(keyValue) as string;
                                if (!string.IsNullOrEmpty(key))
                                {
                                    foundKeys.Add(key);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    async Task AnalyzeScriptsAsync(HashSet<string> foundKeys)
    {
        string[] scriptGuids = AssetDatabase.FindAssets("t:Script");

        for (int i = 0; i < scriptGuids.Length; i++)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuids[i]);

            if (scriptPath.EndsWith(".cs"))
            {
                string content = System.IO.File.ReadAllText(scriptPath);

                // Chercher les patterns communs de clés de localisation
                ExtractKeysFromScript(content, foundKeys);
            }

            if (i % 100 == 0)
            {
                await Task.Delay(1);
            }
        }
    }

    void ExtractKeysFromScript(string scriptContent, HashSet<string> foundKeys)
    {
        // Pattern 1: StringReference avec clé en dur
        // Ex: new TableReference("StringTable"), "MY_KEY"
        var pattern1 = @"""([A-Z_][A-Z0-9_]+)""";

        // Pattern 2: TableEntryReference
        // Ex: TableEntryReference = "MY_KEY"

        var matches = System.Text.RegularExpressions.Regex.Matches(scriptContent, pattern1);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            string potentialKey = match.Groups[1].Value;
            if (IsLikelyLocalizationKey(potentialKey))
            {
                foundKeys.Add(potentialKey);
            }
        }
    }

    bool IsLikelyLocalizationKey(string value)
    {
        // Heuristique : les clés sont souvent en UPPER_SNAKE_CASE
        // ou commencent par certains préfixes
        if (string.IsNullOrEmpty(value)) return false;
        if (value.Length < 3) return false;

        // Vérifier si c'est en majuscules avec underscores
        return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Z][A-Z0-9_]*$");
    }

    void ExportToCSV()
    {
        string path = EditorUtility.SaveFilePanel(
            "Export Unused Keys",
            "",
            "unused_localization_keys.csv",
            "csv"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Key");

            foreach (string key in unusedKeys)
            {
                csv.AppendLine(key);
            }

            System.IO.File.WriteAllText(path, csv.ToString());
            Debug.Log($"Exported {unusedKeys.Count} unused keys to {path}");
            EditorUtility.RevealInFinder(path);
        }
    }

    void CopyToClipboard()
    {
        string result = string.Join("\n", unusedKeys);
        EditorGUIUtility.systemCopyBuffer = result;
        Debug.Log($"Copied {unusedKeys.Count} unused keys to clipboard");
    }
}