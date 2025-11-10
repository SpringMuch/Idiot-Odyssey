using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class ShakeGateAuto : MonoBehaviour
{
    [Header("Cấu hình")]
    [Tooltip("Ngưỡng cường độ rung (g-force) để kích hoạt sự kiện")]
    public float requiredIntensity = 2.5f;

    [Tooltip("Chỉ kích hoạt 1 lần rồi tắt script?")]
    public bool oneShot = false;

    [Header("Sự kiện UnityEvent")]
    [Tooltip("Sẽ được gọi khi rung đủ mạnh.")]
    public UnityEvent OnShakeDetected;

    private bool _hasTriggered = false;

    // Đổi private -> protected virtual
    protected virtual void OnEnable()
    {
        StartCoroutine(WaitAndSubscribe());
    }

    private IEnumerator WaitAndSubscribe()
    {
        while (ShakeDetector.Instance == null)
            yield return null;

        ShakeDetector.Instance.OnShake += HandleShake;
    }

    // Đổi private -> protected virtual
    protected virtual void OnDisable()
    {
        if (ShakeDetector.Instance != null)
            ShakeDetector.Instance.OnShake -= HandleShake;
    }

    private void HandleShake(Vector2 dir, float intensity)
    {
        if (_hasTriggered && oneShot) return;

        if (intensity >= requiredIntensity)
        {
            OnShakeDetected?.Invoke();
            _hasTriggered = true;

            if (oneShot)
            {
                if (ShakeDetector.Instance != null)
                    ShakeDetector.Instance.OnShake -= HandleShake;
                enabled = false;
            }
        }
    }
}
