using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Level 8
public class MilkCtrl : ObjectController
{
    public void GetMilk()
    {
        gameObject.SetActive(true);
    }
    protected override void OnTapped(){}
    protected override void OnPressed(){}
    void OnTriggerEnter2D(Collider2D collision)
    {
        Cat cat = collision.GetComponent<Cat>();
        if (cat != null)
        {
            GameObjectSpawn.Instance.DeSapwn(this.gameObject);
        }
    }
}
