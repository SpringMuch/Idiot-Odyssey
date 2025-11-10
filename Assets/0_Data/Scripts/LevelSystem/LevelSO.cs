using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/LevelSO", fileName = "Level_")]
public class LevelSO : ScriptableObject
{
    public int levelIndex = 1;
    public string levelName = "Level";
    public Sprite previewImage;
    public LocalizationKey key;
    public bool showGizmos = true;

    [Tooltip("Danh sách object sẽ spawn khi level bắt đầu.")]
    public List<ItemSpawnInfo> items = new List<ItemSpawnInfo>();

}
[System.Serializable]
public class ItemSpawnInfo
{
    public string id = "";                // tùy chọn để tham chiếu
    public string pointName = "";         // tên point (ví dụ "A", "Start", "Table")
    public GameObject prefab;             // prefab để spawn
    public Vector2 position;              // world position lưu trong SO (2D game)
    public float rotation;                // góc quay (deg)
    public Vector2 scale = Vector2.one;   // scale override (optional)
    public int count = 1;                 // số lượng spawn (mặc định 1)
}
