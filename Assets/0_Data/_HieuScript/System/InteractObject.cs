using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class InteractObject : ObjectController
{
    [SerializeField] float followSpeed = 20f;
    [SerializeField] bool smooth = true;

    Camera cam;
    bool dragging;
    float depthZ;
    Vector3 grabOffset, targetPos;

    protected override void OnEnable()
    {
        base.OnEnable();
        allowTranslate = false;
        cam = Camera.main;

        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.OnDrag += HandleDrag;
            MobileInputManager.Instance.OnObjectDeselected += StopDragging;
        }
    }

    protected override void OnDisable()
    {
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.OnDrag -= HandleDrag;
            MobileInputManager.Instance.OnObjectDeselected -= StopDragging;
        }
        base.OnDisable();
    }
    protected override void OnPressed(){}

    protected override void OnTapped(){}

    void HandleDrag(Vector2 start, Vector2 end)
    {
        if (!isSelected) return;
        if (!cam) cam = Camera.main;

        if (!dragging)
        {
            depthZ = cam.WorldToScreenPoint(transform.position).z;
            var worldStart = cam.ScreenToWorldPoint(new Vector3(start.x, start.y, depthZ));
            grabOffset = transform.position - worldStart; // giữ lệch tâm -> không “khựng”
            dragging = true;
        }

        var worldEnd = cam.ScreenToWorldPoint(new Vector3(end.x, end.y, depthZ));
        targetPos = worldEnd + grabOffset;

        if (!smooth) transform.position = targetPos;
    }

    void LateUpdate()
    {
        if (smooth && dragging)
        {
            float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
        }
    }

    void StopDragging() => dragging = false;

    public void SetScale(bool v)  => allowScale  = v;
    public void SetRotate(bool v) => allowRotate = v;
}
