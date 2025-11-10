#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelSO currentLevel;
    private Vector2 scroll;

    [MenuItem("Game Tools/Level Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        currentLevel = (LevelSO)EditorGUILayout.ObjectField("Editing Level", currentLevel, typeof(LevelSO), false) as LevelSO;
        if (currentLevel == null)
        {
            EditorGUILayout.HelpBox("Select a LevelSO to edit its points.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Ping In Project"))
            EditorGUIUtility.PingObject(currentLevel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Point (at 0,0)"))
        {
            currentLevel.items.Add(new ItemSpawnInfo
            {
                pointName = $"Point_{currentLevel.items.Count + 1}",
                position = Vector2.zero,
                rotation = 0f,
                scale = Vector2.one,
                count = 1
            });
            EditorUtility.SetDirty(currentLevel);
        }

        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < currentLevel.items.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            var item = currentLevel.items[i];

            item.pointName = EditorGUILayout.TextField("Name", item.pointName);
            item.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", item.prefab, typeof(GameObject), false);
            item.position = EditorGUILayout.Vector2Field("Position", item.position);
            item.rotation = EditorGUILayout.FloatField("Rotation", item.rotation);
            item.scale = EditorGUILayout.Vector2Field("Scale", item.scale);
            item.count = EditorGUILayout.IntField("Count", item.count);

            currentLevel.items[i] = item; // 👈 Gán lại vào danh sách (rất quan trọng)

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Remove"))
            {
                currentLevel.items.RemoveAt(i);
                EditorUtility.SetDirty(currentLevel);
                GUI.backgroundColor = Color.white;
                break;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();

        if (GUILayout.Button("Save Asset"))
        {
            EditorUtility.SetDirty(currentLevel);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        // Nếu có thay đổi, đánh dấu dirty để Unity lưu
        if (GUI.changed)
            EditorUtility.SetDirty(currentLevel);
    }
}
#endif
// [InitializeOnLoad]
// public static class HideEditorWarning
// {
//     static HideEditorWarning()
//     {
//         Application.logMessageReceived += (c, s, t) =>
//         {
//             if (c.Contains("Cannot reparent window to suggested parent"))
//                 return; // Bỏ qua
//             Debug.unityLogger.Log(t, c);
//         };
//     }
// }
