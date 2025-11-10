using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LawnCtrl : ObjectController
{
    public Action OnClicked;
    protected override void OnTapped()
    {
        transform.position += Vector3.left * 1f;
        OnClicked?.Invoke();
    }
    protected override void OnPressed(){}
}
