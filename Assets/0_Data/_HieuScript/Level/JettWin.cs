using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class JettWin : MonoBehaviour
{
    [SerializeField] private string requiredTag = "Cat";

    private bool unlocked = false;
    private bool won = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }
    public void Unlock()
    {
        unlocked = true;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (won) return;
        if (!unlocked) return;

        if (!string.IsNullOrEmpty(requiredTag))
        {
            if (!collision.CompareTag(requiredTag)) return;
        }
        transform.position = new Vector3(1.45f, 2f, 0f);
        won = true;
    }

}
