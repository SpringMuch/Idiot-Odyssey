using UnityEngine;

public class ShowAds : MonoBehaviour
{
    [Header("Bật/tắt từng chế độ")]
    [SerializeField] private bool byLevels = true;
    [SerializeField] private bool byTime   = false;

    [Header("Theo số màn")]
    [SerializeField, Min(1)] private int levelsInterval = 3;

    [Header("Theo thời gian (giây)")]
    [SerializeField, Min(10f)] private float secondsInterval = 90f;

    [Header("Chống spam")]
    [SerializeField, Min(5f)] private float minCooldownSeconds = 15f;

    private int   playsSinceLastAd = 0;
    private float lastAdShowTime   = 0f;

    private void Awake()
    {
        lastAdShowTime = Time.realtimeSinceStartup;
    }

    private void OnEnable()
    {
        GameEventBus.OnGameWon += OnGameWon;
    }

    private void OnDisable()
    {
        GameEventBus.OnGameWon -= OnGameWon;
    }
    private void OnGameWon()
    {
        TryCountAndShow();
    }
    public void ForceCheckNow()
    {
        TryCountAndShow();
    }

    private void TryCountAndShow()
    {
        if (IAPManager.Instance != null && IAPManager.Instance.HasActiveNoAdsSubscription())
        {
            playsSinceLastAd = 0;
            lastAdShowTime = Time.realtimeSinceStartup;
            return;
        }

        playsSinceLastAd++;

        float now = Time.realtimeSinceStartup;
        float dt  = now - lastAdShowTime;

        bool okLevel = byLevels && playsSinceLastAd >= levelsInterval;
        bool okTime  = byTime   && dt >= secondsInterval;

        if (!okLevel && !okTime) return;
        if (dt < minCooldownSeconds) return;

        if (AdsManager.Instance != null && AdsManager.Instance.CanShowInterstitial())
        {
            AdsManager.Instance.ShowInterstitial();
            playsSinceLastAd = 0;
            lastAdShowTime   = Time.realtimeSinceStartup;
        }
        else
        {
            AdsManager.Instance?.LoadInterstitialAd();
        }
    }
}
