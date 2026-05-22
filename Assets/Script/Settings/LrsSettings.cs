using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

class LrsSettings : ScriptableObject
{
    private const string ResourcePath = "Settings/LrsSettings";
    private const string AssetPath = "Assets/Resources/"+ResourcePath+".asset";

    [Serializable]
    public class LrsConfigs {
        public List<LrsBasicAuthConfig> lrsBasicConfigs = new();
        public List<LrsOAuth2Config> lrsOAuth2Configs = new();
    };
    [SerializeField]
    private LrsConfigs m_editorConfigs = new();
    [SerializeField]
    private LrsConfigs m_buildConfigs = new();

    public LrsConfigs GetConfigs()
    {
        return Application.isEditor ? m_editorConfigs : m_buildConfigs;
    }

    internal static LrsSettings GetOrCreate()
    {
        var settings = Resources.Load<LrsSettings>(ResourcePath);
        if (settings == null)
        {
            settings = CreateInstance<LrsSettings>();
            #if UNITY_EDITOR
                var dirInfo = Directory.GetParent(AssetPath);
                if (!dirInfo.Exists) {
                    dirInfo.Create();
                }
                AssetDatabase.CreateAsset(settings, AssetPath);
                AssetDatabase.SaveAssets();
            #endif

        }
        return settings;
    }

    #if UNITY_EDITOR
    internal static SerializedObject GetSerializedObject() => new(GetOrCreate());


    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new SettingsProvider("Project/LRS Settings", SettingsScope.Project)
        {
            label = "LRS Configuration",
            guiHandler = _ =>
            {
                SerializedObject serializedObject = GetSerializedObject();
                EditorGUILayout.LabelField("Editor configuration", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_editorConfigs"), true);
                EditorGUILayout.LabelField("Build configuration", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_buildConfigs"), true);

                serializedObject.ApplyModifiedProperties();
            },
            keywords = new HashSet<string>(new[] { "LRS", "Learning Record Store", "xAPI", "TinCan" })
        };
    }
    #endif
}
