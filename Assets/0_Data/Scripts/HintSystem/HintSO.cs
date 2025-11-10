using UnityEngine;

/// <summary>
/// Chứa dữ liệu gợi ý (Hint) cho từng level.
/// Có thể có nhiều gợi ý text, hình ảnh, và vị trí pointer.
/// </summary>
[CreateAssetMenu(menuName = "Game/HintSO", fileName = "Hint_")]
public class HintSO : ScriptableObject
{
    public int levelIndex = 1;
    public LocalizationKey key;            // key để lấy câu hỏi từ hệ thống dịch thuật
    public int countReward = 2;          // số hint thưởng khi hoàn thành level
}
