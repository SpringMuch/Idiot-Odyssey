using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class ShakeToWinSprite : MonoBehaviour
{
    [Header("Sprite khi shake")]
    [SerializeField] private Sprite shakeSprite;

    [Header("Win")]
    [SerializeField] private float winDelay = 1f;
    [SerializeField] private bool isWin = true;

    private SpriteRenderer sr;
    private bool changed;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        var shaker = ShakeDetector.Instance ?? FindObjectOfType<ShakeDetector>();
        if (shaker != null)
            shaker.OnShake += OnShake;
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
        var shaker = ShakeDetector.Instance ?? FindObjectOfType<ShakeDetector>();
        if (shaker != null)
            shaker.OnShake -= OnShake;
    }

    private void OnShake(Vector2 dir, float intensity)
    {
        if (changed || !shakeSprite) return;
        changed = true;

        sr.sprite = shakeSprite;
        if (isWin)
        {
            Invoke(nameof(DelayWin), winDelay);
        }
        
    }

    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
}
