using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeseptionLevelText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    private LocalizedTextJSON localizedTextJSON;

    private void Awake() {
        localizedTextJSON = GetComponent<LocalizedTextJSON>();
    }

    void Reset()
    {
        if (!questionText) questionText = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (!questionText) questionText = GetComponent<TextMeshProUGUI>();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded += HandleLevelLoaded;
        }

        TryInitFromCurrentProgress();
    }

    void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded -= HandleLevelLoaded;
        }
    }

    private void HandleLevelLoaded(int levelIndex, LevelSO so)
    {
        ApplyQuestion(levelIndex, so);
    }

    private void TryInitFromCurrentProgress()
    {
        var pm = ProgressManager.Instance;
        if (pm == null || pm.Progress == null) return;

        int idx = pm.Progress.currentLevel;
        var ld = pm.Progress.GetLevelData(idx);
        var so = ld != null ? ld.levelSO : null;

        ApplyQuestion(idx, so);
    }

    private void ApplyQuestion(int levelIndex, LevelSO so)
    {
        if (questionText == null || LanguageManagerJSON.Instance == null) return;

        if (so != null /*&& !string.IsNullOrWhiteSpace(so.levelDeseption)*/)
            questionText.text = LanguageManagerJSON.Instance.GetText(so.key);
        else
            questionText.text = "";
    }
}
