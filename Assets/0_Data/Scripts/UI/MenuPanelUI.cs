using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class MenuPanelUI : MonoBehaviour
{
    private Button playButton;
    private Button settingButton;

    [Header("Tween")]
    [SerializeField] float enterDur = 0.22f;
    [SerializeField] float btnScale = 0.94f;

    void Awake()
    {
        playButton    = transform.Find("Button_Play").GetComponent<Button>();
        settingButton = transform.Find("Button_Setting").GetComponent<Button>();
    }

    void OnEnable()
    {
        // Panel pop-in
        var rt = transform as RectTransform;
        if (rt)
        {
            rt.DOKill();
            rt.localScale = Vector3.one * 0.9f;
            rt.DOScale(1f, enterDur).SetEase(Ease.OutBack);
        }
    }

    private void Start()
    {
        playButton.onClick.AddListener(() => { Bounce(playButton); OnPlayClicked(); });
        settingButton.onClick.AddListener(() => { Bounce(settingButton); OnOpenSettingSelect(); });
    }

    void Bounce(Button b)
    {
        var rt = b.transform as RectTransform;
        if (!rt) return;
        rt.DOKill();
        Sequence s = DOTween.Sequence();
        s.Append(rt.DOScale(btnScale, 0.08f));
        s.Append(rt.DOScale(1f, 0.08f));
    }

    public void OnPlayClicked()
    {
        GameManager.Instance.SetState(GameState.LevelSelect);
        SoundManager.PlaySfx(SoundTypes.Button);
    }

    public void OnOpenSettingSelect()
    {
        Debug.Log("Open Setting Panel");
    }
}
