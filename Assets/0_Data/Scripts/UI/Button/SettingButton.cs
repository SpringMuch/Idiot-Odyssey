using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingButton : AbstractButtonUI
{
    
    public override void OnClick()
    {
        UIManager.Instance.SettingPanel.SetActive(true);
        SoundManager.PlaySfx(SoundTypes.Button);
        GameManager.Instance.Pause();
    }
}
