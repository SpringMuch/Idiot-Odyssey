using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgress
{
    #region  Test
    // public int currentLevel;
    // public int highestLevel;
    // public List<LevelData> levels = new List<LevelData>();

    // // 🔓 Mở khóa level kế tiếp khi thắng
    // public void UnlockNextLevel()
    // {
    //     int nextLevelIndex = currentLevel + 1;

    //     LevelData next = levels.Find(l => l.levelIndex == nextLevelIndex);
    //     if (next != null && !next.isUnlocked)
    //     {
    //         next.isUnlocked = true;
    //         highestLevel = UnityEngine.Mathf.Max(highestLevel, nextLevelIndex);
    //         UnityEngine.Debug.Log($"🔓 Level {nextLevelIndex} unlocked!");
    //     }
    // }
    #endregion

    public int currentLevel = 1;
    public int highestLevel = 1;
    public List<LevelData> levels = new List<LevelData>();

    // ✅ Số lượng bóng đèn hiện có
    public int hintCount = 3;

    /// <summary>
    /// Cộng thêm hint cho người chơi
    /// </summary>
    public void AddHint(int amount)
    {
        hintCount = Math.Max(0, hintCount + amount);
    }

    /// <summary>
    /// Trừ hint khi người chơi sử dụng
    /// </summary>
    /// <returns>true nếu đủ hint, false nếu không đủ</returns>
    public bool UseHint(int amount = 1)
    {
        if (hintCount < amount || GameManager.Instance.GetState() != GameState.Playing) return false;
        hintCount -= amount;
        return true;
    }

    /// <summary>
    /// Reset hint (khi reset progress)
    /// </summary>
    public void ResetHints(int amount = 3)
    {
        hintCount = amount;
    }

    // Helper: find level data by levelIndex
    public LevelData GetLevelData(int levelIndex)
    {
        return levels.Find(l => l.levelIndex == levelIndex);
    }

    // Unlock next level relative to currentLevel
    // returns true if something changed
    public bool UnlockNextFrom(int fromLevel)
    {
        int next = fromLevel + 1;
        var ld = GetLevelData(next);
        if (ld != null && !ld.isUnlocked)
        {
            ld.isUnlocked = true;
            highestLevel = UnityEngine.Mathf.Max(highestLevel, next);
            UnityEngine.Debug.Log($"[Progress] Unlocked level {next}");
            return true;
        }
        return false;
    }

    // Mark completed for a level
    public bool MarkComplete(int levelIndex)
    {
        var ld = GetLevelData(levelIndex);
        if (ld != null && !ld.isCompleted)
        {
            ld.isCompleted = true;
            return true;
        }
        return false;
    }
    
}
