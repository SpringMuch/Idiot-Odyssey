using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class FoodCounter : MonoBehaviour
{
    [Header("Cấu hình")]
    [SerializeField, Min(1)] private int totalRequired = 5;
    [SerializeField, Min(0f)] private float winDelay = 1f;

    private int _count = 0;
    private bool _won = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    public void RegisterFood()
    {
        if (_won) return;

        _count++;
        if (_count >= totalRequired)
        {
            _won = true;
            Invoke(nameof(DelayWin), winDelay);
        }
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
    }

    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameEventBus.RaiseGameWon();
        GameManager.Instance.Win();
    }
}
