using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject loadingPanel;

    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Settings")]
    [SerializeField] private float loadDuration = 2f; // thời gian giả lập load (2 giây)

    private bool isLoadingDone = false;

    public void LoadingGame()
    {
        loadingPanel.SetActive(true);
        menuPanel.SetActive(false);

        StartCoroutine(SimulateLoading());
    }

    private IEnumerator SimulateLoading()
    {
        float timer = 0f;
        isLoadingDone = false;

        while (timer < loadDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / loadDuration);

            if (loadingBar != null) loadingBar.value = progress;

            if (loadingText != null) loadingText.text = $"{Mathf.RoundToInt(progress * 100)}%";

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        ShowMenuPanel();
    }

    private void ShowMenuPanel()
    {
        if (isLoadingDone) return;

        isLoadingDone = true;

        loadingPanel.SetActive(false);
        menuPanel.SetActive(true);

        StopAllCoroutines();

        Debug.Log("✅ Loading complete — MenuPanel activated");
    }

    #region Test
    //private void Loading()
    //{
    //    loadingPanel.SetActive(true);
    //    menuPanel.SetActive(false);

    //    StartCoroutine(LoadLevelAsync());
    //}

    //private IEnumerator LoadLevelAsync()
    //{

    //    AsyncOperation loadOperation = SceneManager.LoadSceneAsync(Const.MENUSCREEN);
    //    loadOperation.allowSceneActivation = false;

    //    while (!loadOperation.isDone)
    //    {
    //        int value = (int)Mathf.Clamp01(loadOperation.progress / 0.9f);

    //        loadingBar.value = value;
    //        loadingText.text = value*100f + "%";

    //        if (loadOperation.progress >= 0.9f && !isLoadingDone)
    //        {
    //            isLoadingDone = true;

    //            // Đợi một chút, rồi bật menu
    //            yield return new WaitForSeconds(0.3f);

    //            ShowMenu();

    //            loadOperation.allowSceneActivation = true;
    //        }

    //        yield return null;
    //    }
    //    yield return null;
    //}

    //private void ShowMenu()
    //{
    //    loadingPanel.SetActive(false);
    //    menuPanel.SetActive(true);
    //    StopAllCoroutines();
    //}
    #endregion


}
