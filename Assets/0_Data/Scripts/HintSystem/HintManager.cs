using UnityEngine;
using TMPro;
using System.Collections.Generic;
[DefaultExecutionOrder(-900)]
public class HintManager : MonoBehaviour
{
    public static HintManager Instance { get; private set; }

    // [Header("UI")]
    // [SerializeField] private TextMeshProUGUI hintText;
    // [Header("State")]
    // [SerializeField] private int hintCount = 0;
    [SerializeField] private List<HintSO> hints = new List<HintSO>();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        //UpdateHintDisplay();
    }
    
    public HintSO GetHintSO()
    {
        foreach (var hint in hints)
        {
            if (hint == null) continue;
            int index = LevelManager.Instance.CurrentLevelIndex;
            if (hint.levelIndex == index) return hint;
        }
        return null;
    }
}
