using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TapToDespawn : MonoBehaviour, IPointerDownHandler
{
    [Header("Win")]
    [SerializeField] private float winDelay = 1f;
    public void OnPointerDown(PointerEventData eventData)
    {
        onHandle();
    }
    private void OnMouseDown()
    {
        onHandle();
    }
    private void onHandle()
    {
        GameObjectSpawn.Instance.DeSapwn(this.gameObject);
        Invoke(nameof(DelayWin), winDelay);
    }
    private void DelayWin()
    {
        SoundManager.PlaySfx(SoundTypes.Win);
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
    }
    void OnDisable()
    {
        CancelInvoke(nameof(DelayWin));
    }
}
