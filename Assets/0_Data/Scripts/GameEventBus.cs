using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Win,
    Lose,
    Loading,
    LevelSelect
}

public static class GameEventBus
{
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnGameWon;
    public static event Action OnGameLost;
    public static event Action OnGamePause;
    public static event Action<int> OnRequestLoadLevel;
    public static event Action<int> OnBeforeLevelLoad;
    public static event Action<int> OnAfterLevelLoad;
    public static event Action OnOpenLevelSelect;

    public static void RaiseGameStateChanged(GameState s) => OnGameStateChanged?.Invoke(s);
    public static void RaiseGameWon() => OnGameWon?.Invoke();
    public static void RaiseGameLost() => OnGameLost?.Invoke();
    public static void RequestLoadLevel(int i) => OnRequestLoadLevel?.Invoke(i);
    public static void RaiseBeforeLevelLoad(int i) => OnBeforeLevelLoad?.Invoke(i);
    public static void RaiseAfterLevelLoad(int i) => OnAfterLevelLoad?.Invoke(i);
    public static void RaiseOpenLevelSelect() => OnOpenLevelSelect?.Invoke();
    public static void RaiseGamePause() => OnGamePause?.Invoke();
}
