using UnityEngine;
using TMPro;
using System;

public class DailyHintTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    void Update()
    {
        if (timerText == null) return;

        if (!PlayerPrefs.HasKey("NEXT_HINT_TIME"))
        {
            timerText.text = "Nhận miễn phí!";
            return;
        }

        DateTime nextUtc;
        if (!DateTime.TryParse(PlayerPrefs.GetString("NEXT_HINT_TIME"), out nextUtc))
        {
            timerText.text = "Nhận miễn phí!";
            return;
        }

        var remain = nextUtc - DateTime.UtcNow;
        if (remain.TotalSeconds <= 0)
        {
            timerText.text = "Nhận miễn phí!";
        }
        else
        {
            timerText.text = $"{remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}";
        }
    }
}
