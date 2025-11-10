using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // dùng cho UGUI Image

[DisallowMultipleComponent]
public class RandomWin : MonoBehaviour
{
    [Header("Mục tiêu hiển thị (tự tìm nếu để trống)")]
    [SerializeField] private SpriteRenderer spriteRenderer;   // 2D renderer (fallback)

    [Header("Danh sách sprite để random")]
    [SerializeField] private List<Sprite> spriteOptions = new();

    [Header("Tuỳ chọn")]
    [Tooltip("Tự random mỗi khi WinCanvas/WinPanel được bật")]
    [SerializeField] private bool autoShowOnEnable = true;

    [Tooltip("Tránh lặp lại cùng 1 sprite ở 2 lần liên tiếp (nếu có >=2 sprite)")]
    [SerializeField] private bool avoidImmediateRepeat = true;

    [Tooltip("Phát SFX thắng (nếu bạn đang dùng SoundManager)")]
    [SerializeField] private bool playWinSfx = true;

    int _lastIndex = -1;      // nhớ lần trước để tránh lặp
    bool _cachedTargets = false;

    void Awake()
    {
        CacheTargetsIfNeeded();
        // Bật component hiển thị sẵn để không bị tắt khi đổi sprite
        if (spriteRenderer) spriteRenderer.enabled = true;
    }

    void OnEnable()
    {
        if (autoShowOnEnable)
            ShowRandom();
    }

    /// <summary>
    /// Gọi hàm này khi mở WinCanvas/WinPanel để đổi sang 1 sprite ngẫu nhiên.
    /// </summary>
    public void ShowRandom()
    {
        CacheTargetsIfNeeded();
        if (spriteOptions == null || spriteOptions.Count == 0) return;

        int idx = PickRandomIndex();
        ApplySprite(spriteOptions[idx]);
        _lastIndex = idx;

        if (playWinSfx)
        {
            // an toàn nếu không có SoundManager
            try { SoundManager.PlaySfx(SoundTypes.Win); } catch { }
        }
    }

    /// <summary>
    /// Debug/QA: ép chọn 1 sprite theo index.
    /// </summary>
    public void ShowIndex(int index)
    {
        CacheTargetsIfNeeded();
        if (spriteOptions == null || spriteOptions.Count == 0) return;
        index = Mathf.Clamp(index, 0, spriteOptions.Count - 1);

        ApplySprite(spriteOptions[index]);
        _lastIndex = index;

        if (playWinSfx)
        {
            try { SoundManager.PlaySfx(SoundTypes.Win); } catch { }
        }
    }

    int PickRandomIndex()
    {
        int count = spriteOptions.Count;
        if (!avoidImmediateRepeat || count < 2)
            return Random.Range(0, count);

        // Tránh trùng _lastIndex (nếu có >=2 sprite)
        int idx;
        do { idx = Random.Range(0, count); }
        while (idx == _lastIndex);
        return idx;
    }

    void ApplySprite(Sprite s)
    {
        if (spriteRenderer)
        {
            spriteRenderer.sprite = s;
            if (!spriteRenderer.enabled) spriteRenderer.enabled = true;
            return;
        }
#if UNITY_EDITOR
        Debug.LogWarning($"{name}: Chưa tìm thấy Image hoặc SpriteRenderer để gán sprite.");
#endif
    }

    void CacheTargetsIfNeeded()
    {
        if (_cachedTargets) return;

        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        _cachedTargets = true;
    }

    // ====== tiện ích nhỏ nếu muốn thao tác danh sách lúc runtime ======
    public void SetSprites(IEnumerable<Sprite> sprites)
    {
        spriteOptions.Clear();
        if (sprites != null) spriteOptions.AddRange(sprites);
        _lastIndex = -1;
    }
    public void AddSprite(Sprite s)
    {
        if (s != null) spriteOptions.Add(s);
    }
    public void ClearSprites()
    {
        spriteOptions.Clear();
        _lastIndex = -1;
    }
}
