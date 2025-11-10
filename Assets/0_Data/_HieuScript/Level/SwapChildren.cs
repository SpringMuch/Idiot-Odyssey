using UnityEngine;
using UnityEngine.Events;

// Level 4
[DisallowMultipleComponent]
public class SwapChildren : MonoBehaviour
{
    public enum Mode { ChildrenToggle, SpriteSwap }

    [Header("Chế độ hoạt động")]
    [SerializeField] private Mode mode = Mode.ChildrenToggle;

    // ===== Mode A: bật/tắt các con =====
    [Header("Mode A - Children Toggle")]
    [Tooltip("Danh sách con là các trạng thái. Chỉ 1 con sẽ bật tại 1 thời điểm.")]
    [SerializeField] private GameObject[] states;

    [SerializeField] private int startIndex = -1;

    [Tooltip("Đảm bảo chỉ có đúng 1 trạng thái bật khi Start.")]
    [SerializeField] private bool normalizeOnStart = true;

    // ===== Mode B: đổi sprite trên cùng 1 SpriteRenderer =====
    [Header("Mode B - Sprite Swap")]
    [SerializeField] private SpriteRenderer targetRenderer; 
    [SerializeField] private Sprite[] sprites;

    [Header("Tuỳ chọn chung")]
    [Tooltip("Có vòng lặp khi vượt quá phần tử cuối không?")]
    [SerializeField] private bool loop = true;

    [Tooltip("Index hiện tại")]
    [SerializeField] private int currentIndex = 0;

    [Header("Sự kiện")]
    public UnityEvent<int> onSwitched; 

    void Awake()
    {
        if (mode == Mode.SpriteSwap && targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Chuẩn hoá trạng thái ban đầu
        if (mode == Mode.ChildrenToggle)
        {
            if (states == null || states.Length == 0) return;

            if (normalizeOnStart)
            {
                if (startIndex >= 0 && startIndex < states.Length)
                {
                    currentIndex = startIndex;
                }
                else
                {
                    // Tự tìm child nào đang active làm start; nếu không có chọn 0
                    int firstActive = -1;
                    for (int i = 0; i < states.Length; i++)
                        if (states[i] != null && states[i].activeSelf) { firstActive = i; break; }

                    currentIndex = (firstActive >= 0) ? firstActive : 0;
                }
                ApplyChildrenState(currentIndex);
            }
            else
            {
                // Không normalize: cố gắng đồng bộ currentIndex theo cái đang active, fallback 0
                int firstActive = -1;
                for (int i = 0; i < states.Length; i++)
                    if (states[i] != null && states[i].activeSelf) { firstActive = i; break; }

                currentIndex = (firstActive >= 0) ? firstActive : Mathf.Clamp(currentIndex, 0, states.Length - 1);
            }
        }
        else // SpriteSwap
        {
            if (sprites != null && sprites.Length > 0)
            {
                currentIndex = Mathf.Clamp(currentIndex, 0, sprites.Length - 1);
                ApplySprite(currentIndex);
            }
        }
    }

    // === Nhận tap chuột / chạm (cần có Collider) ===
    void OnMouseUpAsButton()
    {
        Debug.Log("SwapChildren: OnMouseUpAsButton detected!");
        ToggleNext();
    }

    public void ToggleNext()
    {
        int next = currentIndex + 1;
        int max = (mode == Mode.ChildrenToggle) ? (states?.Length ?? 0) : (sprites?.Length ?? 0);
        if (max <= 0) return;

        if (next >= max)
            next = loop ? 0 : max - 1;

        SetIndex(next);
    }

    public void SetIndex(int index)
    {
        int max = (mode == Mode.ChildrenToggle) ? (states?.Length ?? 0) : (sprites?.Length ?? 0);
        if (max <= 0) return;

        currentIndex = Mathf.Clamp(index, 0, max - 1);

        if (mode == Mode.ChildrenToggle)
            ApplyChildrenState(currentIndex);
        else
            ApplySprite(currentIndex);

        onSwitched?.Invoke(currentIndex);
    }

    private void ApplyChildrenState(int idx)
    {
        if (states == null) return;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == null) continue;
            states[i].SetActive(i == idx);
        }
    }

    private void ApplySprite(int idx)
    {
        if (targetRenderer == null || sprites == null) return;
        targetRenderer.sprite = sprites[idx];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (mode == Mode.ChildrenToggle && states != null && states.Length > 0)
            {
                int max = states.Length;
                int idx = Mathf.Clamp(currentIndex, 0, max - 1);
                // chỉ bật 1 con trong editor để preview
                for (int i = 0; i < max; i++)
                    if (states[i] != null) states[i].SetActive(i == idx);
            }
            else if (mode == Mode.SpriteSwap && targetRenderer != null && sprites != null && sprites.Length > 0)
            {
                int max = sprites.Length;
                int idx = Mathf.Clamp(currentIndex, 0, max - 1);
                targetRenderer.sprite = sprites[idx];
            }
        }
    }
#endif
}
