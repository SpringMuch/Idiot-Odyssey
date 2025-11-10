using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetryLevelButton : AbstractButtonUI
{
    public override void OnClick()
    {
        // ProgressManager.Instance.ResetProgress();
        LevelManager.Instance.RetryLevel();
        SoundManager.PlaySfx(SoundTypes.Button);
        GameManager.Instance.PlayState();
    }
}
