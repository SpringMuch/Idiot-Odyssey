// ✅ Custom inspector cho LevelManagerEditor
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManagerEditor))]
public class LevelManagerEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelManagerEditor lm = (LevelManagerEditor)target;

        GUILayout.Space(10);
        GUILayout.Label("=== Editor Tools ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Preview Level"))
            lm.PreviewLevel(lm.name == "" ? 1 : lm.PreviewIndex);

        if (GUILayout.Button("Clear Preview"))
            lm.ClearPreview();

        if (GUILayout.Button("Validate All Levels"))
            lm.ValidateAllLevels();

        if (GUILayout.Button("▶️ Play This Level"))
            lm.PlayThisLevel();
    }
}