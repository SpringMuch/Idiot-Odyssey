using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class LevelSelectItemUI : MonoBehaviour
{
    [Header("Components (tự gán nếu trống)")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    [Header("Sprites trạng thái")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite completedSprite;

    [Header("Màu (tuỳ chọn)")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color completedColor = Color.green;

    [Header("Hiệu ứng DOTween")]
    [SerializeField] private float spawnDelayPerIndex = 0.015f;
    [SerializeField] private float popDuration = 0.22f;
    [SerializeField] private float clickScale = 0.92f;
    [SerializeField] private float clickDuration = 0.07f;

    private int levelIndex;
    private RectTransform rectTransform;
    private Vector3 initScale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initScale = rectTransform.localScale;

        if (button == null) button = GetComponent<Button>();
        if (image == null) image = GetComponent<Image>();
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>();

        button.onClick.AddListener(OnClick);
    }

    public void UpdateData(LevelData data, Vector2 anchoredPosition)
    {
        levelIndex = data.levelIndex;

        if (label)
            label.text = "Level " + data.levelIndex;

        // --- Gán sprite theo trạng thái ---
        if (image)
        {
            if (data.isCompleted && completedSprite != null)
            {
                image.sprite = completedSprite;
                image.color = completedColor;
            }
            else if (!data.isUnlocked && lockedSprite != null)
            {
                image.sprite = lockedSprite;
                image.color = lockedColor;
            }
            else
            {
                image.sprite = normalSprite;
                image.color = normalColor;
            }
        }

        // --- Khóa nút nếu chưa mở ---
        button.interactable = data.isUnlocked;

        // --- Vị trí + hiệu ứng bật lên ---
        rectTransform.anchoredPosition = anchoredPosition;
        gameObject.SetActive(true);

        rectTransform.DOKill();
        rectTransform.localScale = initScale * 0.8f;
        rectTransform
            .DOScale(initScale, popDuration)
            .SetDelay(data.levelIndex * spawnDelayPerIndex)
            .SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnClick()
    {
        rectTransform.DOKill();
        Sequence s = DOTween.Sequence();
        s.Append(rectTransform.DOScale(initScale * clickScale, clickDuration));
        s.Append(rectTransform.DOScale(initScale, clickDuration));
        s.OnComplete(() =>
        {
            SoundManager.PlaySfx(SoundTypes.Button);
            ProgressManager.Instance.SetCurrentLevel(levelIndex);
            GameEventBus.RequestLoadLevel(levelIndex);
        });
    }
}
