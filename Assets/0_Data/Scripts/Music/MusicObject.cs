using UnityEngine;

public class MusicObject : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private AudioSource source;

    void OnEnable()
    {
        source = FindObjectOfType<AudioSource>();
        if (clip != null)
            source.clip = clip;
        source.loop = true;
        source.volume = volume;
        source.Play();
    }

    void OnDisable()
    {
        if (source != null && source.isPlaying)
            source.Stop();
    }
}
