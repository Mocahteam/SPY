using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Comment))]
public class CommentEditor : Editor
{
    private SerializedProperty comment;

    private GUIStyle redTextAreaStyle;

    private void OnEnable()
    {
        comment = serializedObject.FindProperty("comment");

        redTextAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            normal =
            {
                textColor = Color.yellow
            },

            focused =
            {
                textColor = Color.yellow
            },

            hover =
            {
                textColor = Color.yellow
            },

            active =
            {
                textColor = Color.yellow
            },

            wordWrap = true
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Champ de saisie directement en rouge
        comment.stringValue = EditorGUILayout.TextArea(
            comment.stringValue,
            redTextAreaStyle,
            GUILayout.MinHeight(70)
        );

        serializedObject.ApplyModifiedProperties();
    }
}