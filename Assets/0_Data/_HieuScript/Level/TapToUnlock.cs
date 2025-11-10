using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class TapToUnlock : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite swappedSprite;

    [Header("Tham chiếu Rocket")]
    [SerializeField] private JettWin jett;
    [SerializeField] private Collider2D Jett;
    [SerializeField] private bool oneShot = true;

    private bool done;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnMouseUpAsButton()
    {
        if (done && oneShot) return;
        if (targetRenderer && swappedSprite) targetRenderer.sprite = swappedSprite;
        if (jett) jett.Unlock();
        if (jett) jett.enabled = true;
        done = true;
    }
}
