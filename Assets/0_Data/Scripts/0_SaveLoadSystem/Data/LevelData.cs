using System;

[Serializable]
public class LevelData
{
    public int levelIndex;
    public bool isUnlocked;
    public bool isCompleted;
    [NonSerialized] public LevelSO levelSO;

    public LevelData(int index, bool unlocked = false)
    {
        levelIndex = index;
        isUnlocked = unlocked;
        isCompleted = false;
    }
}
