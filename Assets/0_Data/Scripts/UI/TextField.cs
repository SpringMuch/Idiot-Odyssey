using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TextField : MonoBehaviour
{
    [SerializeField] private Button trueButton;
    [SerializeField] private Button falseButton;
    [SerializeField] private Transform correctPanel;
    [SerializeField] private Transform wrongPanel;

    [SerializeField] private LevelManager levelManager;
    private TMP_InputField inputField;

    CanvasGroup _cgCorrect, _cgWrong;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        if (correctPanel) { _cgCorrect = correctPanel.gameObject.GetComponent<CanvasGroup>() ?? correctPanel.gameObject.AddComponent<CanvasGroup>(); correctPanel.gameObject.SetActive(false); _cgCorrect.alpha = 0f; }
        if (wrongPanel)   { _cgWrong   = wrongPanel.gameObject.GetComponent<CanvasGroup>()   ?? wrongPanel.gameObject.AddComponent<CanvasGroup>();   wrongPanel.gameObject.SetActive(false);   _cgWrong.alpha = 0f; }
    }

    void Start()
    {
        trueButton.onClick.AddListener(SubmitResult);
        falseButton.onClick.AddListener(DeleteText);
    }

    void DeleteText() => inputField.text = "";

    public void SubmitResult()
    {
        string result = inputField.text;
        if (result == "3")
            TrueResult();
        else
            FalseResult();
    }

    void FalseResult()
    {
        if (!_cgWrong) return;
        wrongPanel.gameObject.SetActive(true);
        var rt = wrongPanel as RectTransform;

        _cgWrong.DOKill(); rt?.DOKill();
        Sequence s = DOTween.Sequence();
        if (rt) { rt.localScale = Vector3.one * 0.95f; s.Join(rt.DOScale(1f, 0.15f).SetEase(Ease.OutBack)); }
        s.Join(_cgWrong.DOFade(1f, 0.15f));
        s.AppendInterval(0.85f);
        s.Append(_cgWrong.DOFade(0f, 0.15f));
        s.OnComplete(() => wrongPanel.gameObject.SetActive(false));
    }

    void TrueResult()
    {
        if (!_cgCorrect) return;
        correctPanel.gameObject.SetActive(true);
        var rt = correctPanel as RectTransform;

        _cgCorrect.DOKill(); rt?.DOKill();
        Sequence s = DOTween.Sequence();
        if (rt) { rt.localScale = Vector3.one * 0.95f; s.Join(rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack)); }
        s.Join(_cgCorrect.DOFade(1f, 0.18f));
        s.AppendInterval(0.8f);
        s.Append(_cgCorrect.DOFade(0f, 0.15f));
        s.OnComplete(() =>
        {
            correctPanel.gameObject.SetActive(false);
            Debug.Log("Next Level");
            // levelManager?.CompleteLevel();
        });
    }
}
