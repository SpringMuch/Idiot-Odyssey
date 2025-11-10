using UnityEngine;
using System;

[DisallowMultipleComponent]
public class ScaleToWin : ObjectController
{
    public enum ScaleCondition { ScaleUp, ScaleDown }

    [Header("Điều kiện thắng")]
    [SerializeField] private ScaleCondition condition = ScaleCondition.ScaleUp;

    [SerializeField, Min(0.1f)] private float requiredFactor = 1.2f;

    [SerializeField, Min(0f)] private float tolerance = 0.01f;
    [SerializeField] private string targetComp = "";

    [Header("Win")]
    [SerializeField] private float winDelay = 1f;

    private Vector3 _baseLossyAbs;
    private bool _won;

    protected override void OnEnable()
    {
        base.OnEnable();
        _baseLossyAbs = GetAbsLossy(transform);
        _won = false;
    }

    public void GetScaleToWin()
    {
        gameObject.SetActive(true);
        _baseLossyAbs = GetAbsLossy(transform);
        _won = false;
    }

    protected override void OnTapped() { }
    protected override void OnPressed() { }

    private static Vector3 GetAbsLossy(Transform t)
    {
        var s = t.lossyScale;
        return new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    private bool HasReachedRequiredScale()
    {
        var cur = GetAbsLossy(transform);

        float fx = _baseLossyAbs.x > Mathf.Epsilon ? cur.x / _baseLossyAbs.x : 1f;
        float fy = _baseLossyAbs.y > Mathf.Epsilon ? cur.y / _baseLossyAbs.y : 1f;

        float factorUp   = Mathf.Max(fx, fy);
        float factorDown = Mathf.Min(fx, fy);

        switch (condition)
        {
            case ScaleCondition.ScaleUp:
                if (factorUp <= 1f + tolerance) return false;
                return factorUp >= requiredFactor - tolerance;
            case ScaleCondition.ScaleDown:
                float req = Mathf.Min(requiredFactor, 0.999f);
                if (factorDown >= 1f - tolerance) return false;
                return factorDown <= req + tolerance;

            default:
                return false;
        }
    }

    private bool HasTargetComponent(Collider2D col)
    {
        if (string.IsNullOrEmpty(targetComp)) return false;

        // Duyệt các component theo tên kiểu (giữ nguyên cách bạn đang dùng)
        var components = col.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            if (string.Equals(comp.GetType().Name, targetComp, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    protected override void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
    }

    private void DelayWin()
    {
        if (_won) return;
        _won = true;

        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
        SoundManager.PlaySfx(SoundTypes.Win);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_won) return; // chặn nhiều lần

        if (HasTargetComponent(collision) && HasReachedRequiredScale())
        {
            GameObjectSpawn.Instance.DeSapwn(gameObject);
            Invoke(nameof(DelayWin), winDelay);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        tolerance = Mathf.Max(0f, tolerance);

        if (condition == ScaleCondition.ScaleDown && requiredFactor >= 1f)
        {
            requiredFactor = 0.7f;
        }
        if (condition == ScaleCondition.ScaleUp && requiredFactor <= 1f)
        {
            requiredFactor = Mathf.Max(1.1f, requiredFactor);
        }
    }
#endif
}