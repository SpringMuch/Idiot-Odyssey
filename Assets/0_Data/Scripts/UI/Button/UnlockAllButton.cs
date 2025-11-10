using UnityEngine;

public class UnlockAllButton : AbstractButtonUI
{
    public override void OnClick()
    {
        UnlockAll();
        SoundManager.PlaySfx(SoundTypes.Button);
    }
    public void UnlockAll()
    {
        if (ProgressManager.Instance == null || ProgressManager.Instance.Progress == null)
        {
            Debug.LogWarning("[UnlockAllLevels] ProgressManager chưa khởi tạo hoặc progress null.");
            return;
        }

        var progress = ProgressManager.Instance.Progress;

        foreach (var level in progress.levels)
        {
            if (level != null)
                level.isUnlocked = true;
        }

        progress.highestLevel = progress.levels.Count - 1;
        ProgressManager.Instance.Save();
    }
}
