using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class FlipGateAuto : MonoBehaviour
{
    [Header("Cấu hình")]
    public bool oneShot = false;

    [Header("Events")]
    public UnityEvent OnTopDown;     // đỉnh máy chĩa xuống đất
    public UnityEvent OnExitTopDown; // rời trạng thái trên

    private bool _hasTriggered;

    void OnEnable()  { StartCoroutine(WaitAndSubscribe()); }
    void OnDisable()
    {
        if (FlipDetector.Instance != null)
            FlipDetector.Instance.OnTopDownChanged -= HandleChanged;
    }

    private IEnumerator WaitAndSubscribe()
    {
        while (FlipDetector.Instance == null) yield return null;
        FlipDetector.Instance.OnTopDownChanged += HandleChanged;
    }

    private void HandleChanged(bool isTopDown)
    {
        if (_hasTriggered && oneShot) return;

        if (isTopDown) OnTopDown?.Invoke();
        else           OnExitTopDown?.Invoke();

        _hasTriggered = true;

        if (oneShot)
        {
            if (FlipDetector.Instance != null)
                FlipDetector.Instance.OnTopDownChanged -= HandleChanged;
            enabled = false;
        }
    }
}
