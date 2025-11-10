using UnityEngine;

[DisallowMultipleComponent]
public class Grass : MonoBehaviour
{
    private MushroomCtrl targetMushroom;
    private float timer;
    private bool Despawn;

    void Update()
    {
        if (Despawn)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (targetMushroom != null && targetMushroom.gameObject.activeInHierarchy)
                    GameObjectSpawn.Instance.DeSapwn(targetMushroom.gameObject);

                if (GameManager.Instance != null)
                {
                    SoundManager.PlaySfx(SoundTypes.Win);
                    GameManager.Instance.Win();
                    GameEventBus.RaiseGameWon();
                }

                Despawn = false;
                targetMushroom = null;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        MushroomCtrl mushroom = collision.GetComponent<MushroomCtrl>();
        if (mushroom == null) return;

        targetMushroom = mushroom;
        timer = 2f;
        Despawn = true;
    }

    void OnDisable()
    {
        Despawn = false;
        targetMushroom = null;
    }
}
