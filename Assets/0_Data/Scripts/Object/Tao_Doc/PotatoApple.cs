using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotatoApple : MonoBehaviour
{
    [SerializeField] private int countWin = 3;
    int count = 0;
    [SerializeField] private float delayWin = 2f;

    private void Start() {

    }
    private void OnDisable()
    {
        CancelInvoke(nameof(WinDelay));
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        ChooseCorrectAppleSystem(collision);
    }

    private void ChooseCorrectAppleSystem(Collider2D collision)
    {
        AppleNormal appleNormal = collision.GetComponent<AppleNormal>();
        ApplePoinson applePoinson = collision.GetComponent<ApplePoinson>();
        if (appleNormal != null)
        {
            Destroy(appleNormal.gameObject);
            count++;
            if (count == countWin)
            {
                Invoke(nameof(WinDelay), delayWin);
            }
        }
        else if (applePoinson != null)
        {
            Destroy(applePoinson.gameObject);
            GameManager.Instance.Lose();
        }
    }
    private void WinDelay()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon(); 
    }
}
