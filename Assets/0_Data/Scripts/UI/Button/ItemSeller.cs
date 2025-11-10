using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSeller : AbstractButtonUI
{
    [SerializeField] private GameObject targetPanel;
    public override void OnClick()
    {
        ShowPanel();
    }
    public void ShowPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            SoundManager.PlaySfx(SoundTypes.Button);
        }
        else
        {
            Debug.LogWarning("Not Found Panel to Show!");
        }
    }
}
