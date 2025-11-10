using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedTextJSON : MonoBehaviour
{
    [SerializeField] private LocalizationKey key;
    private TextMeshProUGUI tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    void OnDisable()
    {
        if (LanguageManagerJSON.Instance != null)
            LanguageManagerJSON.Instance.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if (LanguageManagerJSON.Instance == null) return;
        tmp.text = LanguageManagerJSON.Instance.GetText(key);
    }
    public void SetKey(LocalizationKey key)
    {
        this.key = key;
    }
}
