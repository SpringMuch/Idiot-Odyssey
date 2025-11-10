using UnityEngine;

public abstract class ObjectController : MonoBehaviour
{
    [Header("Scale/Rotate (2D)")]
    public float minScale = 0.5f;
    public float maxScale = 3f;
    public float scaleSpeed = 0.1f;
    public float rotateSpeed = 1f;

    [SerializeField] protected bool allowTranslate = true;
    [SerializeField] protected bool allowScale = true;
    [SerializeField] protected bool allowRotate = true;

    protected bool isSelected = false;
    [SerializeField] protected string objectID;
    public string GetObjectID => objectID;

    // Drag state (để kéo mượt với offset)
    private bool _dragging;
    private Vector3 _dragOffset;
    private float _dragDepth;

    protected abstract void OnTapped();
    protected abstract void OnPressed();

    protected virtual void OnEnable()
    {
        if (MobileInputManager.Instance == null) return;

        MobileInputManager.Instance.OnObjectSelected += OnObjectSelected;
        MobileInputManager.Instance.OnObjectDeselected += OnObjectDeselected;

        MobileInputManager.Instance.OnTap += HandleTap;
        MobileInputManager.Instance.OnPress += HandlePress;
        MobileInputManager.Instance.OnPinch += HandlePinch;
        MobileInputManager.Instance.OnRotate += HandleRotate;
        MobileInputManager.Instance.OnDrag += HandleDrag;
    }

    protected virtual void OnDisable()
    {
        if (MobileInputManager.Instance == null) return;

        MobileInputManager.Instance.OnObjectSelected -= OnObjectSelected;
        MobileInputManager.Instance.OnObjectDeselected -= OnObjectDeselected;

        MobileInputManager.Instance.OnTap -= HandleTap;
        MobileInputManager.Instance.OnPress -= HandlePress;
        MobileInputManager.Instance.OnPinch -= HandlePinch;
        MobileInputManager.Instance.OnRotate -= HandleRotate;
        MobileInputManager.Instance.OnDrag -= HandleDrag;
    }

    protected virtual void OnObjectSelected(string id)
    {
        isSelected = (id == objectID);
        if (!isSelected)
        {
            _dragging = false;
        }
    }

    protected virtual void OnObjectDeselected()
    {
        isSelected = false;
        _dragging = false;
    }

    void HandleTap(Vector2 pos)
    {
        if (!isSelected) return;
        OnTapped();
    }

    void HandlePress(Vector2 pos)
    {
        if (!isSelected) return;
        OnPressed();
    }

    void HandlePinch(float delta)
    {
        if (!isSelected || !allowScale) return;
        float currentScale = transform.localScale.x;
        float newScale = Mathf.Clamp(currentScale + delta * scaleSpeed, minScale, maxScale);
        transform.localScale = new Vector3(newScale, newScale, 1f);
    }

    void HandleRotate(float angle)
    {
        if (!isSelected || !allowRotate) return;
        transform.Rotate(Vector3.forward, -angle * rotateSpeed);
    }

    void HandleDrag(Vector2 start, Vector2 end)
    {
        if (!isSelected || !allowTranslate) return;

        var cam = Camera.main;
        if (!cam) return;

        if (!_dragging)
        {
            _dragging = true;
            _dragDepth = cam.WorldToScreenPoint(transform.position).z;

            Vector3 startWorld = cam.ScreenToWorldPoint(new Vector3(start.x, start.y, _dragDepth));
            _dragOffset = transform.position - startWorld;
        }

        Vector3 endWorld = cam.ScreenToWorldPoint(new Vector3(end.x, end.y, _dragDepth));
        endWorld += _dragOffset;
        endWorld.z = transform.position.z;
        transform.position = endWorld;
    }
}
