using UnityEngine;
using UnityEditor;

public class ShaderText
	: MonoBehaviour
{
    public string Text;
}

[CustomEditor(typeof(ShaderText))]
public class ShaderTextEditor
    : Editor
{
    Vector2 scroll_position = Vector2.zero;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        scroll_position = EditorGUILayout.BeginScrollView(scroll_position);
        EditorGUILayout.TextArea((target as ShaderText).Text);
        EditorGUILayout.EndScrollView();
    }
}
