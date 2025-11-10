using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LanguageSwitcherUI : MonoBehaviour
{
    [Header("Single toggle: On = English, Off = Vietnamese")]
    [SerializeField] private Toggle languageToggle;

    // Tránh vòng lặp khi ta set isOn bằng code
    private bool _updating;

    void Start()
    {
        if (languageToggle == null)
        {
            Debug.LogError("[LanguageSwitcherUI] Toggle reference is missing!");
            return;
        }

        // Lắng nghe người dùng bấm toggle
        languageToggle.onValueChanged.AddListener(OnToggleChanged);

        // Đồng bộ trạng thái ban đầu theo ngôn ngữ hiện tại
        RefreshToggleFromManager();

        // (tuỳ chọn) nếu bạn muốn auto-reflect khi ngôn ngữ đổi ở nơi khác
        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged += RefreshToggleFromManager;
    }

    void OnDestroy()
    {
        if (languageToggle != null)
            languageToggle.onValueChanged.RemoveListener(OnToggleChanged);

        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged -= RefreshToggleFromManager;
    }

    private void OnToggleChanged(bool isOn)
    {
        if (_updating) return;
        if (LanguageManagerJSON.Instance == null) return;

        // Âm thanh button
        SoundManager.PlaySfx(SoundTypes.Button);

        // On = English, Off = Vietnamese
        var newLang = isOn ? Language.English : Language.Vietnamese;

        // Chỉ set nếu khác để tránh gọi sự kiện dư
        if (LanguageManagerJSON.Instance.currentLanguage != newLang)
        {
            LanguageManagerJSON.Instance.SetLanguage(newLang);
        }

        // Cập nhật lại hiển thị (label/icon nếu có)
        RefreshVisualLabel();
    }

    private void RefreshToggleFromManager()
    {
        if (LanguageManagerJSON.Instance == null || languageToggle == null) return;

        _updating = true;
        languageToggle.isOn = (LanguageManagerJSON.Instance.currentLanguage == Language.English);
        _updating = false;

        RefreshVisualLabel();
    }

    private void RefreshVisualLabel()
    {
        var label = languageToggle.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label) label.text = languageToggle.isOn ? "English" : "Tiếng Việt";

        // Hoặc đổi sprite/icon:
        // var img = languageToggle.GetComponentInChildren<Image>();
        // if (img) img.sprite = languageToggle.isOn ? spriteEN : spriteVI;
    }
}
