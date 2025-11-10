using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WakeUp : ShakeGateAuto
{
    [Header("Hiệu ứng phóng to khi lắc")]
    [SerializeField, Min(0.01f)] float scaleMultiplier = 2f;
    [SerializeField, Min(0.01f)] float scaleDuration = 0.5f;

    [Header("Âm thanh (loop)")]
    [SerializeField] AudioClip wakeUpClip;
    [SerializeField, Range(0f,1f)] float fromVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] float toVolume = 1.0f;

    [Header("Win")]
    [SerializeField] private float winDelay = 1f;
    bool playAudioOnEnable = true;
    bool resetOnDisable = true;
    bool stopAudioOnDisable = false;

    Vector3 baseScale;
    AudioSource src;
    bool running;

    void Awake()
    {
        baseScale = transform.localScale;

        if (wakeUpClip)
        {
            src = GetComponent<AudioSource>();
            if (!src) src = gameObject.AddComponent<AudioSource>();
            src.clip = wakeUpClip;
            src.loop = true;
            src.playOnAwake = false;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (src)
        {
            src.volume = Mathf.Clamp01(fromVolume);
            if (playAudioOnEnable && !src.isPlaying) src.Play();
        }

        OnShakeDetected?.AddListener(HandleShake);
    }

    protected override void OnDisable()
    {
        OnShakeDetected?.RemoveListener(HandleShake);

        if (resetOnDisable) transform.localScale = baseScale;
        if (stopAudioOnDisable && src) src.Stop();
        Invoke(nameof(DelayWin), winDelay);

        base.OnDisable();
    }

    void HandleShake()
    {
        if (!running) StartCoroutine(Boost());
    }

    IEnumerator Boost()
    {
        running = true;

        Vector3 from = baseScale;
        Vector3 to = baseScale * Mathf.Max(0.01f, scaleMultiplier);
        float invDur = 1f / Mathf.Max(0.0001f, scaleDuration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * invDur;
            transform.localScale = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }
        transform.localScale = to;

        if (src)
        {
            float v0 = src.volume;
            float v1 = Mathf.Clamp01(toVolume);
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * invDur;
                src.volume = Mathf.LerpUnclamped(v0, v1, t);
                yield return null;
            }
            src.volume = v1;
        }

        running = false;
    }
    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
}
