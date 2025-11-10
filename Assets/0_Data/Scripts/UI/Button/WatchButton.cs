using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button), typeof(Image))]
public class WatchButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("General")]
    [SerializeField] private bool playIdleMotion = true;
    [SerializeField] private float idleScaleAmplitude = 0.03f;      // 3%
    [SerializeField] private float idleTiltAmplitude  = 3f;         // độ nghiêng
    [SerializeField] private float idlePeriod         = 2.2f;        // giây

    [Header("Hover")]
    [SerializeField] private float hoverScale   = 1.12f;
    [SerializeField] private float hoverTiltZ   = 6f;
    [SerializeField] private float hoverTime    = 0.25f;
    [SerializeField] private Ease  hoverEase    = Ease.OutBack;

    [Header("Click")]
    [SerializeField] private float pressScale   = 0.92f;
    [SerializeField] private float pressTime    = 0.14f;
    [SerializeField] private float punchScale   = 0.08f; // cường độ bật lại
    [SerializeField] private float punchTime    = 0.20f;
    [SerializeField] private int   punchVibrato = 12;

    [Header("Shine Sweep")]
    [SerializeField] private bool   shineEnable     = true;
    [SerializeField] private float  shinePeriod     = 2.8f;
    [SerializeField] private float  shineAngleZ     = 25f;
    [SerializeField] private float  shineWidth      = 0.55f;   // tỉ lệ theo chiều rộng
    [SerializeField] private float  shineAlpha      = 0.55f;

    [Header("Ripple On Click")]
    [SerializeField] private bool   rippleEnable    = true;
    [SerializeField] private Color  rippleColor     = new Color(1f, 0.95f, 0.5f, 0.9f);
    [SerializeField] private float  rippleStartSize = 0.2f; // theo min(width,height)
    [SerializeField] private float  rippleEndSize   = 1.4f;
    [SerializeField] private float  rippleTime      = 0.45f;

    [Header("Optional SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip   clickClip;

    // ---------------- private state ----------------
    Button btn;
    Image  img;
    RectTransform rt;
    Vector3 originalScale;
    Tween idleTween, shineTween, hoverScaleTw, hoverRotTw, pressTw;

    // child runtime
    RectTransform shineRT;
    Image rippleImg;
    RectTransform rippleRT;

    void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        rt  = GetComponent<RectTransform>();
        originalScale = rt.localScale;

        EnsureShine();
        EnsureRipple();
    }

    void OnEnable()
    {
        KillAllTweens();

        if (playIdleMotion) StartIdle();
        if (shineEnable)    StartShine();
    }

    void OnDisable()
    {
        KillAllTweens();
        ResetVisual();
    }

    // ----------------- Runtime children -----------------
    void EnsureShine()
    {
        if (!shineEnable) return;
        if (shineRT != null) return;

        var go = new GameObject("Shine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        shineRT = go.GetComponent<RectTransform>();
        var shineImg = go.GetComponent<Image>();

        shineImg.sprite = img.sprite;
        shineImg.type = Image.Type.Simple;
        shineImg.color = new Color(1f, 1f, 1f, shineAlpha);
        shineImg.raycastTarget = false;

        shineRT.anchorMin = new Vector2(0, 0);
        shineRT.anchorMax = new Vector2(0, 1);
        shineRT.pivot     = new Vector2(0.5f, 0.5f);
        shineRT.sizeDelta = new Vector2(0, 0);
        shineRT.localScale = new Vector3(shineWidth, 1.2f, 1f);
        shineRT.anchoredPosition = new Vector2(-rt.rect.width, 0);
        shineRT.localRotation = Quaternion.Euler(0, 0, shineAngleZ);
    }

    void EnsureRipple()
    {
        if (!rippleEnable) return;
        if (rippleRT != null) return;

        var go = new GameObject("Ripple", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        rippleRT  = go.GetComponent<RectTransform>();
        rippleImg = go.GetComponent<Image>();
        rippleImg.raycastTarget = false;
        rippleImg.color = Color.clear;

        rippleImg.sprite = img.sprite;
        rippleImg.type = Image.Type.Simple;

        rippleRT.anchorMin = rippleRT.anchorMax = new Vector2(0.5f, 0.5f);
        rippleRT.pivot     = new Vector2(0.5f, 0.5f);
        rippleRT.sizeDelta = Vector2.zero;
        rippleRT.localScale = Vector3.zero;
    }
    void StartIdle()
    {
        idleTween = DOTween.Sequence()
            .Append(rt.DOScale(originalScale * (1f + idleScaleAmplitude), idlePeriod * 0.5f).SetEase(Ease.InOutSine))
            .Join(rt.DOLocalRotate(new Vector3(0, 0, idleTiltAmplitude), idlePeriod * 0.5f).SetEase(Ease.InOutSine))
            .Append(rt.DOScale(originalScale * (1f - idleScaleAmplitude), idlePeriod * 0.5f).SetEase(Ease.InOutSine))
            .Join(rt.DOLocalRotate(new Vector3(0, 0, -idleTiltAmplitude), idlePeriod * 0.5f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StartShine()
    {
        if (shineRT == null) return;

        var w = rt.rect.width;
        shineRT.anchoredPosition = new Vector2(-w * 1.1f, 0);

        shineTween = DOTween.Sequence()
            .AppendInterval(0.35f)
            .Append(shineRT.DOAnchorPosX(w * 1.1f, shinePeriod).SetEase(Ease.InOutSine))
            .AppendInterval(0.4f)
            .SetLoops(-1, LoopType.Restart);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverScaleTw?.Kill(); hoverRotTw?.Kill();

        hoverScaleTw = rt.DOScale(originalScale * hoverScale, hoverTime).SetEase(hoverEase);
        hoverRotTw   = rt.DOLocalRotate(new Vector3(0, 0, hoverTiltZ), hoverTime).SetEase(Ease.OutSine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverScaleTw?.Kill(); hoverRotTw?.Kill();

        hoverScaleTw = rt.DOScale(originalScale, hoverTime).SetEase(hoverEase);
        hoverRotTw   = rt.DOLocalRotate(Vector3.zero, hoverTime * 0.9f).SetEase(Ease.InSine);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressTw?.Kill();
        pressTw = rt.DOScale(originalScale * pressScale, pressTime).SetEase(Ease.InOutSine);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // bật lại nhẹ nhàng
        rt.DOScale(originalScale * hoverScale, pressTime * 0.8f).SetEase(Ease.OutBack);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // punch bật lại
        rt.DOPunchScale(Vector3.one * punchScale, punchTime, punchVibrato, 0.9f);

        // ripple
        if (rippleEnable && rippleImg != null)
        {
            rippleImg.color = rippleColor;

            float minSide = Mathf.Min(rt.rect.width, rt.rect.height);
            rippleRT.localScale = Vector3.one * rippleStartSize;
            rippleRT
                .DOScale(rippleEndSize, rippleTime).SetEase(Ease.OutCubic);
            rippleImg
                .DOFade(0f, rippleTime).From(rippleColor.a).SetEase(Ease.OutCubic)
                .OnComplete(() => rippleImg.color = Color.clear);
        }
        if (clickClip && sfxSource) sfxSource.PlayOneShot(clickClip);
    }

    void KillAllTweens()
    {
        idleTween?.Kill(); shineTween?.Kill();
        hoverScaleTw?.Kill(); hoverRotTw?.Kill(); pressTw?.Kill();
    }

    void ResetVisual()
    {
        rt.localScale = originalScale;
        rt.localRotation = Quaternion.identity;
        if (shineRT) shineRT.anchoredPosition = new Vector2(-rt.rect.width, 0);
        if (rippleImg) rippleImg.color = Color.clear;
    }
}
