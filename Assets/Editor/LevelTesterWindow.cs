#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelTesterWindow : EditorWindow
{
    private LevelSO level;
    private Vector2 scroll;
    private GameObject previewParent;
    private const string previewParentName = "[LevelPreviewParent]";
    private const string EditorPrefKey = "LT_TEST_LEVEL_INDEX";

    [MenuItem("Game Tools/Level Tester")]
    public static void ShowWindow() => GetWindow<LevelTesterWindow>("Level Tester");

    private void OnGUI()
    {
        GUILayout.Label("Level Tester", EditorStyles.boldLabel);
        //level = (LevelSO)EditorGUILayout.ObjectField("LevelSO", level, typeof(LevelSO), false);
        // --- SAFE ObjectField (sửa lỗi reparent) ---
        Rect rect = EditorGUILayout.GetControlRect();
        level = (LevelSO)EditorGUI.ObjectField(rect, "LevelSO", level, typeof(LevelSO), true);
        // --- END SAFE ObjectField ---

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Preview Level in Scene"))
        {
            PreviewLevel();
        }
        if (GUILayout.Button("Clear Preview"))
        {
            ClearPreview();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);
        if (GUILayout.Button("Validate Level"))
        {
            ValidateLevel();
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Play In Editor (Start this level)"))
        {
            PlayInEditor();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Preview: instantiate prefabs in EDITOR (not PlayMode). Use Clear Preview to remove.", MessageType.Info);
        GUILayout.FlexibleSpace();

        if (level != null)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField($"Level {level.levelIndex} - {level.levelName}", EditorStyles.boldLabel);
            for (int i = 0; i < level.items.Count; i++)
            {
                var it = level.items[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"#{i} {it.pointName}");
                EditorGUILayout.ObjectField("Prefab", it.prefab, typeof(GameObject), false );
                EditorGUILayout.Vector2Field("Position", it.position);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void PreviewLevel()
    {
        if (level == null) { EditorUtility.DisplayDialog("Preview", "Assign a LevelSO first.", "OK"); return; }

        // create or find parent
        previewParent = GameObject.Find(previewParentName);
        if (previewParent == null)
        {
            previewParent = new GameObject(previewParentName);
        }

        // instantiate prefabs as prefab instances in editor for preview
        foreach (var it in level.items)
        {
            if (it.prefab == null)
            {
                Debug.LogWarning($"[LevelTester] Missing prefab for point {it.pointName}");
                continue;
            }
            // Use PrefabUtility.InstantiatePrefab so instance is linked to prefab (good for scene editing)
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(it.prefab, SceneManager.GetActiveScene());
            go.transform.position = new Vector3(it.position.x, it.position.y, 0f);
            go.name = $"PREVIEW_{it.prefab.name}_L{level.levelIndex}_{it.pointName}";
            Undo.RegisterCreatedObjectUndo(go, "Preview Spawn");
            go.transform.SetParent(previewParent.transform, true);
        }

        // ping
        Selection.activeGameObject = previewParent;
    }

    private void ClearPreview()
    {
        var existing = GameObject.Find(previewParentName);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }
    }

    private void ValidateLevel()
    {
        if (level == null) { EditorUtility.DisplayDialog("Validate", "Assign a LevelSO first.", "OK"); return; }

        List<string> issues = new List<string>();
        // 1. missing prefab
        for (int i = 0; i < level.items.Count; i++)
        {
            if (level.items[i].prefab == null)
                issues.Add($"Missing prefab at item #{i} ({level.items[i].pointName})");
        }
        // 2. duplicate pointName
        var set = new HashSet<string>();
        for (int i = 0; i < level.items.Count; i++)
        {
            var name = level.items[i].pointName;
            if (!string.IsNullOrEmpty(name))
            {
                if (set.Contains(name)) issues.Add($"Duplicate pointName: {name}");
                else set.Add(name);
            }
        }
        // 3. optional: out of range (example: beyond certain area)
        // you can change allowed bounds below
        Rect allowed = new Rect(-20, -20, 40, 40);
        for (int i = 0; i < level.items.Count; i++)
        {
            var p = level.items[i].position;
            if (!allowed.Contains(p))
                issues.Add($"Point {level.items[i].pointName} out of allowed bounds {p}");
        }

        if (issues.Count == 0)
        {
            EditorUtility.DisplayDialog("Validate", "No issues found.", "OK");
        }
        else
        {
            // show results
            string msg = string.Join("\n", issues);
            EditorUtility.DisplayDialog("Validate - Issues", msg, "OK");
            Debug.LogWarning("[LevelTester] Validation issues:\n" + msg);
        }
    }

    private void PlayInEditor()
    {
        if (level == null) { EditorUtility.DisplayDialog("Play", "Assign a LevelSO first.", "OK"); return; }

        // Remember requested level in EditorPrefs and start PlayMode.
        EditorPrefs.SetInt(EditorPrefKey, level.levelIndex);
        // Start PlayMode
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.EnterPlaymode();
        }
    }

    private void OnDestroy()
    {
        // cleanup preview on window close
        // Do NOT auto destroy so user can inspect; keep as manual ClearPreview
    }
}
#endif
