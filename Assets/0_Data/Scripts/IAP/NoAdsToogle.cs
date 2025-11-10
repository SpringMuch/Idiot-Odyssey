using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NoAdsToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private bool clickToPurchase = true;

    void Awake()
    {
        if (!toggle) toggle = GetComponent<Toggle>();
        // Ép OFF & cho phép bấm ngay từ đầu (tránh prefab để sẵn isOn=true)
        toggle.SetIsOnWithoutNotify(false);
        toggle.interactable = true;
    }

    void OnEnable()
    {
        StartCoroutine(WaitAndSyncInitial());

        IAPEvents.OnNoAdsStatusChanged += UpdateState;
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnDisable()
    {
        IAPEvents.OnNoAdsStatusChanged -= UpdateState;
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private IEnumerator WaitAndSyncInitial()
    {
        yield return new WaitUntil(() => IAPManager.Instance != null);
        UpdateState(IAPManager.Instance.HasActiveNoAdsSubscription());
    }

    private void UpdateState(bool hasNoAds)
    {
        toggle.SetIsOnWithoutNotify(hasNoAds);
        toggle.interactable = !hasNoAds; // có NoAds thì khóa, tránh mua lại
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn && clickToPurchase)
        {
            // Khoá trong lúc mua, chờ event OnNoAdsStatusChanged bật lại
            toggle.interactable = false;
            toggle.SetIsOnWithoutNotify(false);
            IAPManager.Instance?.BuyNoAdsMonthly();
        }
    }
}
