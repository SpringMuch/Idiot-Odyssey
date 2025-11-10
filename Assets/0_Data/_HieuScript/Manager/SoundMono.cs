using UnityEngine;
using System.Collections;

public class SoundMono : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume = 2f;
    [SerializeField] private float lifeTime = 2f;

    private Coroutine lifeRoutine;

    void OnEnable()
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
        if (lifeRoutine != null)
            StopCoroutine(lifeRoutine);

        lifeRoutine = StartCoroutine(DisableAfter(lifeTime));
    }

    IEnumerator DisableAfter(float time)
    {
        yield return new WaitForSeconds(time);
        GameObjectSpawn.Instance.DeSapwn(gameObject);
    }

    void OnDisable()
    {
        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }
    }

    public void SetClip(AudioClip newClip)
    {
        clip = newClip;
    }
}
