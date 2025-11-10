using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WatchAdButton : MonoBehaviour
{
    [SerializeField] private GameObject loadingIcon;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(OnClickWatch);
        UpdateInteractable();
        AdsManager.Instance?.PreloadRewardedAd();
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(OnClickWatch);
    }

    void Update()
    {
        UpdateInteractable();
    }

    private void UpdateInteractable()
    {
        bool ready = AdsManager.Instance != null && AdsManager.Instance.CanShowRewarded();
        button.interactable = ready;
        if (loadingIcon != null) loadingIcon.SetActive(!ready);
    }

    private void OnClickWatch()
    {
        AdsManager.Instance?.ShowRewarded(() =>
        {
            Debug.Log("🎉 Reward received!");
        });
    }
}
