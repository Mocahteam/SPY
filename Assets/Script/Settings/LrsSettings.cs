using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

class LrsSettings : ScriptableObject
{
    private const string ResourcePath = "Settings/LrsSettings";
    private const string AssetPath = "Assets/Resources/"+ResourcePath+".asset";
    public List<LrsBasicAuthConfig> lrsBasicConfigs = new();
    public List<LrsOAuth2Config> lrsOAuth2Configs = new();

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
                EditorGUILayout.LabelField("LRS Configuration", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lrsBasicConfigs"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lrsOAuth2Configs"), true);

                serializedObject.ApplyModifiedProperties();
            },
            keywords = new HashSet<string>(new[] { "LRS", "Learning Record Store", "xAPI", "TinCan" })
        };
    }
}
