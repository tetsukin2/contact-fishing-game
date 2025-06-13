using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Facade for input device management components
/// </summary>
[RequireComponent(typeof(BLEDevice))]
[RequireComponent(typeof(IMUInput))]
[RequireComponent(typeof(JoystickInput))]
[RequireComponent(typeof(ButtonInput))]
[RequireComponent(typeof(InputDeviceRotationHelper))]
[RequireComponent(typeof(BrailleOutput))]
public class InputDeviceManager : SingletonPersistent<InputDeviceManager>
{
    public BLEDevice BLEDevice { get; private set; }
    public IMUInput IMUInput { get; private set; }
    public JoystickInput JoystickInput { get; private set; }
    public ButtonInput ButtonInput { get; private set; }
    public BrailleOutput BrailleOutput { get; private set; }
    public InputDeviceRotationHelper RotationHelper { get; private set; }

    /// <summary>
    /// Triggered when device status is updated. Accepts status message as parameter.
    /// </summary>
    public UnityEvent<string> StatusUpdated { get; private set; } = new();

    protected override void OnAwake()
    {
        BLEDevice = GetComponent<BLEDevice>();
        IMUInput = GetComponent<IMUInput>();
        JoystickInput = GetComponent<JoystickInput>();
        ButtonInput = GetComponent<ButtonInput>();
        BrailleOutput = GetComponent<BrailleOutput>();
        RotationHelper = GetComponent<InputDeviceRotationHelper>();
    }

    /// <summary>
    /// Queues a connection status log and event to the main thread
    /// </summary>
    /// <param name="message"></param>
    public void QueueStatusLog(string message)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() => StatusUpdated.Invoke(message));
        Debug.Log(message);
    }
}
