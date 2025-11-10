using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;

public class ConsentManager : MonoBehaviour
{
    public static ConsentManager Instance { get; private set; }

    // --- TRẠNG THÁI ---
    public bool IsReady { get; private set; }
    public bool IsNpa  { get; private set; } // true = non-personalized ads

    private const string KeyPolicyChoice = "policy_choice"; // "accept"
    private ConsentForm consentForm;

    [Header("Links")]
    [SerializeField] private string privacyPolicyUrl = "https://springmuch05.github.io/Idiot-Odyssey/";

    [Header("Optional: Popup Privacy trong Scene (SimplePopup)")]
    [SerializeField] private SimplePopup popupPanel;

    public event Action OnConsentFlowFinished;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Khởi tạo AdMob SỚM để SDK sẵn sàng (có thể dời sau consent nếu muốn)
        MobileAds.Initialize(_ => Debug.Log("✅ Mobile Ads initialized."));

        // 1) Tạo request UMP
        var request = new ConsentRequestParameters();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Bật test mode: giả lập ở EEA để thấy form
        var debug = new ConsentDebugSettings { DebugGeography = DebugGeography.EEA };
        request.ConsentDebugSettings = debug;
#endif
        // 2) Cập nhật thông tin consent
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    private void OnConsentInfoUpdated(FormError error)
    {
        if (error != null)
        {
            Debug.LogWarning("UMP Update error: " + error.Message);
            HandleOutsideEEA(); // vẫn fallback
            return;
        }

        // 3) Nếu có form, tải & hiển thị
        if (ConsentInformation.IsConsentFormAvailable())
        {
            ConsentForm.Load((ConsentForm form, FormError loadErr) =>
            {
                if (loadErr != null)
                {
                    Debug.LogWarning("UMP Load form error: " + loadErr.Message);
                    HandleOutsideEEA();
                    return;
                }

                consentForm = form;
                ShowUMPFormIfRequired();
            });
        }
        else
        {
            HandleOutsideEEA();
        }
    }

    private void ShowUMPFormIfRequired()
    {
        // UMP sẽ tự quyết định có cần show hay không; callback gọi khi form đóng
        consentForm.Show((FormError showErr) =>
        {
            if (showErr != null)
                Debug.LogWarning("UMP Show form error: " + showErr.Message);

            ResolveConsentStatusAndFinish();
        });
    }

    private void HandleOutsideEEA()
    {
        // Người dùng ngoài EEA → UMP không hiển thị form
        // Nếu chưa từng chấp nhận privacy của bạn, hãy show popup và ĐỢI người chơi
        string choice = PlayerPrefs.GetString(KeyPolicyChoice, string.Empty);

        if (string.IsNullOrEmpty(choice))
        {
            Debug.Log("🌏 Outside EEA → using in-game privacy popup.");
            if (popupPanel != null)
            {
                // Lắng nghe kết quả từ popup rồi mới Finish
                popupPanel.Accepted -= OnPopupAccepted;   // tránh add trùng
                popupPanel.Declined -= OnPopupDeclined;
                popupPanel.Accepted += OnPopupAccepted;
                popupPanel.Declined += OnPopupDeclined;
                popupPanel.Show();
            }
            else
            {
                // Không có popup → mặc định NPA và finish
                IsNpa = true;
                Finish(true);
            }
        }
        else
        {
            // Đã từng accept privacy của bạn
            IsNpa = false; // bạn có thể đổi logic theo chính sách riêng
            Finish(true);
        }
    }

    private void OnPopupAccepted()
    {
        // Lưu accept (SimplePopup đã lưu), cho phép PA nếu muốn
        IsNpa = false;
        Finish(true);
    }

    private void OnPopupDeclined()
    {
        // Người chơi từ chối privacy riêng → an toàn nhất là NPA (hoặc thoát app nếu bạn muốn)
        IsNpa = true;
        Finish(true);
    }

    private void ResolveConsentStatusAndFinish()
    {
        var status = ConsentInformation.ConsentStatus;
        // Mapping cơ bản
        IsNpa = (status != ConsentStatus.Obtained);
        Finish(true);
    }

    private void Finish(bool ok)
    {
        if (IsReady) return; // chỉ finish 1 lần
        IsReady = ok;
        Debug.Log($"✅ Consent finished. IsNPA: {IsNpa}");
        OnConsentFlowFinished?.Invoke();
    }

    // Public API
    public void OpenPrivacyOptions()
    {
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError err) =>
        {
            ResolveConsentStatusAndFinish();
        });
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(privacyPolicyUrl);
    }

    public AdRequest BuildAdRequest()
    {
        var request = new AdRequest();
        try
        {
            var extrasProp = typeof(AdRequest).GetProperty("Extras");
            if (extrasProp != null)
            {
                var extras = extrasProp.GetValue(request) as IDictionary<string, string>;
                if (extras == null)
                {
                    extras = new Dictionary<string, string>();
                    extrasProp.SetValue(request, extras);
                }

                if (IsNpa)
                    extras["npa"] = "1";
                else if (extras.ContainsKey("npa"))
                    extras.Remove("npa");
            }
        }
        catch { }
        return request;
    }
}
