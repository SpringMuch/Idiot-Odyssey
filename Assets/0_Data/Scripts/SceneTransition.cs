using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    //[SerializeField] private CanvasGroup fadeCanvas;
    //[SerializeField] private float fadeDuration = 0.1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneSmooth(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        //yield return FadeIn();
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }
        //yield return FadeOut();
    }

    // private IEnumerator FadeIn()
    // {
    //     fadeCanvas.blocksRaycasts = true;
    //     float t = 0;
    //     while (t < fadeDuration)
    //     {
    //         t += Time.deltaTime;
    //         fadeCanvas.alpha = t / fadeDuration;
    //         yield return null;
    //     }
    // }

    // private IEnumerator FadeOut()
    // {
    //     float t = fadeDuration;
    //     while (t > 0)
    //     {
    //         t -= Time.deltaTime;
    //         fadeCanvas.alpha = t / fadeDuration;
    //         yield return null;
    //     }
    //     fadeCanvas.blocksRaycasts = false;
    // }
}
