using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Mute Toggles")]
    public Toggle bgmToggle;
    public Toggle sfxToggle;

    void Start()
    {
        // Lấy trạng thái ban đầu từ SoundManager
        if (bgmToggle != null)
            bgmToggle.isOn = SoundManager.BgmMuted;

        if (sfxToggle != null)
            sfxToggle.isOn = SoundManager.SfxMuted;

        // Gán sự kiện
        if (bgmToggle != null)
            bgmToggle.onValueChanged.AddListener(on => SoundManager.SetBgmMuted(on));

        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(on => SoundManager.SetSfxMuted(on));
    }
}
