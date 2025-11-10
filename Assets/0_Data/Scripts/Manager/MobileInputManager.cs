using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

// [DefaultExecutionOrder(-1111)]
public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance;

    public event Action<Vector2> OnTap;
    public event Action<Vector2> OnPress;
    public event Action<Vector2, Vector2> OnDrag;
    public event Action<float> OnPinch;
    public event Action<float> OnRotate;
    public event Action<string> OnObjectSelected;
    public event Action OnObjectDeselected;

    private enum InputState { Idle, PotentialTap, Pressed, Dragging, Pinch, Rotate }
    private InputState currentState = InputState.Idle;

    [Header("Cấu hình")]
    [SerializeField] private float tapThreshold = 0.3f;
    [SerializeField] private float dragThreshold = 5f;

    private Camera cam;
    private Vector2 startPos;
    private float startTime;
    private Coroutine pressCoroutine;
    private bool pressTriggered;
    private bool isDragging;

    private float prevDistance;
    private float prevAngle;

    private string currentObjectID;

    private const int MaxHits = 24;
    private static readonly Collider2D[] s_hitBuf = new Collider2D[MaxHits];

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            if (currentState != InputState.Idle)
                currentState = InputState.Idle;
        }
    }
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

    #if UNITY_EDITOR || UNITY_STANDALONE
        // Chuột
        return EventSystem.current.IsPointerOverGameObject();
    #else
        // Touch (lấy ngón đầu tiên)
        if (Input.touchCount == 0) return false;
        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    #endif
    }

    // ========== TOUCH ==========
    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI())
                    return;

                BeginInput(t.position);
            }
            else if (t.phase == TouchPhase.Moved)
            {
                ContinueMove(t.position);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                EndInput(t.position);
            }
        }
        else if (Input.touchCount == 2)
        {
            CancelPendingPress();
            HandleMultiTouch(Input.GetTouch(0), Input.GetTouch(1));
        }
    }

    // ========== MOUSE ==========
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
                return;

            BeginInput(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueMove(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndInput(Input.mousePosition);
        }

        HandleMouseScroll();
    }

    private void HandleMouseScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                OnPinch?.Invoke(scroll * 10f);
            else
                OnRotate?.Invoke(-scroll * 10f);
        }
    }

    // ========== MULTI TOUCH ==========
    private void HandleMultiTouch(Touch t0, Touch t1)
    {
        if (currentState != InputState.Pinch && currentState != InputState.Rotate)
            currentState = InputState.Pinch;

        Vector2 p0 = t0.position;
        Vector2 p1 = t1.position;
        float curDistance = Vector2.Distance(p0, p1);
        float curAngle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;

        if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
        {
            prevDistance = curDistance;
            prevAngle = curAngle;
            return;
        }

        HandlePinch(curDistance);
        HandleRotate(curAngle);

        prevDistance = curDistance;
        prevAngle = curAngle;
    }

    // ========== BEGIN / CONTINUE / END ==========
    private void BeginInput(Vector2 screenPos)
    {
        startPos = screenPos;
        startTime = Time.time;
        currentState = InputState.PotentialTap;
        pressTriggered = false;
        isDragging = false;

        GameObject selected = GetObjectAtPosition(screenPos);
        if (selected)
        {
            var ctrl = selected.GetComponent<ObjectController>();
            currentObjectID = ctrl ? ctrl.GetObjectID : selected.name;
            OnObjectSelected?.Invoke(currentObjectID);
        }
        else
        {
            currentObjectID = null;
            OnObjectDeselected?.Invoke();
        }

        pressCoroutine = StartCoroutine(PressTimer(screenPos));
    }

    private void ContinueMove(Vector2 screenPos)
    {
        if (currentState == InputState.PotentialTap || currentState == InputState.Pressed)
        {
            if (Vector2.Distance(screenPos, startPos) > dragThreshold)
            {
                CancelPendingPress();
                currentState = InputState.Dragging;
                isDragging = true;
            }
        }

        if (currentState == InputState.Dragging)
        {
            // Không di chuyển Transform ở đây nữa!
            OnDrag?.Invoke(startPos, screenPos);
        }
    }

    private void EndInput(Vector2 screenPos)
    {
        if (currentState == InputState.Dragging)
        {
            CancelPendingPress();
            currentState = InputState.Idle;
            return;
        }

        if (pressTriggered)
        {
            currentState = InputState.Idle;
            return;
        }

        float held = Time.time - startTime;
        CancelPendingPress();

        if (!isDragging && held <= tapThreshold)
            OnTap?.Invoke(screenPos);

        currentState = InputState.Idle;
    }

    // ========== HELPERS ==========
    private IEnumerator PressTimer(Vector2 initialPos)
    {
        yield return new WaitForSeconds(tapThreshold);
        if (currentState == InputState.PotentialTap && !isDragging && Input.touchCount <= 1)
        {
            pressTriggered = true;
            currentState = InputState.Pressed;
            OnPress?.Invoke(initialPos);
        }
    }

    private void CancelPendingPress()
    {
        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }
        pressTriggered = false;
    }

    private void HandlePinch(float curDistance)
    {
        float delta = curDistance - prevDistance;
        OnPinch?.Invoke(delta * 0.01f);
    }

    private void HandleRotate(float curAngle)
    {
        float deltaAngle = Mathf.DeltaAngle(prevAngle, curAngle);
        if (Mathf.Abs(deltaAngle) > 1f)
        {
            currentState = InputState.Rotate;
            OnRotate?.Invoke(deltaAngle);
        }
    }

    // ========== PICK OBJECT ==========
    private GameObject GetObjectAtPosition(Vector2 screenPos)
    {
        if (cam == null)
        {
            Debug.LogWarning("⚠️ Không tìm thấy MainCamera!");
            return null;
        }

        Vector3 sp = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.nearClipPlane + 1f));
        Vector3 worldPoint = cam.ScreenToWorldPoint(sp);

        int count = Physics2D.OverlapPointNonAlloc(worldPoint, s_hitBuf);
        if (count <= 0) return null;

        GameObject best = null;
        int bestLayerVal = int.MinValue;
        int bestOrder = int.MinValue;
        float bestZ = float.MaxValue;
        int bestId = int.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var col = s_hitBuf[i];
            if (!col || !col.enabled) continue;
            var go = col.transform.gameObject;

            int layerVal, order;
            if (!TryGetSorting(go, out layerVal, out order))
            {
                layerVal = int.MinValue;
                order = int.MinValue;
            }

            float zDist = Mathf.Abs(cam.transform.position.z - go.transform.position.z);
            int id = go.GetInstanceID();

            bool better =
                (layerVal > bestLayerVal) ||
                (layerVal == bestLayerVal && order > bestOrder) ||
                (layerVal == bestLayerVal && order == bestOrder && zDist < bestZ) ||
                (layerVal == bestLayerVal && order == bestOrder && Mathf.Approximately(zDist, bestZ) && id < bestId);

            if (better)
            {
                best = go;
                bestLayerVal = layerVal;
                bestOrder = order;
                bestZ = zDist;
                bestId = id;
            }
        }

        return best;
    }

    private static bool TryGetSorting(GameObject go, out int layerVal, out int order)
    {
        var sg = go.GetComponent<SortingGroup>();
        if (sg)
        {
            layerVal = SortingLayer.GetLayerValueFromID(sg.sortingLayerID);
            order = sg.sortingOrder;
            return true;
        }

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr)
        {
            layerVal = SortingLayer.GetLayerValueFromID(sr.sortingLayerID);
            order = sr.sortingOrder;
            return true;
        }

        layerVal = 0; order = 0;
        return false;
    }
}
