using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SwapWin : MonoBehaviour
{
    [Header("Sprites mới")]
    [SerializeField] private Sprite parentNewSprite;
    [SerializeField] private Sprite childNewSprite;

    [Header("Renderer mục tiêu")]
    [SerializeField] private SpriteRenderer parentRenderer;
    [SerializeField] private SpriteRenderer childRenderer;
    [SerializeField] private bool includeInactiveChildren = true;

    [Header("Win")]
    [SerializeField] private float winDelay = 1f;

    private bool swapped;

    void Awake()
    {
        if (!parentRenderer) parentRenderer = GetComponent<SpriteRenderer>();
        if (!parentRenderer)
        {
            enabled = false;
            return;
        }

        if (!childRenderer)
        {
            var all = GetComponentsInChildren<SpriteRenderer>(includeInactiveChildren);
            foreach (var r in all)
            {
                if (r != parentRenderer) { childRenderer = r; break; }
            }
        }
    }

    void OnMouseUpAsButton()
    {
        if (swapped) return;

        swapped = true;

        parentRenderer.sprite = parentNewSprite;
        childRenderer.sprite = childNewSprite;

        Invoke(nameof(DelayWin), winDelay);
    }

    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
    }
}
