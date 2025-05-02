using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickCursor : MonoBehaviour
{
    public RectTransform CursorRect;       // Assign your UI cursor object here
    public RectTransform CanvasRect;       // Reference to your Canvas (should use Screen Space - Overlay or Camera)
    public float CursorSpeed = 500f;             // Movement CursorSpeed in pixels per second

    private Vector2 CurrentCursorPos;

    void Start()
    {
        CurrentCursorPos = CursorRect.anchoredPosition;
    }

    void Update()
    {
        // Get input vector (you can replace this with actual joystick input)
        Vector2 input = InputDeviceManager.JoystickInput;

        // Move position based on joystick
        Vector2 delta = CursorSpeed * Time.deltaTime * input;
        CurrentCursorPos += delta;

        // Clamp to stay inside the canvas rect
        Vector2 halfCursorSize = CursorRect.sizeDelta * 0.5f;
        Vector2 halfCanvasSize = CanvasRect.sizeDelta * 0.5f; // 0,0 is center of canvas man
        float minX = -halfCanvasSize.x + halfCursorSize.x;
        float maxX = halfCanvasSize.x - halfCursorSize.x;
        float minY = -halfCanvasSize.y + halfCursorSize.y;
        float maxY = halfCanvasSize.y - halfCursorSize.y;

        CurrentCursorPos.x = Mathf.Clamp(CurrentCursorPos.x, minX, maxX);
        CurrentCursorPos.y = Mathf.Clamp(CurrentCursorPos.y, minY, maxY);

        // Apply to cursor
        CursorRect.anchoredPosition = CurrentCursorPos;
    }
}
