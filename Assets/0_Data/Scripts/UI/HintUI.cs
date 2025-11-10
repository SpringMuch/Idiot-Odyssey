using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HintUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI hintTextArea;
    [SerializeField] private TextMeshProUGUI falseTextArea;

    [Header("Tween")]
    [SerializeField] float fadeDur = 0.2f;
    [SerializeField] float showSeconds = 2f;
    [SerializeField] float clickScale = 0.92f;
    [SerializeField] private float falseShowSeconds = 2f;

    CanvasGroup _cg;

    private void Awake()
    {
        hintText = GetComponentInChildren<TextMeshProUGUI>();
        button   = GetComponentInChildren<Button>();
    }

    void Reset()
    {
        hintText = GetComponentInChildren<TextMeshProUGUI>();
        button   = GetComponentInChildren<Button>();
    }

    private void Start() {
        button.onClick.AddListener(HintHandle);
        if (hintTextArea != null)
        {
            _cg = hintTextArea.GetComponent<CanvasGroup>();
            if (!_cg) _cg = hintTextArea.gameObject.AddComponent<CanvasGroup>();
            hintTextArea.gameObject.SetActive(false);
            _cg.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.OnHintCountChanged += UpdateUI;
            UpdateUI(ProgressManager.Instance.GetHintCount());
        }
    }
    void Update()
    {
        falseShowSeconds -= Time.deltaTime;
        if (falseShowSeconds <= 0)
        {
            falseTextArea.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnHintCountChanged -= UpdateUI;
    }

    private void UpdateUI(int newCount)
    {
        if (hintText != null) hintText.text = newCount.ToString();
    }

    public void HintHandle()
    {
        // bounce nút
        var brt = button.transform as RectTransform;
        if (brt)
        {
            brt.DOKill();
            Sequence b = DOTween.Sequence();
            b.Append(brt.DOScale( clickScale, 0.08f));
            b.Append(brt.DOScale(1f, 0.08f));
        }

        if (LevelManager.Instance.CurrentLevelIndex <= 0) return;
        if (HintManager.Instance == null) return;
        if (ProgressManager.Instance == null) return;

        bool success = ProgressManager.Instance.UseHint(10);
        if (success)
        {
            if (!_cg) return;
            int index = ProgressManager.Instance.Progress.currentLevel;
            if (index < 0) return;

            HintSO key = HintManager.Instance?.GetHintSO();
            if (key == null) return;

            string hint = LanguageManagerJSON.Instance?.GetText(key.key);
            hintTextArea.text = string.IsNullOrEmpty(hint) ? "No hint available!" : hint;
            

            hintTextArea.gameObject.SetActive(true);
            _cg.DOKill();
            _cg.alpha = 0f;
            Sequence s = DOTween.Sequence();
            s.Append(_cg.DOFade(1f, fadeDur));
            s.AppendInterval(showSeconds);
            s.Append(_cg.DOFade(0f, fadeDur));
            s.OnComplete(() =>
            {
                hintTextArea.gameObject.SetActive(false);
                hintTextArea.text = "";
            });
            Debug.Log("Always true");
        }
        else
        {
            falseTextArea.gameObject.SetActive(true);
            falseTextArea.text = "Isn't enough hint, Please watch ads!";
            falseShowSeconds = 2f;
        }
    }
}
