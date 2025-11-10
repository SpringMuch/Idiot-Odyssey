using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : AbstractButtonUI
{
    public override void OnClick()
    {
        GameManager.Instance.SetState(GameState.LevelSelect);
        SoundManager.PlaySfx(SoundTypes.Button);
    }
}
