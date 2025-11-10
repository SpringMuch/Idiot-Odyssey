using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class CloudCtrl : ObjectController
{
    [SerializeField] private GameObject rainDrop;
    [SerializeField] private float TimeDuaration = 0.5f;
    private float time;
    private Coroutine rainRoutine;

    void Update()
    {
        time -= Time.deltaTime;
    }
    protected override void OnPressed()
    {
    }

    protected override void OnTapped()
    {
        // if (rainRoutine != null)
        // {
        //     StopCoroutine(rainRoutine);
        // }

        // rainRoutine = StartCoroutine(RainDropSpawn());
        RainSpawnHandle();
    }
    private void RainSpawnHandle()
    {
        if(time < 0)
        {
            time = TimeDuaration;
            GameObject rain = GameObjectSpawn.Instance.Spawn(
            rainDrop,
            new Vector3(transform.position.x, transform.position.y - 1.3f),
            Quaternion.identity
        );
        GameObjectSpawn.Instance.SetParent(rain.transform, transform);

        rain.name = rainDrop.name;
        time = time + TimeDuaration;
        }
    }

    // private IEnumerator RainDropSpawn()
    // {
    //     GameObject rain = GameObjectSpawn.Instance.Spawn(
    //         rainDrop,
    //         new Vector3(transform.position.x, transform.position.y - 1.8f),
    //         Quaternion.identity
    //     );

    //     if (LevelManager.Instance != null &&
    //         LevelManager.Instance.Loader != null &&
    //         LevelManager.Instance.Loader.RuntimeParent != null)
    //     {
    //         GameObjectSpawn.Instance.SetParent(rain.transform, LevelManager.Instance.Loader.RuntimeParent);
    //     }

    //     rain.name = rainDrop.name;

    //     yield return new WaitForSeconds(0.8f);

    //     if (rain != null && rain.activeInHierarchy)
    //     {
    //         Debug.Log("DeSpawn");
    //         GameObjectSpawn.Instance.DeSapwn(rain);
    //     }

    //     rainRoutine = null;
    // }

    // protected override void OnDisable()
    // {
    //     if (rainRoutine != null)
    //     {
    //         StopCoroutine(rainRoutine);
    //         rainRoutine = null;
    //     }
    // }
}
