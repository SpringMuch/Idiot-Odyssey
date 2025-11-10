using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainButton : AbstractButtonUI
{
    public override void OnClick()
    {
        GameManager.Instance.SetState(GameState.MainMenu);
        SoundManager.PlaySfx(SoundTypes.Button);
    }
}