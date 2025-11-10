using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public abstract class CheckAnswerBase : MonoBehaviour
{
    [Header("Hiển thị kết quả (Sprite)")]
    public Sprite correctSprite;
    public Sprite wrongSprite;

    [Tooltip("Hiện tại vị trí click hay vị trí object")]
    public bool showAtClickPosition = true;

    [Tooltip("Thời gian tồn tại của dấu V/X (giây)")]
    public float effectDuration = 0.5f;

    [Header("Tùy chọn render")]
    public int sortingOrder = 100;
    public Vector3 effectScale = Vector3.one;

    [Header("Sự kiện khi đúng/sai")]
    public UnityEvent onCorrect;
    public UnityEvent onWrong;
    [Header("Win")]
    [SerializeField] private float winDelay = 0.5f;
    protected Camera cam;

    private bool _clicked;
    private Coroutine _effectRoutine;
    private GameObject _effectGO;

    protected virtual void Awake() => cam = Camera.main;
    protected virtual void OnEnable()
    {
        _clicked = false;
        if (cam == null) cam = Camera.main;
    }

    protected virtual void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0))
            HandleClick(Input.mousePosition);
#else
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
                HandleClick(touch.position);
        }
#endif
    }

    private void HandleClick(Vector2 screenPos)
    {
        if (_clicked) return;

        // Chặn nếu đang đè lên UI
        if (EventSystem.current != null)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (EventSystem.current.IsPointerOverGameObject())
                return;
#else
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;
#endif
        }

        // Raycast 2D – lấy tất cả collider dưới con trỏ
        var ray = cam.ScreenPointToRay(screenPos);
        var hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
        if (hits == null || hits.Length == 0) return;

        // Chọn collider có sortingOrder cao nhất
        var top = GetTopmostCollider(hits);
        if (top == null) return;

        if (top.transform == transform)
        {
            _clicked = true;

            bool ok = EvaluateCondition();
            Vector3 pos = showAtClickPosition ? GetClickPosition() : transform.position;

            if (ok)
            {
                ShowEffect(correctSprite, pos);
                SoundManager.PlaySfx(SoundTypes.Correct);
                Invoke(nameof(DelayWin), winDelay);
            }
            else
            {
                ShowEffect(wrongSprite, pos);
                SoundManager.PlaySfx(SoundTypes.Wrong);
            }
        }
    }
    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }

    // Chọn collider có SpriteRenderer.sortingOrder cao nhất
    private Collider2D GetTopmostCollider(RaycastHit2D[] hits)
    {
        Collider2D best = null;
        int bestOrder = int.MinValue;

        foreach (var hit in hits)
        {
            var sr = hit.collider.GetComponent<SpriteRenderer>();
            int order = sr ? sr.sortingOrder : 0;
            if (order > bestOrder)
            {
                bestOrder = order;
                best = hit.collider;
            }
        }

        return best;
    }

    protected abstract bool EvaluateCondition();

    protected void ShowEffect(Sprite sprite, Vector3 pos)
    {
        if (sprite == null) { _clicked = false; return; }

        if (_effectRoutine != null)
        {
            if (_effectGO != null) Destroy(_effectGO);
            StopCoroutine(_effectRoutine);
        }
        _effectRoutine = StartCoroutine(SpawnAndAutoHide(sprite, pos));
    }

    private IEnumerator SpawnAndAutoHide(Sprite sprite, Vector3 pos)
    {
        _effectGO = new GameObject("[AnswerEffect]");
        _effectGO.transform.position = pos;
        _effectGO.transform.localScale = effectScale;

        var sr = _effectGO.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        // Dùng realtime để không bị ảnh hưởng bởi Time.timeScale
        yield return new WaitForSecondsRealtime(effectDuration);

        if (_effectGO != null) Destroy(_effectGO);
        _effectGO = null;
        _effectRoutine = null;
        _clicked = false;
    }

    protected Vector3 GetClickPosition()
    {
        Vector3 screen = Input.mousePosition;
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        if (z < 0.01f) z = 0.5f;
        screen.z = z;
        Vector3 world = cam.ScreenToWorldPoint(screen);
        world.z = transform.position.z;
        return world;
    }

    public void ResetClick() => _clicked = false;

    protected virtual void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
        if (_effectRoutine != null) StopCoroutine(_effectRoutine);
        if (_effectGO != null) Destroy(_effectGO);
        _effectRoutine = null;
        _effectGO = null;
        _clicked = false;
    }
}
