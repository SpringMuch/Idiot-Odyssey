using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplePoinson : ObjectController
{
    [SerializeField] GameObject real;
    [SerializeField] GameObject fake;
    void Start()
    {
        fake.gameObject.SetActive(true);
        real.gameObject.SetActive(false);
    }
    protected override void OnPressed()
    {
    }

    protected override void OnTapped()
    {
        fake.gameObject.SetActive(false);
        real.gameObject.SetActive(true);
    }
}
