using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class RainDrop : MonoBehaviour
{
    [SerializeField] private GameObject mushroom;
    private float timer;
    private Coroutine spawnRoutine;

    void Update()
    {
        timer -= Time.deltaTime;
        RainDropHandleDeSpawn();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        Grass grass = collision.GetComponent<Grass>();
        if (grass == null) return;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
        spawnRoutine = StartCoroutine(MushroomSpawn());
    }
    
    private void RainDropHandleDeSpawn()
    {
        if (timer < 0)
        {
            GameObjectSpawn.Instance.DeSapwn(this.gameObject);
            timer = 1f; 
        }
    }
    private IEnumerator MushroomSpawn()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject mushroomCtrl = GameObjectSpawn.Instance.Spawn(
            mushroom,
            new Vector3(transform.position.x, transform.position.y - 0.5f),
            Quaternion.identity
        );

        if (LevelManager.Instance != null &&
            LevelManager.Instance.Loader != null &&
            LevelManager.Instance.Loader.RuntimeParent != null)
        {
            GameObjectSpawn.Instance.SetParent(mushroomCtrl.transform, LevelManager.Instance.Loader.RuntimeParent);
        }

        mushroomCtrl.name = mushroom.name;
        spawnRoutine = null;
    }

    void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }
}
