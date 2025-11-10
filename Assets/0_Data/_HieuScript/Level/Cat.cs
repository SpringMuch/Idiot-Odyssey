using System.Collections;
using UnityEngine;

// Level 8
[DisallowMultipleComponent]
public class Cat : MonoBehaviour
{
    [Header("Sprite cấu hình")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite happySprite;

    [Header("Win")]
    [SerializeField] private float winDelay = 1f;

    private Sprite defaultSprite;

    void Awake()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            defaultSprite = spriteRenderer.sprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MilkCtrl milk = collision.GetComponent<MilkCtrl>();
        if (milk != null)
        {
            if (spriteRenderer != null && happySprite != null)
                spriteRenderer.sprite = happySprite;
            Invoke(nameof(DelayWin), winDelay);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
        if (spriteRenderer != null)
            spriteRenderer.sprite = defaultSprite;
    }
    
    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
}
