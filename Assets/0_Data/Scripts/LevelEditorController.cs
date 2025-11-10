using UnityEngine;

[ExecuteInEditMode]
public class LevelEditorController : MonoBehaviour
{
    [Tooltip("Chọn LevelSO để edit (vị trí điểm sẽ hiển thị trong Scene).")]
    public LevelSO editingLevel;

    [Tooltip("Gizmos size")]
    public float gizmoSize = 0.15f;

    // dùng để highlight item index khi chỉnh bằng custom editor
    [HideInInspector] public int selectedIndex = -1;

    private void OnDrawGizmos()
    {
        if (editingLevel == null || !editingLevel.showGizmos) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < editingLevel.items.Count; i++)
        {
            var item = editingLevel.items[i];
            Vector3 pos = new Vector3(item.position.x, item.position.y, 0f);
            if (i == selectedIndex) Gizmos.color = Color.cyan;
            else Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(pos, gizmoSize);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(pos + Vector3.up * (gizmoSize + 0.05f), $"{i}: {item.pointName}");
#endif
        }
    }
}
