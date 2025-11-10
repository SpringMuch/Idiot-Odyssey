
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public static class SaveSystem
{
    private static readonly string savePath = Path.Combine(Application.persistentDataPath, "player_progress.json");
    private static readonly object fileLock = new object();

    public static void Save(PlayerProgress data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string temp = savePath + ".tmp";

            lock (fileLock)
            {
                File.WriteAllText(temp, json);
                if (File.Exists(savePath)) File.Delete(savePath);
                File.Move(temp, savePath);
            }

            Debug.Log($"[SaveSystem] Saved to: {savePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Save failed: {ex}");
        }
    }

    /// <summary>
    /// Load data. If file missing or invalid, returns null.
    /// The caller (ProgressManager) will create default if needed.
    /// </summary>
    public static PlayerProgress Load()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                Debug.Log("[SaveSystem] No save file found.");
                return null;
            }

            lock (fileLock)
            {
                string json = File.ReadAllText(savePath);
                if (string.IsNullOrWhiteSpace(json)) return null;
                var data = JsonUtility.FromJson<PlayerProgress>(json);
                return data;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Load failed: {ex}");
            return null;
        }
    }

    public static void DeleteSave()
    {
        try
        {
            lock (fileLock)
            {
                if (File.Exists(savePath)) File.Delete(savePath);
            }
            Debug.Log("[SaveSystem] Save deleted.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Delete failed: {ex}");
        }
    }

    public static string GetSavePath() => savePath;

    public static PlayerProgress CreateNewProgress(LevelDatabase levelDatabase)
    {
        // Tạo instance mới
        PlayerProgress progress = new PlayerProgress
        {
            currentLevel = 1,
            highestLevel = 1,
            hintCount = 3
        };

        // Khởi tạo danh sách level
        //for (int i = 1; i <= totalLevels; i++)
        //{
        //    bool unlocked = (i == 1); // Level 1 mở sẵn, các level khác khóa
        //    progress.levels.Add(new LevelData(i, unlocked));
        //}
        foreach (var lvl in levelDatabase.allLevels)
        {
            bool unlocked = lvl.levelIndex == 1;
            progress.levels.Add(new LevelData(lvl.levelIndex, unlocked) { levelSO = lvl });
        }

        // Lưu file ngay sau khi tạo
        Save(progress);
        return progress;
    }

}



