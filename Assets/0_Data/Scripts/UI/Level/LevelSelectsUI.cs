#region Test
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class LevelSelectsUI : MonoBehaviour
// {
//     [SerializeField] private Transform gridParent;
//     [SerializeField] private Button levelButtonPrefab;
//     [SerializeField] private Button backButton;
//     [SerializeField] private RectTransform content;
//     [SerializeField] private ScrollRect scrollRect;

//     void OnEnable()
//     {
//         // Lắng nghe tiến trình đổi để tự rebuild
//         if (ProgressManager.Instance != null)
//         {
//             ProgressManager.Instance.OnProgressLoaded += HandleProgressChanged;
//             ProgressManager.Instance.OnLevelUnlocked  += _ => HandleProgressChanged(ProgressManager.Instance.Progress);
//             ProgressManager.Instance.OnLevelCompleted += _ => HandleProgressChanged(ProgressManager.Instance.Progress);
//             ProgressManager.Instance.OnProgressSaved  += () => HandleProgressChanged(ProgressManager.Instance.Progress);
//         }

//         BuildFromProgress();
//     }

//     void OnDisable()
//     {
//         if (ProgressManager.Instance != null)
//         {
//             ProgressManager.Instance.OnProgressLoaded -= HandleProgressChanged;
//             ProgressManager.Instance.OnLevelUnlocked  -= _ => HandleProgressChanged(ProgressManager.Instance.Progress);
//             ProgressManager.Instance.OnLevelCompleted -= _ => HandleProgressChanged(ProgressManager.Instance.Progress);
//             ProgressManager.Instance.OnProgressSaved  -= () => HandleProgressChanged(ProgressManager.Instance.Progress);
//         }
//     }

//     void HandleProgressChanged(PlayerProgress p) => BuildFromProgress();
//     void HandleProgressSaved() => BuildFromProgress();

//     void BuildFromProgress()
//     {
//         if (UIManager.Instance) UIManager.Instance.MovingCanva.SetActive(true);

//         // Clear cũ trước khi dựng
//         for (int i = gridParent.childCount - 1; i >= 0; i--)
//             Destroy(gridParent.GetChild(i).gameObject);

//         var progress  = ProgressManager.Instance.Progress;
//         var allLevels = progress.levels;

//         foreach (var lvl in allLevels)
//         {
//             var btn = Instantiate(levelButtonPrefab, gridParent);
//             var label = btn.GetComponentInChildren<TextMeshProUGUI>();
//             if (label) label.text = "Level " + lvl.levelIndex;

//             btn.interactable = lvl.isUnlocked;

//             int index = lvl.levelIndex; 
//             btn.onClick.RemoveAllListeners();
//             btn.onClick.AddListener(() =>
//             {
//                 SoundManager.PlaySfx(SoundTypes.Button);
//                 ProgressManager.Instance.SetCurrentLevel(index);
//                 GameEventBus.RequestLoadLevel(index);
//             });

//             var img = btn.GetComponent<Image>();
//             if (img && lvl.isCompleted) img.color = Color.green;
//         }

//         if (backButton != null)
//         {
//             backButton.onClick.RemoveAllListeners();
//             backButton.onClick.AddListener(() =>
//             {
//                 GameEventBus.RaiseGameStateChanged(GameState.MainMenu);
//             });
//         }
//     }
// }
#endregion

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelSelectsUI : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private RectTransform content;
    [SerializeField] private ScrollRect scrollRect;

    // Delegate đúng kiểu event của ProgressManager
    private Action<PlayerProgress> _onLoaded;
    private Action<int> _onUnlocked;
    private Action<int> _onCompleted;
    private Action _onSaved;

    void OnEnable()
    {
        if (ProgressManager.Instance != null)
        {
            _onLoaded   = HandleProgressChanged;
            _onUnlocked = _ => HandleProgressChanged(ProgressManager.Instance.Progress);
            _onCompleted= _ => HandleProgressChanged(ProgressManager.Instance.Progress);
            _onSaved    = HandleProgressSaved;

            ProgressManager.Instance.OnProgressLoaded += _onLoaded;
            ProgressManager.Instance.OnLevelUnlocked  += _onUnlocked;
            ProgressManager.Instance.OnLevelCompleted += _onCompleted;
            ProgressManager.Instance.OnProgressSaved  += _onSaved;
        }

        BuildFromProgress();
    }

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
        {
            if (_onLoaded   != null) ProgressManager.Instance.OnProgressLoaded -= _onLoaded;
            if (_onUnlocked != null) ProgressManager.Instance.OnLevelUnlocked  -= _onUnlocked;
            if (_onCompleted!= null) ProgressManager.Instance.OnLevelCompleted -= _onCompleted;
            if (_onSaved    != null) ProgressManager.Instance.OnProgressSaved  -= _onSaved;
        }
    }

    void HandleProgressChanged(PlayerProgress p) => BuildFromProgress();
    void HandleProgressSaved() => BuildFromProgress();

    void BuildFromProgress()
    {
        if (UIManager.Instance) UIManager.Instance.MovingCanva.SetActive(true);

        // Clear cũ
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        var progress  = ProgressManager.Instance.Progress;
        var allLevels = progress.levels;

        foreach (var lvl in allLevels)
        {
            var btn = Instantiate(levelButtonPrefab, gridParent);
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = "Level " + lvl.levelIndex;

            btn.interactable = lvl.isUnlocked;

            int index = lvl.levelIndex;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SoundManager.PlaySfx(SoundTypes.Button);
                ProgressManager.Instance.SetCurrentLevel(index);
                GameEventBus.RequestLoadLevel(index);
            });

            var img = btn.GetComponent<Image>();
            if (img && lvl.isCompleted) img.color = Color.green;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                GameEventBus.RaiseGameStateChanged(GameState.MainMenu);
            });
        }

        // Force rebuild layout (không set scroll về top)
        if (content != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            // >>> TỰ TÍNH CHIỀU CAO CONTENT (nếu dùng GridLayoutGroup)
            var grid = content.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                int columns = 1;

                if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount && grid.constraintCount > 0)
                {
                    columns = grid.constraintCount;
                }
                else
                {
                    // Ước lượng số cột theo width viewport nếu bạn không cố định cột
                    var viewport = scrollRect ? (RectTransform)scrollRect.viewport : (RectTransform)content.parent;
                    float innerWidth = viewport.rect.width - grid.padding.left - grid.padding.right + grid.spacing.x;
                    columns = Mathf.Max(1, Mathf.FloorToInt(innerWidth / (grid.cellSize.x + grid.spacing.x)));
                }

                // Đếm số button thực sự active (tránh tính cả object tắt)
                int childCount = 0;
                for (int i = 0; i < gridParent.childCount; i++)
                    if (gridParent.GetChild(i).gameObject.activeSelf) childCount++;

                int rows = Mathf.CeilToInt((float)childCount / columns);

                float height =
                    grid.padding.top + grid.padding.bottom +
                    rows * grid.cellSize.y +
                    Mathf.Max(0, rows - 1) * grid.spacing.y;

                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Canvas.ForceUpdateCanvases();
        }
    }
}
