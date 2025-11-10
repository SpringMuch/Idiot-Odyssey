using System;
using static IAPEvents;
using System.Collections;
using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections.Generic;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Ad Unit IDs")]
    private string bannerId = "ca-app-pub-1945244255127558/2361559821";
    private string interId  = "ca-app-pub-1945244255127558/9815160974";
    private string rewardId = "ca-app-pub-1945244255127558/1375781213";

    [Header("Banner")]
    private AdPosition bannerPosition = AdPosition.Bottom;

    private BannerView     _bannerView;
    private InterstitialAd _interAd;
    private RewardedAd     _rewardedAd;

    private static bool _adsInitialized = false;
    private bool _waitingConsent;
    private bool isRewardedReady => _rewardedAd != null && _rewardedAd.CanShowAd();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // Nếu UMP chưa xong thì đợi
        if (ConsentManager.Instance != null && !ConsentManager.Instance.IsReady)
        {
            _waitingConsent = true;
            ConsentManager.Instance.OnConsentFlowFinished += OnConsentReadyThenInit;
        }
        OnNoAdsStatusChanged += HandleNoAdsChanged;
    }

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("✅ AdMob initialized");
            PreloadRewardedAd();
        });
        if (ConsentManager.Instance == null)
        {
            EnsureMobileAdsInitialized(() =>
            {
                LoadBannerAd();
                LoadInterstitialAd();
                LoadRewardedAd();
            });
            return;
        }

        if (ConsentManager.Instance.IsReady)
        {
            OnConsentReadyThenInit();
        }
        else
        {
            _waitingConsent = true;
            ConsentManager.Instance.OnConsentFlowFinished += OnConsentReadyThenInit;
        }
    }
    public void PreloadRewardedAd()
    {
        var adRequest = new AdRequest();

        RewardedAd.Load(rewardId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                return;
            }
            _rewardedAd = ad;
            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                PreloadRewardedAd();
            };
        });
    }

    private void OnConsentReadyThenInit()
    {
        if (ConsentManager.Instance != null)
            ConsentManager.Instance.OnConsentFlowFinished -= OnConsentReadyThenInit;

        _waitingConsent = false;

        EnsureMobileAdsInitialized(() =>
        {
            // Sau khi SDK sẵn sàng, load tất cả các loại ad
            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    private void EnsureMobileAdsInitialized(Action onReady)
    {
        if (!_adsInitialized)
        {
            _adsInitialized = true;
            try
            {
                GoogleMobileAds.Api.MobileAds.Initialize(initStatus =>
                {
                    onReady?.Invoke();
                });
                return;
            }
            catch { Debug.Log("Assume MobileAds already initialized."); }
        }
        onReady?.Invoke();
    }

    // === BUILD REQUEST (luôn thông qua ConsentManager) ===
   private AdRequest BuildRequest()
    {
        if (ConsentManager.Instance != null)
            return ConsentManager.Instance.BuildAdRequest();

        // Fallback an toàn: tạo request thường (không personalize nếu có thể)
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
                extras["npa"] = "1";
            }
        }
        catch { /* tương thích nhiều version */ }

        return request;
    }

    // === BANNER ===
    public void LoadBannerAd()
    {
        if (IAPManager.Instance != null && IAPManager.Instance.HasActiveNoAdsSubscription())
        {
            Debug.Log("NoAds active → skip banner");
            return;
        }
        DestroyBanner();

        _bannerView = new BannerView(bannerId, AdSize.Banner, bannerPosition);

        _bannerView.OnBannerAdLoaded      += () => Debug.Log("✅ Banner loaded");
        _bannerView.OnBannerAdLoadFailed  += (LoadAdError e) => {
            // Debug.LogWarning("❌ Banner failed: " + e.GetMessage());
            StartCoroutine(Retry(LoadBannerAd, 10f));
        };
        _bannerView.OnAdPaid              += (AdValue adValue) =>
        {
            // TODO: optional - gửi revenue
        };

        _bannerView.LoadAd(BuildRequest());
    }

    public void ShowBanner()  { _bannerView?.Show();  }
    public void HideBanner()  { _bannerView?.Hide();  }
    public void DestroyBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    // === INTERSTITIAL ===
    public void LoadInterstitialAd()
    {
        if (IAPManager.Instance != null && IAPManager.Instance.HasActiveNoAdsSubscription())
        {
            Debug.Log("NoAds active → skip interstitial");
            return;
        }
        DestroyInterstitial();

        InterstitialAd.Load(interId, BuildRequest(), (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                // Debug.LogWarning("❌ Interstitial load fail: " + error?.GetMessage());
                StartCoroutine(Retry(LoadInterstitialAd, 10f));
                return;
            }

            _interAd = ad;
            _interAd.OnAdFullScreenContentClosed += () =>
            {
                // Debug.Log("ℹ️ Interstitial closed -> preload next");
                LoadInterstitialAd();
                ProgressManager.Instance?.AddHint(5);
            };
            _interAd.OnAdFullScreenContentFailed += (AdError e) =>
            {
                // Debug.LogWarning("❌ Interstitial show fail: " + e.GetMessage());
                LoadInterstitialAd();
            };
        });
    }

    public bool CanShowInterstitial() => _interAd != null && _interAd.CanShowAd();

    public void ShowInterstitial()
    {
        if (CanShowInterstitial())
        {
            _interAd.Show();
        }
        else
        {
            // Debug.Log("ℹ️ Interstitial not ready, reloading...");
            LoadInterstitialAd();
        }
    }

    private void DestroyInterstitial()
    {
        if (_interAd != null)
        {
            _interAd.Destroy();
            _interAd = null;
        }
    }

    // === REWARDED ===
    public void LoadRewardedAd()
    {
        DestroyRewarded();

        RewardedAd.Load(rewardId, BuildRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                // Debug.LogWarning("❌ Rewarded load fail: " + error?.GetMessage());
                StartCoroutine(Retry(LoadRewardedAd, 10f));
                return;
            }

            _rewardedAd = ad;
            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                // Debug.Log("ℹ️ Rewarded closed -> preload next");
                LoadRewardedAd();
            };
            _rewardedAd.OnAdFullScreenContentFailed += (AdError e) =>
            {
                // Debug.LogWarning("❌ Rewarded show fail: " + e.GetMessage());
                LoadRewardedAd();
            };
        });
    }

    public bool CanShowRewarded()
    {
        return isRewardedReady;
    }

    public void ShowRewarded(Action onRewarded = null)
    {
        if (!isRewardedReady)
        {
            PreloadRewardedAd();
            return;
        }

        _rewardedAd.Show(reward =>
        {
            onRewarded?.Invoke();
        });
    }

    private void DestroyRewarded()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
    }

    // === UTIL ===
    private IEnumerator Retry(Action action, float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        action?.Invoke();
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            // Quay lại game, refresh banner để chắc chắn hiển thị đúng
            if (_bannerView != null)
            {
                _bannerView.Hide();
                _bannerView.Show();
            }
        }
    }

    void OnDisable()
    {
        if (_waitingConsent && ConsentManager.Instance != null)
            ConsentManager.Instance.OnConsentFlowFinished -= OnConsentReadyThenInit;
        OnNoAdsStatusChanged -= HandleNoAdsChanged;
    }

    private void HandleNoAdsChanged(bool active)
    {
        if (active)
        {
            Debug.Log("NoAds active → Tắt quảng cáo.");
            DestroyBanner();
            DestroyInterstitial();
        }
        else
        {
            Debug.Log("NoAds hết hạn → Load quảng cáo lại.");
            LoadBannerAd();
            LoadInterstitialAd();
        }
    }

    void OnDestroy()
    {
        DestroyBanner();
        DestroyInterstitial();
        DestroyRewarded();
    }
}
