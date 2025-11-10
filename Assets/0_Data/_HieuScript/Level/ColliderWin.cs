using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderWin : ObjectController
{
    [Header("Win")]
    [SerializeField] private float winDelay = 1f;
    protected override void OnTapped() { }
    protected override void OnPressed() { }
    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObjectSpawn.Instance.DeSapwn(this.gameObject);
        Invoke(nameof(DelayWin), winDelay);
    }
    protected override void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
    }
    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
}
