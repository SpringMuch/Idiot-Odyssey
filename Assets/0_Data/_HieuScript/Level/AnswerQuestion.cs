using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Level 3
[DisallowMultipleComponent]
public class AnswerQuestion : MonoBehaviour
{
    [Header("Cấu hình")]
    [SerializeField] private int targetAnswer = 12;
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 99;
    [SerializeField] private bool lockOnCorrect = true;

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Delay hiển thị trước khi Win")]
    [SerializeField, Min(0f)] private float winDelaySeconds = 0.6f;
    [SerializeField] private bool useRealtimeDelay = true;

    bool _locked;
    bool _resultFired;
    float _lastSubmitAt = -999f;

    void Awake()
    {
        if (inputField)
        {
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.keyboardType = TouchScreenKeyboardType.NumberPad;

            inputField.onSubmit.AddListener(HandleSubmitFromKeyboard);
            inputField.onEndEdit.AddListener(HandleSubmitFromKeyboard);
            inputField.onValueChanged.AddListener(SanitizeRange);
        }
        
        // HideFeedback(); // <-- THAY ĐỔI: Không cần ở đây nữa, đã chuyển vào ResetState
    }

    void OnEnable()
    {
        // <-- THAY ĐỔI: Gọi hàm reset trạng thái mỗi khi object được kích hoạt (tái sử dụng)
        ResetState();
    }

    private void HandleSubmitFromKeyboard(string _)
    {
        if (Time.unscaledTime - _lastSubmitAt < 0.1f) return;
        _lastSubmitAt = Time.unscaledTime;
        UI_Confirm();
    }

    public void UI_Confirm()
    {
        if (_locked || _resultFired || inputField == null || feedbackText == null) return;

        string raw = inputField.text;
        if (!int.TryParse(raw, out int value))
        {
            ShowFeedback("Answer Please!", Color.black);
            return;
        }

        value = Mathf.Clamp(value, minValue, maxValue);
        if (value.ToString() != raw) inputField.text = value.ToString();

        bool correct = (value == targetAnswer);
        if (correct)
        {
            // Khóa input ngay để tránh spam
            if (lockOnCorrect)
            {
                _locked = true;
                inputField.interactable = false;
            }

            ShowFeedback("Correct!", new Color(0.1f, 0.7f, 0.2f));
            SoundManager.PlaySfx(SoundTypes.Correct);

            _resultFired = true;
            StartCoroutine(WinAfterFeedback());
        }
        else
        {
            ShowFeedback("Dumb Guy!", new Color(0.9f, 0.2f, 0.2f));
            SoundManager.PlaySfx(SoundTypes.Wrong);
        }
    }

    private System.Collections.IEnumerator WinAfterFeedback()
    {
        Canvas.ForceUpdateCanvases();
        yield return null; // đợi hết frame

        // Chờ thêm một khoảng cho người chơi đọc feedback
        if (winDelaySeconds > 0f)
        {
            if (useRealtimeDelay) yield return new WaitForSecondsRealtime(winDelaySeconds);
            else yield return new WaitForSeconds(winDelaySeconds);
        }

        // Gọi Win sau khi đã cho UI hiển thị rõ ràng
        GameManager.Instance.Win();
        GameEventBus.RaiseGameWon();
        SoundManager.PlaySfx(SoundTypes.Win);
    }

    private void SanitizeRange(string _)
    {
        if (string.IsNullOrEmpty(inputField.text)) return;
        if (!int.TryParse(inputField.text, out int v)) return;

        int clamped = Mathf.Clamp(v, minValue, maxValue);
        if (clamped != v)
        {
            int caret = inputField.stringPosition;
            inputField.SetTextWithoutNotify(clamped.ToString());
            inputField.stringPosition = Mathf.Min(caret, inputField.text.Length);
        }
    }

    private void ShowFeedback(string msg, Color col)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = msg;
        feedbackText.color = col;

        // ép UI cập nhật ngay (hữu ích nếu ngay lập tức có overlay/animation)
        Canvas.ForceUpdateCanvases();
    }

    private void HideFeedback()
    {
        if (feedbackText) feedbackText.gameObject.SetActive(false);
    }

    public void SetTarget(int v) => targetAnswer = v;

    public void SetRange(int min, int max)
    {
        minValue = min; maxValue = max;
        if (minValue > maxValue) (minValue, maxValue) = (maxValue, minValue);
    }

    // <-- THAY ĐỔI: Tạo hàm private để reset
    private void ResetState()
    {
        _locked = false;
        _resultFired = false;
        _lastSubmitAt = -999f; // Reset cả biến chống spam

        if (inputField)
        {
            inputField.text = ""; // Xóa text cũ
            inputField.interactable = true;
            inputField.ActivateInputField();
            inputField.Select();
        }
        HideFeedback();
    }

    public void ResetQuiz()
    {
        ResetState();
    }
}