using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Tooltip("Parent dùng để chứa các object spawn trong runtime (tập trung để dễ clear).")]
    [SerializeField] private Transform runtimeParent;
    [SerializeField] private RectTransform uiParent;

    private readonly List<GameObject> spawned = new List<GameObject>();

    public bool HasActiveLevel => spawned.Count > 0 || (runtimeParent != null && runtimeParent.childCount > 0) || (uiParent != null && uiParent.childCount > 0);
    
    // <-- THÊM DÒNG NÀY
    public Transform RuntimeParent => runtimeParent;


    private void Reset()
    {
        if (runtimeParent == null) runtimeParent = this.transform;
    }
    
    public void SpawnLevel(LevelSO so)
    {
        ClearLevel();

        if (so == null) return;

        foreach (var item in so.items)
        {
            for (int i = 0; i < Mathf.Max(1, item.count); i++)
            {
                Vector3 pos = new Vector3(item.position.x, item.position.y, 0f);
                GameObject go = GameObjectSpawn.Instance.Spawn(item.prefab.GetInstanceID(), pos, Quaternion.identity);
                if(go == null) continue;
                go.name = $"{item.prefab.name}_L{so.levelIndex}_{i}";
                go.transform.localScale = new Vector3(item.scale.x, item.scale.y, 1f);
                if (go.TryGetComponent<RectTransform>(out RectTransform rect))
                {
                    GameObjectSpawn.Instance.SetParentUI(rect, uiParent); 
                }
                else
                {
                    GameObjectSpawn.Instance.SetParent(go.transform,runtimeParent);
                }

                spawned.Add(go);
            }
        }
    }
    public void ClearLevel()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            var go = spawned[i];
            if (go != null)
            {
                GameObjectSpawn.Instance.DeSapwn(go);
            }
        }
        spawned.Clear();
    }
}