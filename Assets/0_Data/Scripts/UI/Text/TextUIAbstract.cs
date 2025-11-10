using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class TextUIAbstract : MonoBehaviour
{
    protected TextMeshProUGUI textComponent;
    protected virtual void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }
    public abstract void SetText(string content);
}
