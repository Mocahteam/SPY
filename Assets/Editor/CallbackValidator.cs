// À placer dans un dossier "Editor" (ex : Assets/Editor/UnityEventValidator.cs)
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class CallbackValidator
{
    const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance |
                               BindingFlags.Static | BindingFlags.FlattenHierarchy;

    [MenuItem("Tools/Vérifier les UnityEvents/Scènes ouvertes")]
    static void CheckOpenScenes()
    {
        int errors = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            foreach (var root in scene.GetRootGameObjects())
                errors += CheckGameObject(root, scene.path);
        }
        Report(errors);
    }

    [MenuItem("Tools/Vérifier les UnityEvents/Tout le projet")]
    static void CheckProject()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var setup = EditorSceneManager.GetSceneManagerSetup();
        int errors = 0;
        try
        {
            // Prefabs
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null) errors += CheckGameObject(go, path);
            }

            // ScriptableObjects
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (obj is ScriptableObject so) errors += CheckObject(so, path, so.name);
            }

            // Scènes
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                if (EditorUtility.DisplayCancelableProgressBar("Vérification des UnityEvents", path,
                        (float)i / sceneGuids.Length)) break;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                    errors += CheckGameObject(root, path);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
        Report(errors);
    }

    static int CheckGameObject(GameObject root, string assetPath)
    {
        int errors = 0;
        foreach (var comp in root.GetComponentsInChildren<Component>(true))
        {
            if (comp == null)
            {
                // Script manquant sur un GameObject : on le signale aussi
                continue;
            }
            errors += CheckObject(comp, assetPath, GetHierarchyPath(comp.transform) + " (" + comp.GetType().Name + ")");
        }
        return errors;
    }

    static int CheckObject(Object obj, string assetPath, string location)
    {
        int errors = 0;
        var so = new SerializedObject(obj);
        var prop = so.GetIterator();

        while (prop.Next(true))
        {
            if (!prop.isArray || prop.propertyType != SerializedPropertyType.Generic) continue;
            if (!prop.propertyPath.EndsWith("m_PersistentCalls.m_Calls")) continue;

            for (int i = 0; i < prop.arraySize; i++)
            {
                var call = prop.GetArrayElementAtIndex(i);
                var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
                var method = call.FindPropertyRelative("m_MethodName").stringValue;
                var mode = (PersistentListenerMode)call.FindPropertyRelative("m_Mode").intValue;
                var objArgTypeName = call.FindPropertyRelative("m_Arguments.m_ObjectArgumentAssemblyTypeName").stringValue;

                string eventName = prop.propertyPath.Replace(".m_PersistentCalls.m_Calls", "");
                string where = $"{assetPath} → {location} → {eventName} [listener {i}]";

                if (string.IsNullOrEmpty(method))
                {
                    Debug.LogWarning($"[UnityEvent] Listener sans fonction (No Function) : {where}", obj);
                    continue;
                }
                if (target == null)
                {
                    Debug.LogError($"[UnityEvent] Cible manquante pour '{method}' : {where}", obj);
                    errors++;
                    continue;
                }

                Type objArgType = string.IsNullOrEmpty(objArgTypeName) ? null : Type.GetType(objArgTypeName, false);
                if (!MethodExists(target.GetType(), method, mode, objArgType))
                {
                    Debug.LogError($"[UnityEvent] Méthode introuvable '{target.GetType().Name}.{method}' (mode {mode}) : {where}", obj);
                    errors++;
                }
            }
        }
        return errors;
    }

    static bool MethodExists(Type type, string name, PersistentListenerMode mode, Type objArgType)
    {
        var candidates = type.GetMethods(Flags).Where(m => m.Name == name).ToArray();
        if (candidates.Length == 0) return false;

        switch (mode)
        {
            case PersistentListenerMode.EventDefined:
                // Les paramètres dépendent du type d'événement : on vérifie juste le nom
                return true;
            case PersistentListenerMode.Void:
                return candidates.Any(m => m.GetParameters().Length == 0);
            case PersistentListenerMode.Object:
                var argType = objArgType ?? typeof(Object);
                return candidates.Any(m =>
                {
                    var p = m.GetParameters();
                    return p.Length == 1 && p[0].ParameterType.IsAssignableFrom(argType);
                });
            default:
                Type expected = mode switch
                {
                    PersistentListenerMode.Int => typeof(int),
                    PersistentListenerMode.Float => typeof(float),
                    PersistentListenerMode.String => typeof(string),
                    PersistentListenerMode.Bool => typeof(bool),
                    _ => null
                };
                return candidates.Any(m =>
                {
                    var p = m.GetParameters();
                    return p.Length == 1 && p[0].ParameterType == expected;
                });
        }
    }

    static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    static void Report(int errors)
    {
        if (errors == 0)
            Debug.Log("[UnityEvent] Aucun lien cassé trouvé.");
        else
            Debug.LogError($"[UnityEvent] {errors} lien(s) cassé(s) trouvé(s). Voir les messages ci-dessus.");
    }
}