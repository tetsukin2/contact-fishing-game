using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleJoystickUICursor : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private RectTransform _cursorRect;
    [SerializeField] private RectTransform _canvasRect;
    [SerializeField] private Vector2 _resetPosition = Vector2.zero;
    [SerializeField] private float _cursorSpeed = 700f;

    [Header("Raycast")]
    [SerializeField] private GraphicRaycaster _graphicRaycaster;
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private CanvasGroup _cursorCanvasGroup;

    [Header("Fallback Debug Input")]
    [SerializeField] private KeyCode _fallbackSubmitKey = KeyCode.T;

    private Vector2 _currentCursorPos;
    private PointerEventData _pointerEventData;
    private bool _isEnabled;
    private bool _wasSubmitHeldLastFrame = false;

    private void Awake()
    {
        _pointerEventData = new PointerEventData(_eventSystem);
        _currentCursorPos = _resetPosition;
        _cursorRect.anchoredPosition = _currentCursorPos;
    }

    private void Update()
    {
        if (!_isEnabled)
            return;

        UpdateCursorPosition();

        if (GetSubmitDown())
            ClickCurrentUI();
    }

    public void EnableCursor(bool enable)
    {
        _isEnabled = enable;
        gameObject.SetActive(enable);

        if (enable)
        {
            _currentCursorPos = _resetPosition;
            _cursorRect.anchoredPosition = _currentCursorPos;
        }

        _wasSubmitHeldLastFrame = false;

        if (_cursorCanvasGroup != null)
            _cursorCanvasGroup.alpha = enable ? 1f : 0f;
    }

    private void UpdateCursorPosition()
    {
        Vector2 input = Vector2.zero;

        if (InputDeviceManager.Instance != null &&
            InputDeviceManager.Instance.JoystickInput != null)
        {
            input = InputDeviceManager.Instance.JoystickInput.Value;
        }
        else
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        Vector2 delta = input * _cursorSpeed * Time.unscaledDeltaTime;
        _currentCursorPos += delta;

        Vector2 halfCursorSize = _cursorRect.rect.size * 0.5f;
        Vector2 halfCanvasSize = _canvasRect.rect.size * 0.5f;

        float minX = -halfCanvasSize.x + halfCursorSize.x;
        float maxX = halfCanvasSize.x - halfCursorSize.x;
        float minY = -halfCanvasSize.y + halfCursorSize.y;
        float maxY = halfCanvasSize.y - halfCursorSize.y;

        _currentCursorPos.x = Mathf.Clamp(_currentCursorPos.x, minX, maxX);
        _currentCursorPos.y = Mathf.Clamp(_currentCursorPos.y, minY, maxY);

        _cursorRect.anchoredPosition = _currentCursorPos;
    }

    private bool GetSubmitDown()
    {
        bool heldNow = false;

        if (InputDeviceManager.Instance != null &&
            InputDeviceManager.Instance.JoystickInput != null)
        {
            heldNow = InputDeviceManager.Instance.JoystickInput.JoystickHeld;
        }
        else
        {
            heldNow = Input.GetKey(_fallbackSubmitKey);
        }

        bool pressedThisFrame = !_wasSubmitHeldLastFrame && heldNow;
        _wasSubmitHeldLastFrame = heldNow;

        return pressedThisFrame;
    }

    private void ClickCurrentUI()
    {
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, _cursorRect.position);
        _pointerEventData.position = screenPosition;

        List<RaycastResult> results = new();
        _graphicRaycaster.Raycast(_pointerEventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject hitObject = results[i].gameObject;
            Button button = hitObject.GetComponent<Button>() ?? hitObject.GetComponentInParent<Button>();

            if (button == null || !button.interactable)
                continue;

            button.onClick.Invoke();
            break;
        }
    }
}