using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.MainMenu;
    public GameState GetState() => State;
    private int currentIndex;
    public int CurrentIndex => currentIndex;
    [SerializeField] private SimplePopup consentPopup;

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
            return;
        }
        
    }
    private void Start()
    {
        GameEventBus.RaiseGameStateChanged(State);
        consentPopup.ShowIfNeeded();

        GameEventBus.OnRequestLoadLevel += HandleRequestLoadLevel;
        GameEventBus.OnGameWon += () => SetState(GameState.Win);
        GameEventBus.OnGameLost += () => SetState(GameState.Lose);
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            GameEventBus.OnRequestLoadLevel -= HandleRequestLoadLevel;
            GameEventBus.OnGameWon -= () => SetState(GameState.Win);
            GameEventBus.OnGameLost -= () => SetState(GameState.Lose);
        }
    }

    

    public void StartGame(int levelIndex)
    {
        SetState(GameState.Loading);
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        //Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    public void Win(bool autoAdvance = false)
    {
        SetState(GameState.Win);
        //Time.timeScale = 0f;

        if (LevelManager.Instance == null || ProgressManager.Instance == null) return;

        ProgressManager.Instance.CompleteLevel(currentIndex);
        GameEventBus.RaiseGameWon();
    }

    public void Lose()
    {
        SetState(GameState.Lose);
        GameEventBus.RaiseGameLost();
        LevelManager.Instance.RetryLevel();
    }

    private void HandleRequestLoadLevel(int index)
    {
        if (State == GameState.Loading) return;
        StartCoroutine(LoadLevelRoutine(index));
    }

    private IEnumerator LoadLevelRoutine(int index)
    {
        SetState(GameState.Loading);
        GameEventBus.RaiseBeforeLevelLoad(index);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadSpecificLevel(index);
        }
        else
        {
            Debug.LogWarning("[GameManager] LevelManager not found.");
        }

        yield return null;
        yield return new WaitForSecondsRealtime(0.05f);

        SetState(GameState.Playing);
        GameEventBus.RaiseAfterLevelLoad(index);
    }
    public void PlayState()
    {
        SetState(GameState.Playing);
        Time.timeScale = 1f;
    }

    public void SetState(GameState newState)
    {
        if (State == newState) return;
        State = newState;
        GameEventBus.RaiseGameStateChanged(newState);
    }
}