#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class LevelManagerEditor : MonoBehaviour
{
    [Header("Level Configuration")]
    public LevelDatabase database;
    public LevelLoader loader;
    [SerializeField] private int previewIndex = 1;

    private LevelSO currentPreviewLevel;

    //get
    public int PreviewIndex => previewIndex;

    public void PreviewLevel(int levelIndex)
    {
        if (database == null || loader == null)
        {
            Debug.LogError("❌ LevelManagerEditor: Missing LevelDatabase or LevelLoader!");
            return;
        }

        var so = database.GetLevel(levelIndex);
        if (so == null)
        {
            Debug.LogWarning($"⚠️ Level {levelIndex} not found in database.");
            return;
        }

        loader.ClearLevel();
        loader.SpawnLevel(so);
        currentPreviewLevel = so;
        previewIndex = levelIndex;

        Debug.Log($"👀 Previewing Level {levelIndex}: {so.name}");
    }

    public void ClearPreview()
    {
        loader.ClearLevel();
        currentPreviewLevel = null;
    }

    public void ValidateAllLevels()
    {
        if (database == null)
        {
            Debug.LogError("❌ LevelDatabase not assigned!");
            return;
        }

        foreach (var so in database.GetAllLevelsOrdered())
        {
            if (so == null)
                Debug.LogError("⚠️ Null LevelSO in database!");
        }

        Debug.Log("✅ All levels validated!");
    }

    public void PlayThisLevel()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("⚠️ Cannot test level while in Play Mode!");
            return;
        }

        EditorPrefs.SetInt("LT_TEST_LEVEL_INDEX", previewIndex);
        EditorApplication.EnterPlaymode();

        Debug.Log($"▶️ Play In Editor: Level {previewIndex}");
    }
}

#endif
