
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-100)]
public class UIManager : MonoBehaviour
{

    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject movingCanvas;
    [SerializeField] private GameObject settingPanel;

    //get
    public GameObject MovingCanva => movingCanvas;
    public GameObject SettingPanel => settingPanel;

    private void Awake()
    {
        Instance = this;
        HideAllPanels();
        mainMenuPanel.SetActive(true);
        settingPanel.SetActive(false);
        EnableGameplayInputs();
    }

    private void OnEnable()
    {
        GameEventBus.OnGameStateChanged += HandleGameStateChanged;
        GameEventBus.OnGameWon += ShowWinPanel;
        // GameEventBus.OnGameLost += ShowLosePanel;
        GameEventBus.OnOpenLevelSelect += ShowLevelSelect;
    }

    private void OnDisable()
    {
        GameEventBus.OnGameStateChanged -= HandleGameStateChanged;
        GameEventBus.OnGameWon -= ShowWinPanel;
        // GameEventBus.OnGameLost -= ShowLosePanel;
        GameEventBus.OnOpenLevelSelect -= ShowLevelSelect;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        HideAllPanels();

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                mainMenuPanel?.SetActive(true);
                movingCanvas.SetActive(false);
                EnableGameplayInputs(false);
                break;
            case GameState.LevelSelect:
                Time.timeScale = 1f;
                levelSelectPanel?.SetActive(true);
                movingCanvas.SetActive(true);
                settingPanel.SetActive(false);
                LevelManager.Instance.ClearCurrentLevel();
                EnableGameplayInputs(false);
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                gameplayPanel?.SetActive(true);
                movingCanvas.SetActive(true);
                settingPanel.SetActive(false);
                EnableGameplayInputs(true);
                break;
            case GameState.Paused:
                gameplayPanel?.SetActive(true);
                movingCanvas.SetActive(true);
                EnableGameplayInputs(false);
                break;
            case GameState.Win:
                Time.timeScale = 1f;
                EventSystem.current.SetSelectedGameObject(null);
                winPanel?.SetActive(true);
                // DisableAllUIExcept(winPanel);
                EnableGameplayInputs(false);
                break;
            case GameState.Lose:
                break;
        }
    }
    private void DisableAllUIExcept(GameObject target)
    {
        CanvasGroup[] groups = FindObjectsOfType<CanvasGroup>();

        foreach (var g in groups)
        {
            if (g.gameObject == target)
            {
                g.interactable = true;
                g.blocksRaycasts = true;
            }
            else
            {
                g.interactable = false;
                g.blocksRaycasts = false;
            }
        }
    }
    private void EnableGameplayInputs(bool enable = true)
    {
        var controllers = FindObjectsOfType<ObjectController>(true);
        foreach (var ctrl in controllers)
        {
            ctrl.enabled = enable;
        }
        var mobile = FindObjectOfType<MobileInputManager>(true);
        if (mobile) mobile.enabled = enable;
    }

    private void HideAllPanels()
    {
        mainMenuPanel?.SetActive(false);
        levelSelectPanel?.SetActive(false);
        gameplayPanel?.SetActive(false);
        //pausePanel?.SetActive(false);
        winPanel?.SetActive(false);
    }


    //Level Select UI
    private void ShowLevelSelect()
    {
        HideAllPanels();
        GameManager.Instance.SetState(GameState.LevelSelect);
        // levelSelectPanel?.SetActive(true);
        //StartCoroutine(ShowLevelSelectPanel());

    }

    private IEnumerator ShowLevelSelectPanel()
    {
        yield return new WaitForSeconds(0.01f);
        levelSelectPanel?.SetActive(true);
    }

    //Win-Lose UI

    public void ShowWinPanel() => winPanel?.SetActive(true);
}