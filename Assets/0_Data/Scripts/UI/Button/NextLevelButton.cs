using UnityEngine;

public class NextLevelButton : AbstractButtonUI
{
    public override void OnClick()
    {
        int cur = LevelManager.Instance.CurrentLevelIndex;

        ProgressManager.Instance.CompleteLevel(cur);
        // AdsManager.Instance.ShowInterstitial();
        if (!LevelManager.Instance.HasNextLevel())
        {
            SoundManager.PlaySfx(SoundTypes.Button);
            GameEventBus.RaiseOpenLevelSelect();
            return;
        }

        int next = cur + 1;
        ProgressManager.Instance.SetCurrentLevel(next);
        SoundManager.PlaySfx(SoundTypes.Button);
        GameEventBus.RequestLoadLevel(next);
    }
}
