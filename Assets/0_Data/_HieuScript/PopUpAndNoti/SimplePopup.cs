using System;
using UnityEngine;
using UnityEngine.UI;

#if SIMPLEPOPUP_USE_TMP
using TMPro;
#endif

[DisallowMultipleComponent]
public class SimplePopup : MonoBehaviour
{
    [Header("Buttons (kéo từ Hierarchy)")]
    [SerializeField] private Button btnAccept;
    [SerializeField] private Button btnDecline;
    [SerializeField] private Button btnPrivacy;

    [Header("Optional (để chặn click nền)")]
    [SerializeField] private Image bgBlocker; // bật Raycast Target

    [Header("Privacy Policy")]
    [SerializeField] private string privacyUrl = "https://springmuch05.github.io/Idiot-Odyssey/";

    [Header("Lưu trạng thái")]
    [SerializeField] private string consentKey = "policy_choice"; // "accept"

    [Header("Hành vi")]
    [SerializeField] private bool quitAppOnDecline = false; // Mặc định: KHÔNG quit khi Decline để tránh "crash"

    public event Action Accepted;
    public event Action Declined;

#if SIMPLEPOPUP_USE_TMP
    [Header("Optional UI Text")]
    [SerializeField] private TextMeshProUGUI bodyText;
#endif

    CanvasGroup _cg;

    public static bool HasAccepted(string key = "policy_choice")
        => PlayerPrefs.GetString(key, string.Empty) == "accept";

    public void ShowIfNeeded()
    {
        if (HasAccepted(consentKey)) gameObject.SetActive(false);
        else Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (_cg == null) _cg = gameObject.GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
        _cg.alpha = 1f;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;

        if (bgBlocker) bgBlocker.raycastTarget = true;

        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one * 0.01f;
            StopAllCoroutines();
            StartCoroutine(Pop(rt, Vector3.one, 0.18f));
        }
    }

    public void HideImmediate()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    void Awake()
    {
        if (!btnAccept)  btnAccept  = transform.Find("Accept") ?.GetComponent<Button>();
        if (!btnDecline) btnDecline = transform.Find("Decline")?.GetComponent<Button>();
        if (!btnPrivacy) btnPrivacy = transform.Find("Privacy")?.GetComponent<Button>();
        if (!bgBlocker)  bgBlocker  = transform.Find("BG")     ?.GetComponent<Image>();

        _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (btnAccept)
        {
            btnAccept.onClick.RemoveAllListeners();
            btnAccept.onClick.AddListener(OnAccept);
        }
        if (btnDecline)
        {
            btnDecline.onClick.RemoveAllListeners();
            btnDecline.onClick.AddListener(OnDecline);
        }
        if (btnPrivacy)
        {
            btnPrivacy.onClick.RemoveAllListeners();
            btnPrivacy.onClick.AddListener(OnPrivacy);
        }
    }

    // --- Button handlers ---
    void OnAccept()
    {
        PlayerPrefs.SetString(consentKey, "accept");
        PlayerPrefs.Save();
        HideImmediate();
        try { Accepted?.Invoke(); } catch { }
    }

    void OnDecline()
    {
        HideImmediate();
        try { Declined?.Invoke(); } catch { }

        if (quitAppOnDecline)
        {
#if UNITY_EDITOR
            Debug.Log("Decline -> Exit in editor");
#else
            Application.Quit();
#endif
        }
    }

    void OnPrivacy()
    {
        if (!string.IsNullOrEmpty(privacyUrl))
            Application.OpenURL(privacyUrl);
    }

    System.Collections.IEnumerator Pop(RectTransform rt, Vector3 to, float dur)
    {
        Vector3 from = rt.localScale;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float ease = Mathf.Sin(k * Mathf.PI * 0.5f);
            rt.localScale = Vector3.Lerp(from, to, ease);
            yield return null;
        }
        rt.localScale = to;
    }
}
