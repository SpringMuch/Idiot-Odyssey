using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CloseButton : AbstractButtonUI
{
    [SerializeField] private GameObject targetPanel;
    public override void OnClick()
    {
        ClosePanel();
    }

    public void ClosePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
            SoundManager.PlaySfx(SoundTypes.Button);
        }
        else
        {
            Debug.LogWarning("Not Found Panel to Close!");
        }
    }
}
