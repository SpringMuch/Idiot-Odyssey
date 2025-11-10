using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

#pragma warning disable 0618

[DisallowMultipleComponent]
public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }
    private static IStoreController controller;
    private static IExtensionProvider extensions;

    // IDs
    public const string PRODUCT_NOADS_MONTHLY = "no_ads_monthly";
    public const string PRODUCT_HINT_50  = "hint_50";
    public const string PRODUCT_HINT_100 = "hint_100";
    public const string PRODUCT_HINT_500 = "hint_500";

    [Header("DEV")]
    [Tooltip("Bật để giả lập đã có No Ads active khi chưa có Play Console.")]
    [SerializeField] private bool DEV_FAKE_SUBS_ACTIVE = false;
    [SerializeField] private bool DEV_MARK_SUB_ACTIVE_ON_FAKE_PURCHASE = true;
    private bool _hasNoAdsActive;
    public bool HasActiveNoAdsSubscription() => _hasNoAdsActive || DEV_FAKE_SUBS_ACTIVE;

    private DateTime _nextFreeHintUtc;

    private bool _purchasing;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        var module  = StandardPurchasingModule.Instance(AppStore.GooglePlay);
        var builder = ConfigurationBuilder.Instance(module);

        builder.AddProduct(PRODUCT_NOADS_MONTHLY, ProductType.Subscription);
        builder.AddProduct(PRODUCT_HINT_50,  ProductType.Consumable);
        builder.AddProduct(PRODUCT_HINT_100, ProductType.Consumable);
        builder.AddProduct(PRODUCT_HINT_500, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);

        EnsureDailyHintState();
    }

    // ===== Public API cho UI =====
    public void BuyNoAdsMonthly() => TryBuy(PRODUCT_NOADS_MONTHLY);
    public void BuyHint50()       => TryBuy(PRODUCT_HINT_50);
    public void BuyHint100()      => TryBuy(PRODUCT_HINT_100);
    public void BuyHint500()      => TryBuy(PRODUCT_HINT_500);

    public void TryClaimDailyHint()
    {
        var remain = _nextFreeHintUtc - DateTime.UtcNow;
        if (remain.TotalSeconds <= 0)
        {
            ProgressManager.Instance?.AddHint(30);
            _nextFreeHintUtc = DateTime.UtcNow.AddHours(24);
            PlayerPrefs.SetString("NEXT_HINT_TIME", _nextFreeHintUtc.ToString("o"));
            IAPEvents.OnSuccess?.Invoke("Take 30 Hints free!");
        }
        else
        {
            IAPEvents.OnInfo?.Invoke($"Come Back after {remain.Hours}h {remain.Minutes}m.");
        }
    }

    // ===== IStoreListener =====
    public void OnInitialized(IStoreController c, IExtensionProvider e)
    {
        controller = c;
        extensions = e;
        RefreshNoAdsEntitlement(); // đọc receipt để biết sub còn hạn không
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        IAPEvents.OnError?.Invoke("IAP init failed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        IAPEvents.OnError?.Invoke($"IAP init failed: {error} - {message}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        _purchasing = false;
        IAPEvents.OnError?.Invoke($"Payment Failed: {reason}");
        IAPEvents.OnNoAdsStatusChanged?.Invoke(HasActiveNoAdsSubscription());
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        _purchasing = false;
        string id = e.purchasedProduct.definition.id;

        if (id == PRODUCT_HINT_50)  { GrantHints(50);  return PurchaseProcessingResult.Complete; }
        if (id == PRODUCT_HINT_100) { GrantHints(100); return PurchaseProcessingResult.Complete; }
        if (id == PRODUCT_HINT_500) { GrantHints(500); return PurchaseProcessingResult.Complete; }

        if (id == PRODUCT_NOADS_MONTHLY)
        {
        #if UNITY_EDITOR
            // Test bằng FakeStore: kích hoạt ngay để UI/Ads cập nhật đúng
            if (DEV_MARK_SUB_ACTIVE_ON_FAKE_PURCHASE)
            {
                NotifyNoAdsStatus(true);
                IAPEvents.OnSuccess?.Invoke("Activated No Ads (FakeStore test).");
                return PurchaseProcessingResult.Complete;
            }
        #endif

            // Trường hợp live: đọc entitlement từ receipt thật
            RefreshNoAdsEntitlement();
            if (HasActiveNoAdsSubscription())
                IAPEvents.OnSuccess?.Invoke("Subscribed to No Ads (monthly renewal).");
            else
                IAPEvents.OnInfo?.Invoke("Processed payment. Benefits will be synchronized soon.");
            return PurchaseProcessingResult.Complete;
        }

        IAPEvents.OnError?.Invoke("Unknown product.");
        return PurchaseProcessingResult.Complete;
    }

    // ===== Helpers =====
    private void TryBuy(string productId)
    {
        if (_purchasing) return;

        if (productId == PRODUCT_NOADS_MONTHLY && HasActiveNoAdsSubscription())
        {
            IAPEvents.OnInfo?.Invoke("You have purchased No Ads");
            return;
        }

        if (controller == null)
        {
            IAPEvents.OnInfo?.Invoke("IAP is not ready. Please try again.");
            return;
        }

        var p = controller.products.WithID(productId);
        if (p == null || !p.availableToPurchase)
        {
            IAPEvents.OnInfo?.Invoke("Product is not available.");
            return;
        }

        string desc = GetClearDescription(productId, p.metadata.localizedPriceString, p.definition.type);
        if (IAPEvents.OnConfirm != null)
        {
            IAPEvents.OnConfirm.Invoke(desc, (ok) =>
            {
                if (!ok) return;
                _purchasing = true;
                controller.InitiatePurchase(p);
            });
        }
        else
        {
            _purchasing = true;
            controller.InitiatePurchase(p);
        }
    }

    private string GetClearDescription(string productId, string localizedPrice, ProductType type)
    {
        if (productId == PRODUCT_NOADS_MONTHLY)
        {
            return
$@"No Ads Bundle – {localizedPrice}
• Type: Monthly Subscription
• Payment Cycle: Monthly, auto-renewed by Google Play
• Benefits: Remove Banner & Interstitial Ads (Rewarded still watch to receive rewards)
• NO subscription required to use the app
• Can be canceled in Google Play anytime before the next period.";
        }

        string baseLine = productId switch
        {
            PRODUCT_HINT_50  => $"50 Hint Package – {localizedPrice}",
            PRODUCT_HINT_100 => $"100 Hint Package – {localizedPrice}",
            PRODUCT_HINT_500 => $"500 Hint Package – {localizedPrice}",
            _ => $"Product – {localizedPrice}"
        };
        return
$@"{baseLine}
• Type: Consumable
• Payment: One-time, NOT renewable
• Give Hints to use in-game
• NO subscription required to use the app.";
}

    private void GrantHints(int amount)
    {
        ProgressManager.Instance?.AddHint(amount);
        IAPEvents.OnSuccess?.Invoke($"+{amount} Hint added.");
    }

    private void EnsureDailyHintState()
    {
        if (!PlayerPrefs.HasKey("NEXT_HINT_TIME"))
            PlayerPrefs.SetString("NEXT_HINT_TIME", DateTime.UtcNow.ToString("o"));
        _nextFreeHintUtc = DateTime.Parse(PlayerPrefs.GetString("NEXT_HINT_TIME"));
    }

    // ===== Sub entitlement =====
    public void RefreshNoAdsEntitlement()
    {
        // 1) Dev flag luôn bật (mô phỏng đã có NoAds)
        if (DEV_FAKE_SUBS_ACTIVE)
        {
            NotifyNoAdsStatus(true);
            return;
        }

    #if UNITY_EDITOR
        // 2) FakeStore: không có receipt hợp lệ -> tránh reset về false
        if (DEV_MARK_SUB_ACTIVE_ON_FAKE_PURCHASE)
        {
            return;
        }
    #endif

        if (controller == null) { NotifyNoAdsStatus(false); return; }

        var p = controller.products.WithID(PRODUCT_NOADS_MONTHLY);
        if (p == null || string.IsNullOrEmpty(p.receipt))
        {
            NotifyNoAdsStatus(false);
            return;
        }

        try
        {
            var subMgr = new SubscriptionManager(p, null);
            var info = subMgr.getSubscriptionInfo();
            bool isSub = info.isSubscribed() == Result.True;
            bool isExpired = info.isExpired() == Result.True;
            NotifyNoAdsStatus(isSub && !isExpired);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Subscription check failed: " + ex.Message);
            NotifyNoAdsStatus(false);
        }
    }
    private void NotifyNoAdsStatus(bool active)
    {
        _hasNoAdsActive = active;
        IAPEvents.OnNoAdsStatusChanged?.Invoke(active);
        Debug.Log($"[IAP] NoAds status changed: {(active ? "ACTIVE" : "INACTIVE")}");
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) RefreshNoAdsEntitlement();
    }
    public static string GetLocalizedPrice(string productId)
    {
        var p = controller?.products?.WithID(productId);
        return (p != null && p.availableToPurchase) ? p.metadata.localizedPriceString : "";
    }
}
#pragma warning restore 0618
