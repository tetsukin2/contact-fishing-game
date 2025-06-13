using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Exposes Joystick input value.
/// </summary>
public class JoystickInput : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool showJoystickData = false;

    private Vector2 joystickCenter = Vector2.zero;
    private bool calibrated = false;

    public Vector2 Value { get; private set; } = Vector2.zero;

    /// <summary>
    /// Invoked on the frame the joystick is pressed.
    /// </summary>
    [SerializeField] private UnityEvent _joystickPressed = new();

    public bool JoystickHeld { get; private set; } = false;

    public UnityEvent JoystickPressed => _joystickPressed;

    /// <summary>
    /// Begins the process of reading Joystick data.
    /// </summary>=
    public void StartReadingJoystickData(string characteristicUUID)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() => StartCoroutine(ReadJoystickData(characteristicUUID)));
    }

    private IEnumerator ReadJoystickData(string characteristicUuid)
    {
        while (true)
        {
            bool hasData = BleApi.PollData(out BleApi.BLEData data, false);

            if (hasData && data.characteristicUuid.ToLower().Contains(characteristicUuid.ToLower()))
            {
                if (data.size >= 3)
                {
                    byte rawX = data.buf[0];
                    byte rawY = data.buf[1];
                    byte sw = data.buf[2];
                    //Debug.Log(sw);

                    float normX = (rawX - 128) / 128f;
                    float normY = (rawY - 128) / 128f;

                    Vector2 rawInput = new(normX, normY);

                    if (!calibrated && rawInput != Vector2.zero)
                    {
                        joystickCenter = rawInput;
                        calibrated = true;
                        InputDeviceManager.Instance.QueueStatusLog($"JoystickCursor Center Calibrated: {joystickCenter}");
                    }

                    // Adjust for calibrated center
                    Vector2 adjustedInputRaw = rawInput - joystickCenter;

                    // Compute per-axis scaling, as center calibration may not be 0,0
                    // Some inversion here
                    float scaledY = (adjustedInputRaw.x >= 0)
                        ? adjustedInputRaw.x / (1f - joystickCenter.x)
                        : adjustedInputRaw.x / (1f + joystickCenter.x);

                    float scaledX = (adjustedInputRaw.y >= 0)
                        ? adjustedInputRaw.y / (1f - joystickCenter.y)
                        : adjustedInputRaw.y / (1f + joystickCenter.y);

                    Vector2 adjustedInput = new(scaledX, scaledY);

                    // Deadzone
                    if (adjustedInput.magnitude < 0.2f)
                        adjustedInput = Vector2.zero;

                    Value = adjustedInput;
                    bool wasJoystickPreviouslyPressed = JoystickHeld;
                    JoystickHeld = (sw == 1) || Input.GetKey(KeyCode.T);
                    //Debug.Log($"{wasJoystickPreviouslyPressed}, {JoystickHeld}");
                    if (!wasJoystickPreviouslyPressed && JoystickHeld)
                    {
                        UnityMainThreadDispatcher.Instance.Enqueue(() => JoystickPressed.Invoke());
                        if (showJoystickData) Debug.Log("Joystick Pressed!");
                    }
                    if (showJoystickData) Debug.Log(Value);

                    //Debug.Log(Input.GetKey(KeyCode.T));

                    //Debug.Log($"JoystickCursor: ({normX:F2}, {normY:F2}), {JoystickInput}");
                }
            }
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
