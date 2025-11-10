using UnityEngine;

public class DespawnOnWin : MonoBehaviour
{
    private bool hasDespawned = false;

    private void OnEnable()
    {
        hasDespawned = false;
        GameEventBus.OnGameWon += HandleGameWon;
    }

    private void OnDisable()
    {
        GameEventBus.OnGameWon -= HandleGameWon;
    }

    private void HandleGameWon()
    {
        if (hasDespawned) return;
        hasDespawned = true;
        GameObjectSpawn.Instance.DeSapwn(this.gameObject);
    }
}
