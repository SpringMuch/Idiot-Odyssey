using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SoundTypes
{
    Background,
    Button,
    Click,
    Win,
    Wrong,
    Correct,
    GlassBreak,
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    [Header("Nguồn âm thanh (KHÔNG CẦN KÉO)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clip theo enum (gán trong Inspector)")]
    [SerializeField] private SoundList[] soundList;

    [Header("BGM mặc định (tuỳ chọn)")]
    [SerializeField] private AudioClip defaultBgm;

    [Header("Volume khởi tạo (set 1 lần)")]
    [Range(0f,1f)] public float initialBgmVolume = 0.5f;
    [Range(0f,1f)] public float initialSfxVolume = 1.0f;

    // Mute flags
    public static bool BgmMuted { get; private set; } = false;
    public static bool SfxMuted { get; private set; } = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (FindObjectOfType<SoundManager>() == null)
        {
            var go = new GameObject("SoundManager(Auto)");
            go.hideFlags = HideFlags.DontSave;
            go.AddComponent<SoundManager>();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        EnsureSources();

        // Set volume 1 lần duy nhất
        bgmSource.volume = initialBgmVolume;
        sfxSource.volume = initialSfxVolume;

        // iOS tôn trọng silent switch
        #if UNITY_IOS
        AudioSettings.Mobile.stopAudioOutputOnMute = true;
        #endif

        ApplyMuteState();

        // Auto play BGM nếu có
        if (defaultBgm) PlayBackground(defaultBgm);
    }

    // Tạo hoặc cấu hình AudioSource
    private void EnsureSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            SetupSource(bgmSource, loop: true, ignorePause: true);
        }
        else
        {
            SetupSource(bgmSource, loop: true, ignorePause: true);
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            SetupSource(sfxSource, loop: false, ignorePause: false);
        }
        else
        {
            SetupSource(sfxSource, loop: false, ignorePause: false);
        }
    }

    private void SetupSource(AudioSource src, bool loop, bool ignorePause)
    {
        src.playOnAwake = false;
        src.loop = loop;
        src.spatialBlend = 0f;
        src.dopplerLevel = 0f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 1f;
        src.maxDistance = 500f;
        src.ignoreListenerPause = ignorePause;
    }

    // ===== Public API =====
    public static void PlayBackground(AudioClip clip = null)
    {
        if (!EnsureInstanceReady()) return;
        instance.EnsureSources();

        if (clip == null) clip = instance.defaultBgm;
        if (!clip) return;

        if (instance.bgmSource.clip == clip && instance.bgmSource.isPlaying) return;

        instance.bgmSource.clip = clip;
        instance.bgmSource.loop = true;

        if (!BgmMuted)
        {
            instance.bgmSource.Play();
        }
    }

    public static void StopBackground()
    {
        if (!EnsureInstanceReady()) return;
        instance.bgmSource.Stop();
    }

    // Phát SFX theo enum
    public static void PlaySfx(SoundTypes type, float volumeMul = 1f, bool randomizePitch = false, float pitchVar = 0.05f)
        => PlaySfxInternal(GetClipByType(type), volumeMul, randomizePitch, pitchVar);

    // Phát SFX bằng clip rời (ví dụ từ thắng mini-game nào đó)
    public static void PlaySfx(AudioClip clip, float volumeMul = 1f, bool randomizePitch = false, float pitchVar = 0.05f)
        => PlaySfxInternal(clip, volumeMul, randomizePitch, pitchVar);

    public static void SetBgmMuted(bool on)
    {
        if (!EnsureInstanceReady()) return;
        BgmMuted = on;
        instance.ApplyMuteState();
    }

    public static void SetSfxMuted(bool on)
    {
        if (!EnsureInstanceReady()) return;
        SfxMuted = on;
        instance.ApplyMuteState();
    }

    private static void PlaySfxInternal(AudioClip clip, float volumeMul, bool randomizePitch, float pitchVar)
    {
        if (!EnsureInstanceReady() || clip == null) return;
        if (SfxMuted) return;

        instance.EnsureSources();

        var src = instance.sfxSource;
        float vol = Mathf.Clamp01(instance.initialSfxVolume * volumeMul);

        if (randomizePitch)
        {
            float oldPitch = src.pitch;
            src.pitch = 1f + Random.Range(-pitchVar, pitchVar);
            src.PlayOneShot(clip, vol);
            src.pitch = oldPitch;
        }
        else
        {
            src.PlayOneShot(clip, vol);
        }
    }

    private static bool EnsureInstanceReady()
    {
        if (instance != null) return true;
        // Trường hợp bị gọi sớm trước AutoBootstrap
        var existing = FindObjectOfType<SoundManager>();
        if (existing != null) { instance = existing; return true; }

        var go = new GameObject("SoundManager(Auto)");
        instance = go.AddComponent<SoundManager>();
        return instance != null;
    }

    private void ApplyMuteState()
    {
        if (bgmSource)
        {
            bgmSource.mute = BgmMuted;
            if (BgmMuted && bgmSource.isPlaying) bgmSource.Pause();
            else if (!BgmMuted && bgmSource.clip && !bgmSource.isPlaying) bgmSource.UnPause();
        }
        if (sfxSource)
        {
            sfxSource.mute = SfxMuted;
        }
    }

    private static AudioClip GetClipByType(SoundTypes t)
    {
        if (!EnsureInstanceReady()) return null;
        int idx = (int)t;

        // đảm bảo mảng có size đúng theo enum
        if (instance.soundList == null || instance.soundList.Length != Enum.GetNames(typeof(SoundTypes)).Length)
            return null;

        var arr = instance.soundList[idx].Sounds;
        if (arr == null || arr.Length == 0) return null;

        return arr.Length == 1 ? arr[0] : arr[Random.Range(0, arr.Length)];
    }

    private void OnApplicationPause(bool pause)
    {
        if (bgmSource == null) return;
        if (pause) bgmSource.Pause();
        else if (!BgmMuted && bgmSource.clip != null) bgmSource.UnPause();
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // resize soundList = số phần tử đúng bằng số enum
        string[] names = Enum.GetNames(typeof(SoundTypes));
        if (soundList == null || soundList.Length != names.Length)
            Array.Resize(ref soundList, names.Length);

        for (int i = 0; i < soundList.Length; i++)
            soundList[i].name = i < names.Length ? names[i] : $"Index{i}";

        initialBgmVolume = Mathf.Clamp01(initialBgmVolume);
        initialSfxVolume = Mathf.Clamp01(initialSfxVolume);
    }
    #endif
}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
    public AudioClip[] Sounds => sounds;
}
