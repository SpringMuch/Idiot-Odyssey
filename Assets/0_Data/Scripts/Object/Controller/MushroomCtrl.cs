using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MushroomCtrl : ObjectController
{
    [Header("Win")]
    [SerializeField] private float winDelay = 0.5f;
    public void GetMushroom()
    {
        gameObject.SetActive(true);
    }
    protected override void OnTapped(){}
    protected override void OnPressed(){}
    void OnTriggerEnter2D(Collider2D collision)
    {
        Potato potato = collision.GetComponent<Potato>();
        if (potato != null)
        {
            potato.transform.localScale *= 2f;

            GameObjectSpawn.Instance.DeSapwn(this.gameObject);
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
