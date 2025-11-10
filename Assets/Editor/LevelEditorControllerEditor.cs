#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditorController))]
public class LevelEditorControllerEditor : Editor
{
    private LevelEditorController controller;
    private SerializedProperty editingLevelProp;

    private void OnEnable()
    {
        controller = (LevelEditorController)target;
        editingLevelProp = serializedObject.FindProperty("editingLevel");
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(editingLevelProp);

        if (controller.editingLevel == null)
        {
            EditorGUILayout.HelpBox("Assign a LevelSO to edit. Then use Scene View to move points.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        if (GUILayout.Button("Add Point at Scene Camera"))
        {
            var so = controller.editingLevel;
            Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
            Vector2 p = new Vector2(camPos.x, camPos.y);
            so.items.Add(new ItemSpawnInfo { pointName = $"Point_{so.items.Count + 1}", position = p });
            EditorUtility.SetDirty(so);
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Clear All Points"))
        {
            if (EditorUtility.DisplayDialog("Clear points", "Remove all spawn points from this LevelSO?", "Yes", "No"))
            {
                controller.editingLevel.items.Clear();
                EditorUtility.SetDirty(controller.editingLevel);
            }
        }

        GUILayout.Space(6);
        EditorGUILayout.LabelField($"Points: {controller.editingLevel.items.Count}");
        // show small list
        for (int i = 0; i < controller.editingLevel.items.Count; i++)
        {
            var it = controller.editingLevel.items[i];
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(controller.selectedIndex == i, $"#{i} {it.pointName}", "Button"))
            {
                controller.selectedIndex = i;
                // ping object
                EditorGUIUtility.PingObject(controller.editingLevel);
            }
            if (GUILayout.Button("Remove", GUILayout.MaxWidth(60)))
            {
                controller.editingLevel.items.RemoveAt(i);
                controller.selectedIndex = -1;
                EditorUtility.SetDirty(controller.editingLevel);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (controller.editingLevel == null) return;
        if (controller.selectedIndex < 0 || controller.selectedIndex >= controller.editingLevel.items.Count) return;

        var so = controller.editingLevel;
        var item = so.items[controller.selectedIndex];

        // current position
        Vector3 worldPos = new Vector3(item.position.x, item.position.y, 0f);

        EditorGUI.BeginChangeCheck();
        Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(so, "Move spawn point");
            item.position = new Vector2(newWorldPos.x, newWorldPos.y);
            EditorUtility.SetDirty(so);
        }

        // small labels and operations
        Handles.color = Color.white;
        Handles.Label(newWorldPos + Vector3.up * 0.25f, $"Point {controller.selectedIndex}: {item.pointName}");
    }
}
#endif
