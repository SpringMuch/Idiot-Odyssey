using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractButtonUI : MonoBehaviour
{
    [SerializeField] protected bool isActive;
    protected Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    void Start()
    {
        button.onClick.AddListener(OnClick);
    }
    public abstract void OnClick();
}
