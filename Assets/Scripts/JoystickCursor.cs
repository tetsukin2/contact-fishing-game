using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class JoystickCursor : MonoBehaviour
{
    public RectTransform CursorRect;       // Assign your UI cursor object here
    public RectTransform CanvasRect;       // Reference to your Canvas (should use Screen Space - Overlay or Camera)
    public float CursorSpeed = 500f;             // Movement CursorSpeed in pixels per second

    [SerializeField] private BraillePin _00;
    [SerializeField] private BraillePin _01;
    [SerializeField] private BraillePin _02;
    [SerializeField] private BraillePin _03;
    [SerializeField] private BraillePin _10;
    [SerializeField] private BraillePin _11;
    [SerializeField] private BraillePin _12;
    [SerializeField] private BraillePin _13;
    [SerializeField] private BraillePin _20;
    [SerializeField] private BraillePin _21;
    [SerializeField] private BraillePin _22;
    [SerializeField] private BraillePin _23;
    [SerializeField] private BraillePin _30;
    [SerializeField] private BraillePin _31;
    [SerializeField] private BraillePin _32;
    [SerializeField] private BraillePin _33;

    public int BrailleVal1 { get; private set; } = 0;
    public int BrailleVal2 { get; private set; } = 0;
    private Vector2 CurrentCursorPos;

    void Start()
    {
        CurrentCursorPos = CursorRect.anchoredPosition;
    }

    void Update()
    {
        UpdateCursorPosition();
        UpdateBrailleValues();
    }

    private void UpdateCursorPosition()
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

    private void UpdateBrailleValues()
    {
        int val1 = 64 * _00.Value
            + 4 * _01.Value
            + 2 * _02.Value
            + _03.Value
            + 128 * _10.Value
            + 32 * _11.Value
            + 16 * _12.Value
            + 8 * _13.Value;
        int val2 = 64 * _20.Value
            + 4 * _21.Value
            + 2 * _22.Value
            + _23.Value
            + 128 * _30.Value
            + 32 * _31.Value
            + 16 * _32.Value
            + 8 * _33.Value;

        if (val1 != BrailleVal1 || val2 != BrailleVal2)
        {
            BrailleVal1 = val1;
            BrailleVal2 = val2;
            InputDeviceManager.SendBrailleASCII(BrailleVal1, BrailleVal2);
        }
    }
}
