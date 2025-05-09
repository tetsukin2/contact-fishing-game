using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickCursor : MonoBehaviour
{
    public RectTransform CursorRect;       // Assign your UI cursor object here
    public RectTransform CanvasRect;       // Reference to your Canvas (should use Screen Space - Overlay or Camera)
    public Vector2 ResetPosition;
    public float CursorSpeed = 500f;             // Movement CursorSpeed in pixels per second
    [SerializeField] private JoystickCursorTooltip _tooltip; // Tooltip for the cursor

    [Space]
    [Header("Pins")] // I wanna read this easier for mapping later
    // Might swap this out for something to automatically register pins cuz this aint good manually
    [SerializeField] private CursorBraillePin _t00;
    [SerializeField] private CursorBraillePin _t01;
    [SerializeField] private CursorBraillePin _t02;
    [SerializeField] private CursorBraillePin _t03;
    [SerializeField] private CursorBraillePin _t10;
    [SerializeField] private CursorBraillePin _t11;
    [SerializeField] private CursorBraillePin _t12;
    [SerializeField] private CursorBraillePin _t13;
    [SerializeField] private CursorBraillePin _t20;
    [SerializeField] private CursorBraillePin _t21;
    [SerializeField] private CursorBraillePin _t22;
    [SerializeField] private CursorBraillePin _t23;
    [SerializeField] private CursorBraillePin _t30;
    [SerializeField] private CursorBraillePin _t31;
    [SerializeField] private CursorBraillePin _t32;
    [SerializeField] private CursorBraillePin _t33;

    [Space]
    [SerializeField] private CursorBraillePin _i00;
    [SerializeField] private CursorBraillePin _i01;
    [SerializeField] private CursorBraillePin _i02;
    [SerializeField] private CursorBraillePin _i03;
    [SerializeField] private CursorBraillePin _i10;
    [SerializeField] private CursorBraillePin _i11;
    [SerializeField] private CursorBraillePin _i12;
    [SerializeField] private CursorBraillePin _i13;
    [SerializeField] private CursorBraillePin _i20;
    [SerializeField] private CursorBraillePin _i21;
    [SerializeField] private CursorBraillePin _i22;
    [SerializeField] private CursorBraillePin _i23;
    [SerializeField] private CursorBraillePin _i30;
    [SerializeField] private CursorBraillePin _i31;
    [SerializeField] private CursorBraillePin _i32;
    [SerializeField] private CursorBraillePin _i33;

    public int T0 { get; private set; } = 0;
    public int T1 { get; private set; } = 0;
    public int I0 { get; private set; } = 0;
    public int I1 { get; private set; } = 0;
    private Vector2 CurrentCursorPos;

    public JoystickCursorTooltip Tooltip => _tooltip;

    private void Awake()
    {
        // Initialize the tooltip with the cursor and canvas rect transforms
        _tooltip.Initialize(CursorRect, CanvasRect);
    }

    void Start()
    {
        CurrentCursorPos = CursorRect.anchoredPosition;
        GameManager.Instance.GameStateUpdated.AddListener(OnGameStateUpdated);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.EncyclopediaState) return;
        UpdateCursorPosition();
        UpdateBrailleValues();
    }

    private void OnGameStateUpdated(GameState state)
    {
        if (state == GameManager.Instance.EncyclopediaState)
        {
            // Reset cursor position when entering the encyclopedia state
            CurrentCursorPos = ResetPosition;
            CursorRect.anchoredPosition = CurrentCursorPos;
        }
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
        int temp_t0 = 1 * _t00.Value + 8 * _t01.Value
            + 2 * _t10.Value + 16 * _t11.Value
            + 4 * _t20.Value + 32 * _t21.Value
            + 64 * _t30.Value + 128 * _t31.Value;
        int temp_t1 = 1 * _t02.Value + 8 * _t03.Value
            + 2 * _t12.Value + 16 * _t13.Value
            + 4 * _t22.Value + 32 * _t23.Value
            + 64 * _t32.Value + 128 * _t33.Value;

        int temp_i0 = 8 * _i20.Value + 1 * _i30.Value
          + 16 * _i21.Value + 2 * _i31.Value
          + 32 * _i22.Value + 4 * _i32.Value
          + 128 * _i23.Value + 64 * _i33.Value;

        int temp_i1 = 8 * _i00.Value + 1 * _i10.Value
            + 16 * _i01.Value + 2 * _i11.Value
            + 32 * _i02.Value + 4 * _i12.Value
            + 128 * _i03.Value + 64 * _i13.Value;

        if (temp_t0 != T0 || temp_t1 != T1 || temp_i0 != I0 || temp_i1 != I1)
        {
            T0 = temp_t0;
            T1 = temp_t1;
            I0 = temp_i0;
            I1 = temp_i1;
            InputDeviceManager.SendBrailleASCII(T0, T1, I0, I1);
        }
    }

    // Get the list of Braille pins for external access
    public List<CursorBraillePin> GetBraillePins()
    {
        return new List<CursorBraillePin>
            {
                _t00, _t01, _t02, _t03,
                _t10, _t11, _t12, _t13,
                _t20, _t21, _t22, _t23,
                _t30, _t31, _t32, _t33,
                _i00, _i01, _i02, _i03,
                _i10, _i11, _i12, _i13,
                _i20, _i21, _i22, _i23,
                _i30, _i31, _i32, _i33
            };
    }
}
