using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HoldCtrl : ObjectController
{
    [Header("Thời gian")]
    [SerializeField] float holdSeconds = 2.0f;
    [SerializeField] float tapFlashSeconds = 0.5f;

    [Header("Renderer + Sprites")]
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] Sprite tapSprite;
    [SerializeField] Sprite changedSprite;
    [Header("Win")]
    [SerializeField] private float winDelay = 0.5f;
    Sprite defaultSprite;
    bool hasWon;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<SpriteRenderer>();
        if (targetRenderer) defaultSprite = targetRenderer.sprite;
    }

    protected override void OnTapped()
    {
        if (hasWon || !targetRenderer) return;

        targetRenderer.sprite = tapSprite ? tapSprite : defaultSprite;
        CancelInvoke(nameof(ResetSprite));
        Invoke(nameof(ResetSprite), tapFlashSeconds);
    }

    protected override void OnPressed()
    {
        if (hasWon) return;

        CancelInvoke(nameof(DoWin));
        Invoke(nameof(DoWin), holdSeconds);
    }

    void DoWin()
    {
        if (hasWon) return;

        if (targetRenderer && changedSprite)
            targetRenderer.sprite = changedSprite;

        Invoke(nameof(DelayWin), winDelay);
    }

    void ResetSprite()
    {
        if (!hasWon && targetRenderer)
            targetRenderer.sprite = defaultSprite;
    }

    protected override void OnDisable()
    {
        CancelInvoke();
        hasWon = false;
        if (targetRenderer) targetRenderer.sprite = defaultSprite;
    }

    private void DelayWin()
    {
        hasWon = true;
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
}
