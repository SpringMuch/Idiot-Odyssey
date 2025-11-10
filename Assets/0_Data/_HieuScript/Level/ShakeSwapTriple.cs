using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
public class ShakeSwapTriple : MonoBehaviour
{
    [Header("Children (auto-find theo tên nếu để trống)")]
    [SerializeField] private GameObject sleepCat;
    [SerializeField] private GameObject wakeCat;
    [SerializeField] private GameObject clock;

    [Header("Shake config")]
    [SerializeField] private float requiredIntensity = 10f;
    [SerializeField] private bool  oneShot = true;

    [Header("Simulate trong Editor (Space)")]
    [SerializeField] private bool simulateInEditor = true;
    [SerializeField] private KeyCode simulateKey = KeyCode.Space;

    [Header("Scale Clock")]
    [SerializeField, Min(1.01f)] private float cScaleMultiplier  = 1.4f;
    [SerializeField, Min(0f)]     private float cScaleDuration   = 0.35f;
    [SerializeField]              private Ease  cScaleEase       = Ease.OutBack;
    [SerializeField]              private bool  yoyoBack         = false;
    [SerializeField, Min(0f)]     private float yoyoBackDelay    = 0.0f;
    [SerializeField, Min(0f)]     private float yoyoBackDuration = 0.25f;

    [Header("Win Delay")]
    [SerializeField, Min(0f)] private float winDelay = 2;
    [SerializeField]          private bool  useUnscaledTime = true;

    [Header("Âm thanh (tùy chọn)")]
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f,1f)] private float startVolume   = 0.3f;
    [SerializeField, Range(0f,1f)] private float targetVolume  = 1f;
    [SerializeField, Min(0f)]      private float volumeDuration = 0.35f;

    // cache
    private AudioSource _loopSrc;
    private Vector3     _clockBaseScale;
    private bool        _done;

    void Awake()
    {
        if (sleepCat == null) sleepCat = transform.Find("SleepCat")?.gameObject;
        if (wakeCat  == null) wakeCat  = transform.Find("WakeCat") ?.gameObject;
        if (clock    == null) clock    = transform.Find("Clock")   ?.gameObject;

        if (clock != null) _clockBaseScale = clock.transform.localScale;

        if (clip != null)
        {
            _loopSrc = GetComponent<AudioSource>();
            if (_loopSrc == null) _loopSrc = gameObject.AddComponent<AudioSource>();
            _loopSrc.playOnAwake = false;
            _loopSrc.loop        = true;
            _loopSrc.clip        = clip;
            _loopSrc.volume      = startVolume;
        }
    }

    void OnEnable()
    {
        // Reset trạng thái
        _done = false;
        if (sleepCat) sleepCat.SetActive(true);
        if (wakeCat)  wakeCat.SetActive(false);
        if (clock) clock.transform.localScale = _clockBaseScale;

        if (_loopSrc != null)
        {
            _loopSrc.volume = startVolume;
            if (!_loopSrc.isPlaying) _loopSrc.Play();
        }

        // Đăng ký sự kiện rung
        if (ShakeDetector.Instance != null)
        {
            ShakeDetector.Instance.OnShake += HandleShake;
        }
        else
        {
            Debug.LogWarning("[ShakeSwapTriple] ShakeDetector.Instance chưa khởi tạo!");
        }
    }

    void OnDisable()
    {
        if (ShakeDetector.Instance != null)
            ShakeDetector.Instance.OnShake -= HandleShake;

        if (_loopSrc != null)
        {
            _loopSrc.DOKill();
            _loopSrc.volume = targetVolume;
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (simulateInEditor && Input.GetKeyDown(simulateKey))
            HandleShake(Vector2.up, requiredIntensity + 1);
#endif
    }

    private void HandleShake(Vector2 dir, float intensity)
    {
        if (_done && oneShot) return;
        if (intensity < requiredIntensity) return;

        _done = true;

        // Scale đồng hồ
        if (clock)
        {
            var t = clock.transform;
            t.DOKill();
            var target = _clockBaseScale * cScaleMultiplier;

            if (yoyoBack)
            {
                var seq = DOTween.Sequence().SetUpdate(useUnscaledTime);
                seq.Append(t.DOScale(target, cScaleDuration).SetEase(cScaleEase).SetUpdate(useUnscaledTime));
                if (yoyoBackDelay > 0f) seq.AppendInterval(yoyoBackDelay);
                seq.Append(t.DOScale(_clockBaseScale, yoyoBackDuration).SetEase(Ease.OutQuad).SetUpdate(useUnscaledTime));
                seq.Play();
            }
            else
            {
                t.DOScale(target, cScaleDuration)
                 .SetEase(cScaleEase)
                 .SetUpdate(useUnscaledTime);
            }
        }

        if (sleepCat) sleepCat.SetActive(false);
        if (wakeCat)  wakeCat.SetActive(true);

        if (_loopSrc != null)
        {
            _loopSrc.DOKill();
            _loopSrc.DOFade(targetVolume, volumeDuration)
                    .SetUpdate(useUnscaledTime);
        }

        DOVirtual.DelayedCall(winDelay, () =>
        {
            SoundManager.PlaySfx(SoundTypes.Win);
            GameManager.Instance.Win();
            GameEventBus.RaiseGameWon();
        }, useUnscaledTime);

        if (oneShot)
            enabled = false;
    }
}
