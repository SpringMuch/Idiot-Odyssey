using UnityEngine;
using UnityEngine.Events;
using System;

[DisallowMultipleComponent]
public class FlipDetector : MonoBehaviour
{
    public static FlipDetector Instance { get; private set; }

    [Header("Ngưỡng góc (đỉnh máy hướng xuống)")]
    [Tooltip("Góc tối đa giữa +Y của máy và Vector3.down (độ). Ví dụ 30°.")]
    [Range(5f, 85f)] public float maxAngleFromDown = 30f;

    [Header("Bộ lọc trạng thái 'màn hình đang đứng' (không úp/ngửa)")]
    [Tooltip("Giới hạn |dot(deviceForward, worldUp)| để loại trừ khi máy úp/ngửa sát mặt phẳng.")]
    [Range(0f, 1f)] public float forwardUpAbsDotMax = 0.6f;

    [Header("Làm mượt + Chống double-trigger")]
    [Range(0f, 1f)] public float smoothFactor = 0.15f;
    [Tooltip("Giữ đủ lâu mới xác nhận (giây)")]
    public float confirmSeconds = 0.25f;

    [Header("Events")]
    public UnityEvent onTopDown;     // khi đỉnh máy chĩa xuống đất (enter)
    public UnityEvent onExitTopDown; // khi thoát trạng thái trên

    public event Action<bool> OnTopDownChanged; // arg = isTopDown

    [Header("Editor Test")]
    public bool allowKeyTestInEditor = true;

    // internal
    private bool _isTopDown;
    private float _timer;
    private Vector3 _smoothUp;
    private Vector3 _smoothForward;
    private bool _init;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (allowKeyTestInEditor && Input.GetKeyDown(KeyCode.F))
        {
            SetStateImmediate(!_isTopDown);
            return;
        }
#endif
        // Lấy hướng máy
        Vector3 devUp, devForward;
        GetDeviceAxes(out devUp, out devForward);

        if (!_init)
        {
            _smoothUp = devUp;
            _smoothForward = devForward;
            _init = true;
        }
        else
        {
            _smoothUp = Vector3.Slerp(_smoothUp, devUp, Mathf.Clamp01(smoothFactor));
            _smoothForward = Vector3.Slerp(_smoothForward, devForward, Mathf.Clamp01(smoothFactor));
        }

        // Điều kiện: đỉnh máy gần hướng xuống
        float cosMax = Mathf.Cos(maxAngleFromDown * Mathf.Deg2Rad);
        bool candidateTopDown =
            Vector3.Dot(_smoothUp, Vector3.down) >= cosMax
            // và màn hình không úp/ngửa phẳng:
            && Mathf.Abs(Vector3.Dot(_smoothForward, Vector3.up)) <= forwardUpAbsDotMax;

        // Debounce theo hướng state mong muốn
        if (candidateTopDown && !_isTopDown)
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= confirmSeconds) SetStateImmediate(true);
        }
        else if (!candidateTopDown && _isTopDown)
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= confirmSeconds) SetStateImmediate(false);
        }
        else
        {
            _timer = 0f;
        }
    }

    private void GetDeviceAxes(out Vector3 devUp, out Vector3 devForward)
    {
        if (SystemInfo.supportsGyroscope)
        {
            // Quy đổi attitude sang không gian Unity
            Quaternion q = Input.gyro.attitude;
            q = new Quaternion(q.x, q.y, -q.z, -q.w);
            devUp = q * Vector3.up;
            devForward = q * Vector3.forward;
        }
        else
        {
            // Fallback: dùng gia tốc (up xấp xỉ -acc). Forward ước lượng thô từ ngang.
            Vector3 acc = Input.acceleration;
            devUp = (-acc).normalized;

            // Ước lượng forward: vuông góc với up và gần trục màn hình
            Vector3 right = Vector3.Cross(Vector3.forward, devUp).normalized;
            if (right.sqrMagnitude < 1e-4f) right = Vector3.right;
            devForward = Vector3.Cross(right, devUp).normalized;
        }
    }

    private void SetStateImmediate(bool topDown)
    {
        if (_isTopDown == topDown) return;
        _isTopDown = topDown;
        _timer = 0f;

        if (_isTopDown) onTopDown?.Invoke();
        else            onExitTopDown?.Invoke();

        OnTopDownChanged?.Invoke(_isTopDown);
    }

    public bool IsTopDown() => _isTopDown;
}
