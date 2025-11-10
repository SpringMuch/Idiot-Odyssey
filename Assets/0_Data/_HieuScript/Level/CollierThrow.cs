using UnityEngine;

public class ColliderThrow : ObjectController
{
    [Header("Win")]
    [SerializeField] private float winDelay = 2f;
    private bool hasWon = false;

    protected override void OnTapped() { }
    protected override void OnPressed() { }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hasWon) return;
        if (IsPointerDown()) return;
        hasWon = true;
        GameObjectSpawn.Instance.DeSapwn(this.gameObject);
        Invoke(nameof(DelayWin), winDelay);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CancelInvoke(nameof(DelayWin));
        hasWon = false;
    }

    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
    private static bool IsPointerDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButton(0);
#else
        return Input.touchCount > 0;
#endif
    }
}
