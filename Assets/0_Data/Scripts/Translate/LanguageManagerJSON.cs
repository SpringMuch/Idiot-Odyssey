using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-800)]
[Serializable]
public class LocalizationItem
{
    public string key;
    public string en;
    public string vi;
}

[Serializable]
public class LocalizationFile
{
    public List<LocalizationItem> items;
}

public enum Language
{
    English = 0,
    Vietnamese = 1,
}

public class LanguageManagerJSON : MonoBehaviour
{
    public static LanguageManagerJSON Instance { get; private set; }

    public Language currentLanguage = Language.English;

    public event Action OnLanguageChanged;

    private const string PLAYERPREFS_KEY = "LANG_JSON";
    private Dictionary<string, LocalizationItem> map;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        LoadFromResources();
        LoadSavedLanguage();
    }

    void LoadFromResources()
    {
        TextAsset ta = Resources.Load<TextAsset>("localization");
        if (ta == null)
        {
            Debug.LogError("❌ Localization file not found! Expected at: Resources/localization.json");
            map = new Dictionary<string, LocalizationItem>();
            return;
        }

        LocalizationFile file = JsonUtility.FromJson<LocalizationFile>(ta.text);
        map = new Dictionary<string, LocalizationItem>(StringComparer.OrdinalIgnoreCase);

        if (file != null && file.items != null)
        {
            foreach (var it in file.items)
            {
                if (!string.IsNullOrEmpty(it.key))
                    map[it.key] = it;
            }
        }
    }

    void LoadSavedLanguage()
    {
        int saved = PlayerPrefs.GetInt(PLAYERPREFS_KEY, (int)currentLanguage);
        currentLanguage = (Language)Mathf.Clamp(saved, 0, Enum.GetValues(typeof(Language)).Length - 1);
    }

    public void SetLanguage(Language lang)
    {
        if (currentLanguage == lang) return;
        currentLanguage = lang;
        PlayerPrefs.SetInt(PLAYERPREFS_KEY, (int)lang);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();
    }

    public string GetText(LocalizationKey key, params object[] args)
    {
        string keyStr = key.ToString();
        if (!map.TryGetValue(keyStr, out var item))
            return keyStr; // fallback nếu không tìm thấy key

        string text = currentLanguage switch
        {
            Language.English => item.en,
            Language.Vietnamese => item.vi,
            _ => item.en
        };

        // Nếu có tham số truyền vào, dùng string.Format
        return args != null && args.Length > 0 ? string.Format(text, args) : text;
    }
}
