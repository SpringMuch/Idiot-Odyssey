using TMPro;
using UnityEngine;
using DG.Tweening;

public class TextLevel : MonoBehaviour
{
    private TextMeshProUGUI txt;
    private bool inMenu = true;
    private int currentLevel = 1;

    void OnEnable()
    {
        txt = GetComponent<TextMeshProUGUI>();

        GameEventBus.OnOpenLevelSelect += ShowMenu;
        GameEventBus.OnRequestLoadLevel += ShowLevel;
        GameEventBus.OnGameStateChanged += OnGameStateChanged;

        // Lắng nghe sự kiện đổi ngôn ngữ
        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged += Refresh;

        Refresh();
    }

    void OnDisable()
    {
        GameEventBus.OnOpenLevelSelect -= ShowMenu;
        GameEventBus.OnRequestLoadLevel -= ShowLevel;
        GameEventBus.OnGameStateChanged -= OnGameStateChanged;

        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged -= Refresh;
    }

    private void OnGameStateChanged(GameState s)
    {
        if (s == GameState.LevelSelect || s == GameState.MainMenu)
            ShowMenu();
    }

    private void ShowMenu()
    {
        inMenu = true;
        Refresh();
    }

    private void ShowLevel(int levelIndex1Based)
    {
        inMenu = false;
        currentLevel = levelIndex1Based; // không cộng +1
        Refresh();
    }

    private void Refresh()
    {
        if (!txt) return;

        string t;

        if (inMenu)
        {
            // Lấy text từ file ngôn ngữ
            t = (LanguageManagerJSON.Instance != null)
                ? LanguageManagerJSON.Instance.GetText(LocalizationKey.SELECT_MENU)
                : "Select Menu";
        }
        else
        {
            if (LanguageManagerJSON.Instance != null)
                t = LanguageManagerJSON.Instance.GetText(LocalizationKey.LEVEL, currentLevel);
            else
                t = $"Level {currentLevel}";
        }

        if (txt.text == t) return;
        txt.text = t;

        // Hiệu ứng nhẹ khi đổi chữ
        var rt = txt.rectTransform;
        rt.DOKill();
        rt.localScale = Vector3.one;
        rt.DOScale(1.05f, 0.1f).OnComplete(() => rt.DOScale(1f, 0.1f));
    }
}
