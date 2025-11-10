using UnityEngine;

public class ExitButton : AbstractButtonUI
{
    public override void OnClick()
    {
        ClosePanel();
    }

    public void ClosePanel()
    {
        UIManager.Instance.SettingPanel.SetActive(false);
        SoundManager.PlaySfx(SoundTypes.Button);
        GameManager.Instance.Resume();
    }
}
