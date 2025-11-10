using TMPro;
using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
public class LevelQuestionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questionText;
    string _last;

    void Reset()
    {
        if (!questionText) questionText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (!questionText) questionText = GetComponent<TMP_Text>();

        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelLoaded += HandleLevelLoaded;
        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged += RefreshFromCurrent;

        RefreshFromCurrent();
    }

    void OnDisable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelLoaded -= HandleLevelLoaded;

        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged -= RefreshFromCurrent;
    }

    private void HandleLevelLoaded(int levelIndex, LevelSO so) => ApplyQuestion(levelIndex, so);

    private void RefreshFromCurrent()
    {
        var pm = ProgressManager.Instance;
        if (pm == null || pm.Progress == null) { ApplyQuestion(-1, null); return; }

        int idx = pm.Progress.currentLevel;
        var ld = pm.Progress.GetLevelData(idx);
        var so = (ld != null) ? ld.levelSO : null;

        ApplyQuestion(idx, so);
    }

    private void ApplyQuestion(int levelIndex, LevelSO so)
    {
        if (questionText == null || LanguageManagerJSON.Instance == null) return;

        string next = so != null ? LanguageManagerJSON.Instance.GetText(so.key) : "";
        if (next == _last) { questionText.text = next; return; }

        questionText.text = next;
        _last = next;

        // punch scale
        var rt = questionText.rectTransform;
        rt.DOKill();
        rt.localScale = Vector3.one;
        Sequence s = DOTween.Sequence();
        s.Append(rt.DOScale(1.06f, 0.08f));
        s.Append(rt.DOScale(1f, 0.10f));
    }
}
