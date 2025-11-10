using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddHintButton : AbstractButtonUI
{
    public override void OnClick()
    {
        AddHint();
        SoundManager.PlaySfx(SoundTypes.Button);
    }
    private void AddHint()
    {
        ProgressManager.Instance.AddHint(1000);
    }
}
