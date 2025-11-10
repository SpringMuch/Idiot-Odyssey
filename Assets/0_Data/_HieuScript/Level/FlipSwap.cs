using System.Collections;
using UnityEngine;


// Level 9
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class FlipSwap : MonoBehaviour
{
    [Header("Renderer mục tiêu (để trống = lấy trên chính object)")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Sprite khi xoay (TopDown)")]
    [SerializeField] private Sprite flippedSprite;

    [Header("Tùy chọn")]
    [Tooltip("Nếu bật, khi rời trạng thái TopDown sẽ tự trả về sprite mặc định.")]
    [SerializeField] private bool revertOnExitTopDown = false;

    [Tooltip("Chỉ cho phép thắng 1 lần duy nhất.")]
    [SerializeField] private bool triggerOnce = true;
    [Header("Win")]
    [SerializeField] private float winDelay = 1f;

    private Sprite _defaultSprite;
    private bool _subscribed;
    private bool _won;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<SpriteRenderer>();
        if (!targetRenderer)
        {
            enabled = false;
            return;
        }

        _defaultSprite = targetRenderer.sprite;
    }

    void OnEnable()
    {
        _won = false;

        if (revertOnExitTopDown && targetRenderer) 
            targetRenderer.sprite = _defaultSprite;

        StartCoroutine(WaitAndSubscribe());
    }

    void OnDisable()
    {
        Unsubscribe();

        CancelInvoke(nameof(DelayWin));

        if (targetRenderer) 
            targetRenderer.sprite = _defaultSprite;
    }

    private IEnumerator WaitAndSubscribe()
    {
        while (FlipDetector.Instance == null) yield return null;

        FlipDetector.Instance.OnTopDownChanged += HandleTopDownChanged;
        _subscribed = true;

        HandleTopDownChanged(FlipDetector.Instance.IsTopDown());
    }

    private void Unsubscribe()
    {
        if (_subscribed && FlipDetector.Instance != null)
        {
            FlipDetector.Instance.OnTopDownChanged -= HandleTopDownChanged;
        }
        _subscribed = false;
    }

    private void HandleTopDownChanged(bool isTopDown)
    {
        if (!targetRenderer) return;

        if (isTopDown)
        {
            if (flippedSprite)
                targetRenderer.sprite = flippedSprite;

            if (!_won)
            {
                _won = true;
                Invoke(nameof(DelayWin), winDelay);

                if (triggerOnce)
                    Unsubscribe();
            }
        }
        else
        {
            if (revertOnExitTopDown)
                targetRenderer.sprite = _defaultSprite;

            if (!triggerOnce)
                _won = false;
        }
    }
    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameEventBus.RaiseGameWon();
        GameManager.Instance.Win();
    }
}
