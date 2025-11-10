using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Cần dùng List
using System; // Cần dùng Action

// Gắn script này vào GameObject LevelSelectPanel thay cho script cũ
public class LevelSelectsUI_Optimized : MonoBehaviour
{
    [Header("Tham chiếu UI")]
    [SerializeField] private Button levelButtonPrefab; // Prefab có gắn LevelSelectItemUI
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;  // Panel Content chứa các nút
    [SerializeField] private RectTransform viewport; // Viewport của ScrollRect
    [SerializeField] private Button backButton;

    [Header("Cấu hình Grid (PHẢI GIỐNG VỚI GRID CŨ)")]
    [SerializeField] private float cellWidth   = 200f;
    [SerializeField] private float cellHeight  = 200f;
    [SerializeField] private float spacingX    = 20f;
    [SerializeField] private float spacingY    = 20f;
    [SerializeField] private int   columns     = 3;
    [SerializeField] private float paddingTop  = 20f;
    [SerializeField] private float paddingLeft = 20f;

    // Pool của chúng ta
    private readonly List<LevelSelectItemUI> itemPool = new List<LevelSelectItemUI>();
    private List<LevelData> allLevels; // Cache danh sách level

    // Delegate để unsubcribe event
    private Action<PlayerProgress> _onLoaded;
    private Action<int> _onUnlocked;
    private Action<int> _onCompleted;
    private Action _onSaved;

    void Start()
    {
        // Tắt component GridLayoutGroup đi, chúng ta sẽ tự tính toán
        var gridLayout = content.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            Debug.LogWarning("Đã tắt GridLayoutGroup để tối ưu. Script sẽ tự tính toán vị trí.");
            // Lấy thông số từ Grid Layout nếu có
            cellWidth   = gridLayout.cellSize.x;
            cellHeight  = gridLayout.cellSize.y;
            spacingX    = gridLayout.spacing.x;
            spacingY    = gridLayout.spacing.y;
            paddingTop  = gridLayout.padding.top;
            paddingLeft = gridLayout.padding.left;

            if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                columns = gridLayout.constraintCount;
            }
            gridLayout.enabled = false;
        }

        // Thêm listener vào sự kiện cuộn
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    void OnEnable()
    {
        // Đăng ký các event từ ProgressManager
        if (ProgressManager.Instance != null)
        {
            _onLoaded    = HandleProgressChanged;
            _onUnlocked  = _ => HandleProgressChanged(ProgressManager.Instance.Progress);
            _onCompleted = _ => HandleProgressChanged(ProgressManager.Instance.Progress);
            _onSaved     = HandleProgressSaved;

            ProgressManager.Instance.OnProgressLoaded += _onLoaded;
            ProgressManager.Instance.OnLevelUnlocked  += _onUnlocked;
            ProgressManager.Instance.OnLevelCompleted += _onCompleted;
            ProgressManager.Instance.OnProgressSaved  += _onSaved;

            // Build UI khi được bật
            HandleProgressChanged(ProgressManager.Instance.Progress);
        }
        else
        {
            // Không có ProgressManager, tránh NullRef
            HandleProgressChanged(null);
        }
    }

    void OnDisable()
    {
        // Hủy đăng ký event
        if (ProgressManager.Instance != null)
        {
            if (_onLoaded    != null) ProgressManager.Instance.OnProgressLoaded -= _onLoaded;
            if (_onUnlocked  != null) ProgressManager.Instance.OnLevelUnlocked  -= _onUnlocked;
            if (_onCompleted != null) ProgressManager.Instance.OnLevelCompleted -= _onCompleted;
            if (_onSaved     != null) ProgressManager.Instance.OnProgressSaved  -= _onSaved;
        }
    }

    /// <summary>
    /// Hàm này được gọi khi ProgressManager tải xong hoặc có cập nhật
    /// </summary>
    void HandleProgressChanged(PlayerProgress p)
    {
        if (p == null)
        {
            // Không có progress -> clear list
            allLevels = new List<LevelData>();
            return;
        }

        allLevels = p.levels;
        if (UIManager.Instance != null)
            UIManager.Instance.MovingCanva.SetActive(true);

        // --- Bước Ảo hóa ---
        // 1. Tính toán chiều cao tổng "ảo" của Content
        int totalRows = Mathf.CeilToInt((float)allLevels.Count / columns);
        float totalHeight = 0f;
        if (totalRows > 0)
        {
            totalHeight = paddingTop + (totalRows * (cellHeight + spacingY)) - spacingY;
        }

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        // 2. Di chuyển content về đầu
        content.anchoredPosition      = new Vector2(content.anchoredPosition.x, 0f);
        scrollRect.normalizedPosition = new Vector2(0f, 1f);

        // 3. Cập nhật các nút hiển thị
        UpdateVisibleItems();

        // 4. Nút Back
        if (backButton != null)
        {
            var abstractButton = backButton.GetComponent<AbstractButtonUI>();
            if (abstractButton == null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.SetState(GameState.MainMenu);
                });
            }
        }
    }

    void HandleProgressSaved()
    {
        UpdateVisibleItems();
    }

    private void OnScroll(Vector2 normalizedPos)
    {
        UpdateVisibleItems();
    }

    private void UpdateVisibleItems()
    {
        if (allLevels == null || allLevels.Count == 0)
        {
            // Ẩn tất cả các nút trong pool nếu không có level
            for (int i = 0; i < itemPool.Count; i++)
            {
                itemPool[i].Hide();
            }
            return;
        }

        float viewportHeight = viewport.rect.height;
        float currentY = content.anchoredPosition.y;

        int firstVisibleRow = Mathf.FloorToInt((currentY - paddingTop) / (cellHeight + spacingY));
        firstVisibleRow = Mathf.Max(0, firstVisibleRow - 1); // Trừ 1 để đệm

        int rowsInView    = Mathf.CeilToInt(viewportHeight / (cellHeight + spacingY));
        int lastVisibleRow = firstVisibleRow + rowsInView + 1; // Cộng 1 để đệm

        int totalRows = Mathf.CeilToInt((float)allLevels.Count / columns);
        lastVisibleRow = Mathf.Min(lastVisibleRow, totalRows - 1);

        // --- Quản lý Pool ---
        int poolIndex = 0;

        // 1. Cập nhật và hiển thị các nút cần thiết
        for (int row = firstVisibleRow; row <= lastVisibleRow; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int levelDataIndex = (row * columns) + col;

                // Dừng nếu hết level
                if (levelDataIndex >= allLevels.Count) break;

                // Lấy nút từ pool (hoặc tạo mới nếu pool chưa đủ)
                LevelSelectItemUI item;
                if (poolIndex < itemPool.Count)
                {
                    item = itemPool[poolIndex];
                }
                else
                {
                    var btnGO = Instantiate(levelButtonPrefab, content);
                    item = btnGO.GetComponent<LevelSelectItemUI>();
                    itemPool.Add(item);
                }
                poolIndex++;

                // Lấy data
                LevelData data = allLevels[levelDataIndex];

                // Tính vị trí X, Y cho nút
                float xPos = paddingLeft + col * (cellWidth + spacingX);
                float yPos = -paddingTop - row * (cellHeight + spacingY); // Dùng âm vì UI anchor ở trên

                // Cập nhật nút
                item.UpdateData(data, new Vector2(xPos, yPos));
            }
        }

        // 2. Ẩn các nút không dùng đến trong pool
        for (int i = poolIndex; i < itemPool.Count; i++)
        {
            itemPool[i].Hide();
        }
    }
}
