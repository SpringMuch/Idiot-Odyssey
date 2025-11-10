using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : ObjectController
{
    [SerializeField] private bool isNeed = false;
    [SerializeField] private GameObject spawnObject;
    [SerializeField] private AudioClip CrashSound;

    public void GetChair()
    {
        gameObject.SetActive(true);
    }

    protected override void OnTapped() { }
    protected override void OnPressed() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        SwapChair swapchair = collision.GetComponent<SwapChair>();
        if (swapchair == null) return;

        if (isNeed)
        {
            SpawnObjectAtChair();
        }
        GameObjectSpawn.Instance.DeSapwn(this.gameObject);
    }

    private void SpawnObjectAtChair()
    {
        if (spawnObject == null)
        {
            return;
        }

        GameObject spawned = GameObjectSpawn.Instance.Spawn(
            spawnObject,
            transform.position,
            Quaternion.identity
        );
        
        if (LevelManager.Instance != null &&
            LevelManager.Instance.Loader != null &&
            LevelManager.Instance.Loader.RuntimeParent != null)
        {
            GameObjectSpawn.Instance.SetParent(
                spawned.transform,
                LevelManager.Instance.Loader.RuntimeParent
            );
        }
        spawned.name = spawnObject.name;
        if (CrashSound != null)
        {
            GameObject soundObj = new GameObject("CrashSound");
            var sm = soundObj.AddComponent<SoundMono>();
            sm.SetClip(CrashSound);
            soundObj.transform.position = transform.position;
        }
    }
}
