using System.Collections;
using UnityEngine;

// Level 7
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class ManyTapToWin : MonoBehaviour
{
    [Header("Cấu hình số lần tap cần thiết")]
    [SerializeField, Min(1)] private int requiredTaps = 7;

    [Header("Win")]
    [SerializeField] private float winDelay = 1f;

    [Header("Hiệu ứng mờ dần")]
    [SerializeField, Range(0.05f, 1f)] private float fadeStep = 0.1f;

    private int tapCount;
    private bool completed;
    private SpriteRenderer sr;
    private Color originalColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    void OnEnable()
    {
        tapCount = 0;
        completed = false;
        if (sr != null)
            sr.color = originalColor;
    }

    void Update()
    {
        if (completed) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CheckTap(pos);
        }
#else
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            CheckTap(pos);
        }
#endif
    }

    private void CheckTap(Vector2 pos)
    {
        Collider2D col = Physics2D.OverlapPoint(pos);
        if (col == null || col.gameObject != gameObject) return;

        tapCount++;

        // Giảm độ trong suốt dần mỗi lần tap
        if (sr != null)
        {
            float alpha = Mathf.Clamp01(sr.color.a - fadeStep);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        }

        // Nếu đã đủ số lần tap
        if (tapCount >= requiredTaps)
        {
            completed = true;

            GameObjectSpawn.Instance.DeSapwn(this.gameObject);
            Invoke(nameof(DelayWin), winDelay);
        }
        else
        {
            SoundManager.PlaySfx(SoundTypes.Click);
        }
    }

    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
        // Reset lại độ trong suốt khi object được tái sử dụng từ pool
        if (sr != null)
            sr.color = originalColor;
    }
}
