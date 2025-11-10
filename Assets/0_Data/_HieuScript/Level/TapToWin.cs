using UnityEngine;
using System.Collections;
public class TapToWin : CheckAnswerBase
{
    [Header("Đáp án đúng hay sai")]
    [Tooltip("Đặt true nếu đây là đáp án đúng.")]
    public bool isCorrect = false;
    protected override bool EvaluateCondition()
    {
        return isCorrect;
    }
}
