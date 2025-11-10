using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-10000)] // Đảm bảo LevelManager chạy trước các script khác
[RequireComponent(typeof(LevelLoader))]
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // 🔔 Event dùng cho gameplay hoặc UI
    public Action<int> OnLevelWin;
    public event Action<int, LevelSO> OnLevelLoaded;  // Khi level mới được load
    public event Action<int> OnLevelCleared;          // Khi level hiện tại bị clear
    public event Action OnBeforeLevelLoad;            // Khi bắt đầu load
    public event Action OnAfterLevelLoad;             // Khi load xong
                    // Khi level hiện tại win

    [Tooltip("Danh sách LevelSO nếu bạn không dùng LevelDatabase. Nếu dùng LevelDatabase, assign nó vào LevelDatabase field in ProgressManager and ProgressManager will map references.")]
    [SerializeField] private List<LevelSO> allLevels = new List<LevelSO>();

    private LevelLoader loader;
    public LevelLoader Loader => loader;
    // [SerializeField] private int startLevelIndex = 1;
    [SerializeField] private int currentLevelIndex = 0;
    public int CurrentLevelIndex => currentLevelIndex;

    [Tooltip("Cho phép override khi test trong Editor (LevelTesterWindow).")]
    public bool allowEditorStartOverride = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        loader = GetComponent<LevelLoader>();
    }

    private void Start()
    {
        // ✅ Lấy level hiện tại từ ProgressManager hoặc fallback
        if (ProgressManager.Instance != null && ProgressManager.Instance.Progress != null)
            currentLevelIndex = ProgressManager.Instance.Progress.currentLevel;
        else if (GameManager.Instance.State == GameState.Playing)
            currentLevelIndex = 1;
        else
            currentLevelIndex = 0;

#if UNITY_EDITOR
        // ✅ Cho phép override từ Editor Test Window
        if (allowEditorStartOverride && EditorPrefs.HasKey("LT_TEST_LEVEL_INDEX"))
        {
            int editorLevel = EditorPrefs.GetInt("LT_TEST_LEVEL_INDEX", -1);
            if (editorLevel > 0)
            {
                currentLevelIndex = editorLevel;
                Debug.Log($"[LevelManager] Editor override start level -> {editorLevel}");
            }
            EditorPrefs.DeleteKey("LT_TEST_LEVEL_INDEX");
        }
#endif
    }

    public void LoadNextLevel()
    {
        int next = currentLevelIndex + 1;
        LevelSO nextSO = FindLevelSO(next);
        if (nextSO == null)
        {
            GameEventBus.RaiseOpenLevelSelect();
            return;
        }

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.CompleteLevel(currentLevelIndex);

        StartCoroutine(LoadLevelRoutine(next));
    }

    public void RetryLevel()
    {
        StartCoroutine(LoadLevelRoutine(currentLevelIndex));
        GameManager.Instance.SetState(GameState.Playing);
    }

    public void LoadSpecificLevel(int levelIndex)
    {
        StartCoroutine(LoadLevelRoutine(levelIndex));
    }

    public void ClearCurrentLevel()
    {
        if (loader.HasActiveLevel)
        {
            loader.ClearLevel();
            OnLevelCleared?.Invoke(currentLevelIndex);
        }
    }

    // ----------------------------------------------------------
    // 🧠 HÀM NỘI BỘ CHÍNH
    // ----------------------------------------------------------

    private IEnumerator LoadLevelRoutine(int levelIndex)
    {
        OnBeforeLevelLoad?.Invoke();

        if (loader.HasActiveLevel)
        {
            loader.ClearLevel();
            OnLevelCleared?.Invoke(currentLevelIndex);
            yield return null;
        }

        currentLevelIndex = levelIndex;
        LevelSO so = FindLevelSO(levelIndex);
        if (so == null)
        {
            Debug.LogWarning($"[LevelManager] LevelSO {levelIndex} not found → open Level Select.");
            GameEventBus.RaiseOpenLevelSelect();
            yield break;
        }

        loader.SpawnLevel(so);
        OnLevelLoaded?.Invoke(levelIndex, so);

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.SetCurrentLevel(levelIndex);

        OnAfterLevelLoad?.Invoke();
    }

    private LevelSO FindLevelSO(int index)
    {
        // Ưu tiên tìm qua ProgressManager
        if (ProgressManager.Instance != null)
        {
            var data = ProgressManager.Instance.Progress.GetLevelData(index);
            if (data != null && data.levelSO != null)
                return data.levelSO;
        }

        // Fallback
        return allLevels.Find(l => l != null && l.levelIndex == index);
    }

    public bool HasNextLevel()
    {
        return FindLevelSO(currentLevelIndex + 1) != null;
    }
    public void SetCurrentIndex(int currentIndex)
    {
        this.currentLevelIndex = currentIndex;
    }
}
