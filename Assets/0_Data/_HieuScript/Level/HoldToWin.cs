using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class HoldToWin : MonoBehaviour
{
    [Header("Thời gian cần giữ (giây)")]
    [SerializeField, Min(0.1f)] private float holdDuration = 2f;

    [Header("Sprite trạng thái")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite tapSprite;
    [SerializeField] private Sprite holdSprite;

    [Header("Thời gian nháy Tap (giây)")]
    [SerializeField, Min(0.02f)] private float tapFlashSeconds = 0.5f;
    [Header("Win")]
    [SerializeField] private float winDelay = 1f;

    private float holdTime;
    private bool completed;
    private bool isTouching;
    private float tapHideAt = -1f;
    private Sprite defaultSprite;

    void Awake()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) defaultSprite = spriteRenderer.sprite;
    }

    void OnEnable()
    {
        holdTime = 0f;
        completed = false;
        isTouching = false;
        tapHideAt = -1f;
        if (spriteRenderer) spriteRenderer.sprite = defaultSprite;
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
        if (spriteRenderer) spriteRenderer.sprite = defaultSprite;
    }

    void Update()
    {
        if (completed) return;

        bool touchingNow = false;
        Vector2 pos = Vector2.zero;

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            touchingNow = IsTouchingThis(pos);
        }
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            pos = Camera.main.ScreenToWorldPoint(t.position);
            if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
                touchingNow = IsTouchingThis(pos);
        }
#endif

        if (touchingNow && !isTouching)
        {
            isTouching = true;
            holdTime = 0f;

            if (spriteRenderer && tapSprite)
            {
                spriteRenderer.sprite = tapSprite;
                tapHideAt = Time.time + tapFlashSeconds;
            }
            else
            {
                tapHideAt = -1f;
            }
        }

        if (isTouching && touchingNow)
        {
            if (!completed && tapHideAt > 0f && Time.time >= tapHideAt)
            {
                if (spriteRenderer) spriteRenderer.sprite = defaultSprite;
                tapHideAt = -1f;
            }

            holdTime += Time.deltaTime;
            if (holdTime >= holdDuration)
            {
                completed = true;
                if (spriteRenderer && holdSprite) spriteRenderer.sprite = holdSprite;
                Invoke(nameof(DelayWin), winDelay);
            }
        }

        // Thả tay hoặc trượt ra ngoài object
        if (!touchingNow && isTouching)
        {
            isTouching = false;
            holdTime = 0f;
            tapHideAt = -1f;
            if (!completed && spriteRenderer) spriteRenderer.sprite = defaultSprite;
        }
    }

    private bool IsTouchingThis(Vector2 worldPos)
    {
        Collider2D col = Physics2D.OverlapPoint(worldPos);
        return col != null && col.gameObject == gameObject;
    }

    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
}
