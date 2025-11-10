using UnityEngine;

public class DisableInput : MonoBehaviour
{
    private bool _prevState;

    void OnEnable()
    {
        if (MobileInputManager.Instance != null)
        {
            _prevState = MobileInputManager.Instance.enabled;
            MobileInputManager.Instance.enabled = false;
        }
    }

    void OnDisable()
    {
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.enabled = _prevState;
        }
    }
}
