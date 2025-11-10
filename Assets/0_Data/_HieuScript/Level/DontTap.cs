using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class DontTap : MonoBehaviour, IPointerDownHandler
{
    [Header("Cấu hình thời gian")]
    [SerializeField, Min(0f)] private float countdownSeconds = 6f;
    [Header("Win")]
    [SerializeField] private float winDelay = 1f;

    private float timer;
    private bool isRunning = true;

    void OnEnable()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ResetTimer();
        }

        if (Input.GetMouseButtonDown(0))
        {
            ResetTimer();
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isRunning = false;
            Invoke(nameof(DelayWin), winDelay);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        ResetTimer();
    }

    private void ResetTimer()
    {
        timer = countdownSeconds;
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));       
    }

    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
}
