using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class TapSwap : MonoBehaviour, IPointerDownHandler
{
    [Header("Cấu hình sprite")]
    [SerializeField] private Sprite changeSprite;

    [Header("Số lần tap cần để thắng")]
    [SerializeField, Min(1)] private int requiredTaps = 1;

    [Header("Win")]
    [SerializeField] private float winDelay = 1f;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool isWin = true;

    [Header("Anti-sticky (optional)")]
    [SerializeField, Min(0f)] private float inputBlockSeconds = 0.15f;

    private SpriteRenderer sr;
    private Sprite defaultSprite;

    private bool hasWon;
    private int tapCount;

    private int lastHandledFrame = -1;
    private float acceptInputAt = 0f;
    private uint runId = 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultSprite = sr.sprite;
    }

    void OnEnable()
    {
        CancelInvoke();
        StopAllCoroutines();

        runId++;
        sr.sprite = defaultSprite;
        tapCount = 0;
        hasWon = false;
        lastHandledFrame = -1;

        acceptInputAt = Time.unscaledTime + inputBlockSeconds;
    }

    void OnDisable()
    {
        CancelInvoke();
        StopAllCoroutines();

        // if (sr != null) sr.sprite = defaultSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (lastHandledFrame == Time.frameCount) return;
        lastHandledFrame = Time.frameCount;
        if (Time.unscaledTime < acceptInputAt) return;

        HandleTap();
    }

    public void OnMouseDown()
    {
        if (lastHandledFrame == Time.frameCount) return;
        lastHandledFrame = Time.frameCount;
        if (Time.unscaledTime < acceptInputAt) return;

        HandleTap();
    }

    private void HandleTap()
    {
        if (hasWon) return;

        tapCount++;

        if (tapCount >= requiredTaps)
        {
            sr.sprite = (sr.sprite == defaultSprite) ? changeSprite : defaultSprite;

            if (isWin)
            {
                hasWon = true;
                uint thisRun = runId;
                StartCoroutine(CoDelayWin(thisRun));
            }
        }
        else
        {
            SoundManager.PlaySfx(SoundTypes.Click);
        }
    }

    private IEnumerator CoDelayWin(uint thisRun)
    {
        float t0 = Time.unscaledTime;
        while (Time.unscaledTime - t0 < winDelay)
            yield return null;
        if (thisRun != runId) yield break;

        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
        SoundManager.PlaySfx(SoundTypes.Win);

        if (triggerOnce) enabled = false;
    }
}
