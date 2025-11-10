#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class LevelValidationTool
{
    [MenuItem("Game Tools/Validate All Levels")]
    public static void ValidateAllLevels()
    {
        string[] guids = AssetDatabase.FindAssets("t:LevelSO",new[] { "Assets/0_Data/ScriptableObject/Levels" });
        List<string> errors = new List<string>();
        Dictionary<int, string> indexMap = new Dictionary<int, string>();

        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            LevelSO so = AssetDatabase.LoadAssetAtPath<LevelSO>(path);
            if (so == null) continue;

            if (indexMap.ContainsKey(so.levelIndex))
                errors.Add($"Duplicate levelIndex {so.levelIndex}: {path} and {indexMap[so.levelIndex]}");
            else
                indexMap[so.levelIndex] = path;

            for (int i = 0; i < so.items.Count; i++)
            {
                var it = so.items[i];
                if (it.prefab == null)
                    errors.Add($"Missing prefab: {path} - item #{i} ({it.pointName})");
            }
        }

        if (errors.Count == 0)
        {
            EditorUtility.DisplayDialog("Validate All Levels", "No issues found.", "OK");
        }
        else
        {
            string outMsg = string.Join("\n", errors.ToArray());
            Debug.LogWarning("[LevelValidation] Issues:\n" + outMsg);
            EditorUtility.DisplayDialog("Validate All Levels - Issues", "See Console for details.", "OK");
        }
    }
}
#endif
