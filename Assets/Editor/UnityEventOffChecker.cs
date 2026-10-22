// À placer dans un dossier "Editor" (ex : Assets/Editor/UnityEventOffChecker.cs)
// Utilisation : Tools → Vérifier les UnityEvents en Off
// Lecture seule : aucune scène ni aucun prefab n'est modifié.
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

public static class UnityEventOffChecker
{
    const string CallsSuffix = "m_PersistentCalls.m_Calls";

    [MenuItem("Tools/Vérifier les UnityEvents en Off")]
    static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var setup = EditorSceneManager.GetSceneManagerSetup();
        int problems = 0;

        try
        {
            // Prefabs
            foreach (var path in Find("t:Prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) problems += CheckHierarchy(prefab, path, pingable: true);
            }

            // Scènes
            var scenes = Find("t:Scene");
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Vérification des UnityEvents", scenes[i], (float)i / scenes.Length);
                var scene = EditorSceneManager.OpenScene(scenes[i], OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                    problems += CheckHierarchy(root, scenes[i], pingable: false);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        if (problems == 0)
            Debug.Log("[OffChecker] Aucun listener en Off.");
        else
            Debug.LogWarning($"[OffChecker] {problems} listener(s) en Off trouvé(s), voir la liste ci-dessus.");
    }

    static string[] Find(string filter) =>
        AssetDatabase.FindAssets(filter, new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).ToArray();

    static int CheckHierarchy(GameObject root, string assetPath, bool pingable)
    {
        int problems = 0;
        foreach (var comp in root.GetComponentsInChildren<Component>(true))
        {
            if (comp == null) continue;

            var it = new SerializedObject(comp).GetIterator();
            while (it.Next(true))
            {
                if (!it.isArray || it.propertyType != SerializedPropertyType.Generic ||
                    !it.propertyPath.EndsWith(CallsSuffix)) continue;

                for (int i = 0; i < it.arraySize; i++)
                {
                    var call = it.GetArrayElementAtIndex(i);
                    if (call.FindPropertyRelative("m_CallState").intValue != (int)UnityEventCallState.Off) continue;

                    var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
                    var method = call.FindPropertyRelative("m_MethodName").stringValue;
                    string listener = target == null && string.IsNullOrEmpty(method)
                        ? "entrée vide"
                        : $"{(target != null ? target.GetType().Name : "<cible manquante>")}.{method}";
                    string eventName = it.propertyPath.Replace("." + CallsSuffix, "");

                    // Le clic sur le message sélectionne l'objet pour les prefabs ;
                    // les objets de scène sont déchargés après l'analyse, d'où le chemin complet.
                    Debug.LogWarning(
                        $"[OffChecker] {assetPath} → {GetPath(comp.transform)} ({comp.GetType().Name}) → {eventName}[{i}] : {listener}",
                        pingable ? comp : null);
                    problems++;
                }
            }
        }
        return problems;
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}