using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : AbstractButtonUI
{
    public override void OnClick()
    {
        ProgressManager.Instance.ResetProgress();
        SoundManager.PlaySfx(SoundTypes.Button);
    }
}

