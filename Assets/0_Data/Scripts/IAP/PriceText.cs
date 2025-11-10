using UnityEngine;
using TMPro;
using System.Collections;

public class PriceText : MonoBehaviour
{
    [SerializeField] private TMP_Text priceNoAdsMonthly;
    [SerializeField] private TMP_Text priceHint50;
    [SerializeField] private TMP_Text priceHint100;
    [SerializeField] private TMP_Text priceHint500;

    IEnumerator Start()
    {
        // Chờ IAP init (controller != null)
        yield return new WaitUntil(() => IAPManager.Instance != null);
        yield return new WaitUntil(() => !string.IsNullOrEmpty(IAPManager.GetLocalizedPrice(IAPManager.PRODUCT_HINT_50))
                                      || !string.IsNullOrEmpty(IAPManager.GetLocalizedPrice(IAPManager.PRODUCT_NOADS_MONTHLY)));

        if (priceNoAdsMonthly) priceNoAdsMonthly.text = IAPManager.GetLocalizedPrice(IAPManager.PRODUCT_NOADS_MONTHLY);
        if (priceHint50)       priceHint50.text       = IAPManager.GetLocalizedPrice(IAPManager.PRODUCT_HINT_50);
        if (priceHint100)      priceHint100.text      = IAPManager.GetLocalizedPrice(IAPManager.PRODUCT_HINT_100);
        if (priceHint500)      priceHint500.text      = IAPManager.GetLocalizedPrice(IAPManager.PRODUCT_HINT_500);
    }
}
