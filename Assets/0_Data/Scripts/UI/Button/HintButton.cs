using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[DisallowMultipleComponent]
public class HintButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hintTextArea;

    [Header("Config")]
    [SerializeField, Min(0)] private int costPerHint = 10;
    [SerializeField, Min(0f)] private float showSeconds = 2f;
    [SerializeField] private bool autoHide = true;
    [SerializeField] private bool refreshOnLanguageChange = true;

    [Header("Tween")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;

    private Button _button;
    private CanvasGroup _cg;

    void Reset()
    {
        if (!hintTextArea) hintTextArea = GetComponentInChildren<TextMeshProUGUI>();
        if (!_button) _button = GetComponent<Button>();
    }

    void Awake()
    {
        if (!_button) _button = GetComponent<Button>();
        if (!hintTextArea)
            Debug.LogWarning("[HintButton] Chưa gán hintTextArea. Vui lòng kéo TextMeshProUGUI vào Inspector.");
    }

    void OnEnable()
    {
        if (_button) _button.onClick.AddListener(OnClick);

        if (hintTextArea)
        {
            _cg = hintTextArea.GetComponent<CanvasGroup>();
            if (!_cg) _cg = hintTextArea.gameObject.AddComponent<CanvasGroup>();

            hintTextArea.gameObject.SetActive(false);
            _cg.alpha = 0f;
            hintTextArea.text = "";
        }

        if (refreshOnLanguageChange && LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged += HandleLanguageChanged;
    }

    void OnDisable()
    {
        if (_button) _button.onClick.RemoveListener(OnClick);

        if (refreshOnLanguageChange && LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged -= HandleLanguageChanged;
    }

    public void OnClick()
    {
        SoundManager.PlaySfx(SoundTypes.Button);

        bool success = ProgressManager.Instance.UseHint(costPerHint);
        if (!success) { Debug.Log("❌ Không đủ bóng đèn!"); return; }

        var so = HintManager.Instance.GetHintSO();
        string text = GetLocalizedHintText(so);
        if (!hintTextArea) return;

        hintTextArea.text = text;
        hintTextArea.gameObject.SetActive(true);

        _cg.DOKill();
        _cg.alpha = 0f;
        _cg.DOFade(1f, fadeDuration).SetEase(fadeEase);

        if (autoHide && showSeconds > 0f)
        {
            // Fade out sau showSeconds
            DOVirtual.DelayedCall(showSeconds, () =>
            {
                _cg.DOKill();
                _cg.DOFade(0f, fadeDuration).SetEase(fadeEase)
                   .OnComplete(() =>
                   {
                       hintTextArea.gameObject.SetActive(false);
                       hintTextArea.text = "";
                   });
            });
        }
    }

    private string GetLocalizedHintText(HintSO so)
    {
        if (so == null) return "No hint available.";
        if (LanguageManagerJSON.Instance != null)
            return LanguageManagerJSON.Instance.GetText(so.key);
        return so.key.ToString();
    }

    private void HandleLanguageChanged()
    {
        if (!hintTextArea || !hintTextArea.gameObject.activeSelf) return;

        var so = HintManager.Instance != null ? HintManager.Instance.GetHintSO() : null;
        hintTextArea.text = GetLocalizedHintText(so);
    }
}
