using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    #region  Test
    // public List<LevelSO> allLevels = new List<LevelSO>();
    // private Dictionary<int, LevelSO> lookup;

    // // Ensure lookup exists
    // private void EnsureInitialized()
    // {
    //     if (lookup == null || lookup.Count == 0)
    //         lookup = allLevels?.ToDictionary(x => x.levelIndex, x => x) ?? new Dictionary<int, LevelSO>();
    // }

    // public LevelSO GetLevel(int index)
    // {
    //     EnsureInitialized();
    //     return lookup.TryGetValue(index, out var so) ? so : null;
    // }

    // public int Count => allLevels?.Count ?? 0;

    // // Build default playerprogress levels from database
    // public List<LevelData> CreateDefaultLevelData()
    // {
    //     EnsureInitialized();
    //     var list = new List<LevelData>();
    //     foreach (var so in allLevels.OrderBy(l => l.levelIndex))
    //     {
    //         bool unlocked = (so.levelIndex == 1);
    //         var ld = new LevelData(so.levelIndex, unlocked) { levelSO = so };
    //         list.Add(ld);
    //     }
    //     return list;
    // }
    #endregion

    [Header("All Levels (assign in order or any order)")]
    [Tooltip("List of all Level ScriptableObjects in the game.")]
    public List<LevelSO> allLevels = new List<LevelSO>();

    // --- Runtime Cache ---
    private Dictionary<int, LevelSO> lookup;          // Tra cứu nhanh theo levelIndex
    private List<LevelSO> orderedLevels;              // Danh sách LevelSO đã được sắp xếp theo levelIndex
    private bool initialized = false;                 // Cờ báo đã khởi tạo chưa

    // ------------------------------
    // 🧠 HÀM KHỞI TẠO VÀ CẬP NHẬT DỮ LIỆU
    // ------------------------------
    private void EnsureInitialized()
    {
        // Nếu đã khởi tạo rồi thì không làm lại
        if (initialized) return;

        initialized = true;

        // Nếu allLevels rỗng, tạo rỗng để tránh lỗi null
        if (allLevels == null)
        {
            allLevels = new List<LevelSO>();
        }

        // Tạo dictionary tra cứu nhanh: O(n)
        lookup = new Dictionary<int, LevelSO>(allLevels.Count);
        foreach (var so in allLevels)
        {
            if (so == null) continue;
            if (!lookup.ContainsKey(so.levelIndex))
                lookup.Add(so.levelIndex, so);
            else
                Debug.LogWarning($"Duplicate level index found: {so.levelIndex} in {so.name}");
        }

        // Tạo danh sách sắp xếp: O(n log n)
        orderedLevels = allLevels
            .Where(l => l != null)
            .OrderBy(l => l.levelIndex)
            .ToList();
    }

#if UNITY_EDITOR
    // Khi thay đổi trong editor, tự cập nhật lại lookup và orderedLevels
    private void OnValidate()
    {
        initialized = false;
        lookup = null;
        orderedLevels = null;
    }
#endif

    // ------------------------------
    // 🔍 TRUY CẬP DỮ LIỆU
    // ------------------------------
    public LevelSO GetLevel(int index)
    {
        EnsureInitialized();
        return lookup.TryGetValue(index, out var so) ? so : null;
    }

    public LevelSO GetNextLevel(int currentIndex)
    {
        EnsureInitialized();
        int nextIndex = currentIndex + 1;
        return lookup.TryGetValue(nextIndex, out var so) ? so : null;
    }

    public int Count => allLevels?.Count ?? 0;

    // ------------------------------
    // 🏗️ TẠO DỮ LIỆU TIẾN TRÌNH MẶC ĐỊNH
    // ------------------------------
    public List<LevelData> CreateDefaultLevelData()
    {
        EnsureInitialized();

        var list = new List<LevelData>(orderedLevels.Count);
        foreach (var so in orderedLevels)
        {
            bool unlocked = (so.levelIndex == 1);
            var ld = new LevelData(so.levelIndex, unlocked)
            {
                levelSO = so
            };
            list.Add(ld);
        }
        return list;
    }

    // ------------------------------
    // 🧾 CÁC HÀM TIỆN ÍCH KHÁC
    // ------------------------------
    public bool HasLevel(int index)
    {
        EnsureInitialized();
        return lookup.ContainsKey(index);
    }

    public IEnumerable<LevelSO> GetAllLevelsOrdered()
    {
        EnsureInitialized();
        return orderedLevels;
    }
}
