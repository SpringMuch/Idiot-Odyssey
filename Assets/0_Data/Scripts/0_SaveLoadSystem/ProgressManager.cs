using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1110)]
public class ProgressManager : MonoBehaviour
{
    #region  Test
    // public static ProgressManager Instance;
    // [SerializeField] private PlayerProgress progress;
    // [SerializeField] private LevelDatabase levelDatabase;
    // public PlayerProgress Progress => progress;

    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //         LoadProgress();
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    // public void LoadProgress()
    // {
    //     if ((progress == null || levelDatabase == null)) return;
    //     levelDatabase.Initialize();
    //     progress = SaveSystem.Load(levelDatabase);
    // }

    // public void SaveProgress()
    // {
    //     SaveSystem.Save(Progress);
    // }

    // public void LoadLevel(int levelIndex)
    // {
    //     LevelSO level = levelDatabase.GetLevel(levelIndex);
    //     if (level == null)
    //     {
    //         Debug.LogError($"Level {levelIndex} not found!");
    //         return;
    //     }

    //     progress.currentLevel = levelIndex;
    //     SaveSystem.Save(progress);

    //     //SceneManager.LoadScene(level.sceneName);
    //     SceneTransition.Instance.LoadSceneSmooth($"Level_{level.levelIndex}");
    // }

    // public void CompleteCurrentLevel()
    // {
    //     var data = progress.levels.Find(l => l.levelIndex == progress.currentLevel);
    //     if (data != null && !data.isCompleted)
    //     {
    //         data.isCompleted = true;

    //         if (data.levelIndex < progress.levels.Count)
    //         {
    //             var next = progress.levels[data.levelIndex];
    //             next.isUnlocked = true;
    //             progress.highestLevel = Mathf.Max(progress.highestLevel, next.levelIndex);
    //         }

    //         SaveSystem.Save(progress);
    //     }
    // }

    // public void CompleteLevel(int levelIndex)
    // {
    //     var data = progress.levels.Find(l => l.levelIndex == levelIndex);
    //     if (data != null)
    //     {
    //         data.isCompleted = true;

    //         int next = levelIndex + 1;
    //         var nextLevel = progress.levels.Find(l => l.levelIndex == next);

    //         if (nextLevel != null) nextLevel.isUnlocked = true;

    //         progress.currentLevel = next;
    //         if (next > progress.highestLevel) progress.highestLevel = next;

    //         SaveProgress();
    //     }
    // }

    // public void SetCurrentLevel(int levelIndex)
    // {
    //     progress.currentLevel = levelIndex;
    //     SaveProgress();
    // }
    #endregion

    public static ProgressManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private LevelDatabase levelDatabase;
    public PlayerProgress Progress => progress;

    // Events
    public event Action<int> OnHintCountChanged;
    public event Action<PlayerProgress> OnProgressLoaded;
    public event Action<int> OnLevelUnlocked;    // levelIndex
    public event Action<int> OnLevelCompleted;   // levelIndex
    public event Action OnProgressSaved;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (levelDatabase == null) Debug.LogError("[ProgressManager] levelDatabase not assigned!");
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Load progress from disk or create new from database
    public void Load()
    {
        var loaded = SaveSystem.Load();
        if (loaded == null)
        {
            // create new from DB
            progress = new PlayerProgress
            {
                currentLevel = 1,
                highestLevel = 1,
                levels = levelDatabase.CreateDefaultLevelData()
            };
            Save(); // persist initial state
            Debug.Log("[ProgressManager] Created new progress.");
        }
        else
        {
            progress = loaded;
            // map SO references (in case they are null)
            foreach (var ld in Progress.levels)
            {
                ld.levelSO = levelDatabase.GetLevel(ld.levelIndex);
            }
            // Debug.Log("[ProgressManager] Loaded existing progress.");
        }

        OnProgressLoaded?.Invoke(progress);
        OnHintCountChanged?.Invoke(progress.hintCount);
    }

    // Save current progress
    public void Save()
    {
        if (Progress == null) return;
        SaveSystem.Save(progress);
        OnProgressSaved?.Invoke();
    }
    public void AddHint(int amount)
    {
        progress.AddHint(amount);
        Save();
        OnHintCountChanged?.Invoke(progress.hintCount);
    }

    public bool UseHint(int amount = 10)
    {
        if (!progress.UseHint(amount))
        {
            return false;
        }

        Save();
        OnHintCountChanged?.Invoke(progress.hintCount);
        return true;
    }

    public int GetHintCount() => progress.hintCount;
    public void CompleteLevel(int levelIndex = -1)
    {
        int idx = levelIndex > 0 ? levelIndex : progress.currentLevel;

        bool changed = progress.MarkComplete(idx);
        if (changed)
        {
            OnLevelCompleted?.Invoke(idx);
            AddHint(2);

            // unlock next
            bool unlocked = progress.UnlockNextFrom(idx);
            if (unlocked)
            {
                OnLevelUnlocked?.Invoke(idx + 1);
            }
            Save();
        }
    }

    public void SetCurrentLevel(int levelIndex)
    {
        if (Progress.levels != null && Progress.levels.Count > 0)
        {
            int lastIndex = Progress.levels[Progress.levels.Count - 1].levelIndex;
            if (levelIndex > lastIndex) levelIndex = lastIndex;
            if (levelIndex < Progress.levels[0].levelIndex) levelIndex = Progress.levels[0].levelIndex;
        }

        Progress.currentLevel = levelIndex;
        Save();
    }


    private void OnApplicationQuit()
    {
        Save();
    }

    public void ResetProgress()
    {
        SaveSystem.DeleteSave();
        progress = SaveSystem.CreateNewProgress(levelDatabase);
        progress.ResetHints(10);
        OnHintCountChanged?.Invoke(progress.hintCount);
    }

}
