using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class SwapChair : MonoBehaviour
{
    [SerializeField] private GameObject childA;
    [SerializeField] private GameObject childB;

    private bool done;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (childA == null || childB == null)
        {
            int count = transform.childCount;
            if (count >= 1 && childA == null) childA = transform.GetChild(0).gameObject;
            if (count >= 2 && childB == null) childB = transform.GetChild(1).gameObject;
        }
        if (childA != null) childA.SetActive(true);
        if (childB != null) childB.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done) return;
        if (other.GetComponent<Chair>() == null) return;

        done = true;
        if (childA != null) childA.SetActive(false);
        if (childB != null) childB.SetActive(true);
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
    }
}
