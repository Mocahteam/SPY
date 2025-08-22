using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

public class LocalizationCleaner : EditorWindow
{
    private StringTableCollection stringTableCollection;
    private List<long> unusedKeys = new List<long>();
    private Vector2 scrollPosition;
    private bool showOnlyUnused = true;

    [MenuItem("Tools/Localization Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationCleaner>("Localization Cleaner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Localization Table Cleaner", EditorStyles.boldLabel);

        // S�lection de la table
        stringTableCollection = (StringTableCollection)EditorGUILayout.ObjectField(
            "String Table Collection",
            stringTableCollection,
            typeof(StringTableCollection),
            false
        );

        if (stringTableCollection == null)
        {
            EditorGUILayout.HelpBox("S�lectionnez une String Table Collection", MessageType.Info);
            return;
        }

        GUILayout.Space(10);

        // Boutons d'action
        if (GUILayout.Button("Analyser les cl�s inutilis�es"))
        {
            AnalyzeUnusedKeys();
        }

        if (unusedKeys.Count > 0)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField($"Cl�s inutilis�es trouv�es: {unusedKeys.Count}");

            showOnlyUnused = EditorGUILayout.Toggle("Afficher seulement les inutilis�es", showOnlyUnused);

            // Liste des cl�s
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            var keysToShow = showOnlyUnused ? unusedKeys : GetAllKeys();

            foreach (long key in keysToShow)
            {
                EditorGUILayout.BeginHorizontal();

                // Couleur diff�rente pour les cl�s inutilis�es
               /* if (unusedKeys.Contains(key))
                {
                    GUI.color = Color.red;
                }*/

                EditorGUILayout.LabelField(getKeyName(key));

                if (unusedKeys.Contains(key))
                {
                    GUI.color = Color.white;
                    if (GUILayout.Button("Supprimer", GUILayout.Width(80)))
                    {
                        RemoveKey(key);
                    }
                }

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // Bouton de suppression en masse
            if (GUILayout.Button("Supprimer toutes les cl�s inutilis�es"))
            {
                if (EditorUtility.DisplayDialog("Confirmation",
                    $"�tes-vous s�r de vouloir supprimer {unusedKeys.Count} cl�s ?",
                    "Oui", "Non"))
                {
                    RemoveAllUnusedKeys();
                }
            }
        }
    }


    private string getKeyName(long key)
    {
        foreach (var table in stringTableCollection.StringTables)
        {
            foreach (var entry in table.Values)
            {
                if (entry.KeyId == key)
                {
                    return entry.Key;
                }
            }
        }
        return key + " name not found";
    }
    private void AnalyzeUnusedKeys()
    {
        unusedKeys.Clear();

        var allKeys = GetAllKeys();
        var projectFiles = GetAllProjectFiles();

        foreach (long key in allKeys)
        {
            bool isUsed = false;

            // Rechercher dans tous les fichiers du projet
            foreach (string filePath in projectFiles)
            {
                if (IsKeyUsedInFile(key, filePath))
                {
                    isUsed = true;
                    break;
                }
            }

            if (!isUsed)
            {
                unusedKeys.Add(key);
            }
        }

        Debug.Log($"Analyse termin�e. {unusedKeys.Count} cl�s inutilis�es trouv�es.");
    }

    private List<long> GetAllKeys()
    {
        var keys = new List<long>();

        if (stringTableCollection != null)
        {
            foreach (var table in stringTableCollection.StringTables)
            {
                foreach (var entry in table.Values)
                {
                    if (!keys.Contains(entry.KeyId))
                    {
                        keys.Add(entry.KeyId);
                    }
                }
            }
        }

        return keys;
    }

    private List<string> GetAllProjectFiles()
    {
        var files = new List<string>();

        // Fichiers C#
        files.AddRange(Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories));

        // Fichiers prefabs et sc�nes
        files.AddRange(Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories));
        files.AddRange(Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories));

        // Fichiers UI (si vous utilisez UI Toolkit)
        files.AddRange(Directory.GetFiles(Application.dataPath, "*.uxml", SearchOption.AllDirectories));

        return files;
    }

    private bool IsKeyUsedInFile(long key, string filePath)
    {
        try
        {
            string content = File.ReadAllText(filePath);

            // Rechercher diff�rents patterns d'utilisation
            var patterns = new string[]
            {
                $"\"{key}\"",           // Usage direct en string
                $"'{key}'",             // Usage avec apostrophes
                $"m_KeyId: {key}",          // Dans les fichiers YAML/prefab
                $"m_TableEntryReference: {key}", // R�f�rence Unity
                ""+key                     // Recherche simple
            };

            return patterns.Any(pattern => content.Contains(pattern));
        }
        catch
        {
            return false;
        }
    }

    private void RemoveKey(long key)
    {
        if (stringTableCollection != null)
        {
            foreach (var table in stringTableCollection.StringTables)
            {
                if (table.Remove(key))
                {
                    EditorUtility.SetDirty(table);
                }
            }

            unusedKeys.Remove(key);
            AssetDatabase.SaveAssets();
        }
    }

    private void RemoveAllUnusedKeys()
    {
        var keysToRemove = new List<long>(unusedKeys);

        foreach (long key in keysToRemove)
        {
            RemoveKey(key);
        }

        Debug.Log($"{keysToRemove.Count} cl�s supprim�es.");
    }
}