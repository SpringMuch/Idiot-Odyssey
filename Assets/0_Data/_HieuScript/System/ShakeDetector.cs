using UnityEngine;
using UnityEngine.Events;
using System;

[DisallowMultipleComponent]
public class ShakeDetector : MonoBehaviour
{
    public static ShakeDetector Instance { get; private set; }

    [Header("Cấu hình phát hiện rung")]
    [Tooltip("Ngưỡng độ rung (g-force) để kích hoạt sự kiện.")]
    public float shakeSensitivity = 2.0f;

    [Tooltip("Cho phép bấm phím Space trong Editor để test rung.")]
    public bool allowSpaceKeyInEditor = true;

    [Header("Sự kiện UnityEvent (kéo thả trong Inspector)")]
    [Tooltip("Sự kiện sẽ được gọi khi phát hiện rung đủ mạnh.")]
    public UnityEvent onShake;

    public event Action<Vector2, float> OnShake;

    // Nội bộ
    private Vector3 _prevAcc;
    private bool _initialized;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (allowSpaceKeyInEditor && Input.GetKeyDown(KeyCode.Space))
        {
            onShake?.Invoke();
            OnShake?.Invoke(Vector2.up, shakeSensitivity + 1.0f);
            return;
        }
#endif

        Vector3 acc = Input.acceleration;

        if (!_initialized)
        {
            _prevAcc = acc;
            _initialized = true;
            return;
        }

        Vector3 delta = acc - _prevAcc;
        _prevAcc = acc;

        float intensity = new Vector2(delta.x, delta.y).magnitude * 9.81f;

        if (intensity >= shakeSensitivity)
        {
            Vector2 dir = new Vector2(delta.x, delta.y).normalized;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector2.up;

            onShake?.Invoke();
            OnShake?.Invoke(dir, intensity);
        }
    }
}
