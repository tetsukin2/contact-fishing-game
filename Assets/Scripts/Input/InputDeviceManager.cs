using System;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }

    public enum RotationAxis { x, y, z }

    private const string TARGET_DEVICE_NAME = "FishingRodIMU";

    private const string IMU_SERVICE_UUID = "19b10000-e8f2-537e-4f6c-d104768a1214"; 
    private const string IMU_CHARACTERISTIC_UUID = "19b10001-e8f2-537e-4f6c-d104768a1214"; 

    //private const string JOY_SERVICE_UUID = "19b20000-e8f2-537e-4f6c-d104768a1214";
    private const string JOY_CHARACTERISTIC_UUID = "19b20001-e8f2-537e-4f6c-d104768a1214";

    private const string BRAILLE_SERVICE_UUID = "19b30000-e8f2-537e-4f6c-d104768a1214";
    private const string BRAILLE_CHARACTERISTIC_UUID = "19b30001-e8f2-537e-4f6c-d104768a1214";

    private static string targetDeviceId = null;
    //private string imuCharUUID = null;
    //private string joyCharUUID = null;
    private static string brailleCharUUID = null;

    [Header("Debugging")]
    [SerializeField] private bool showIMUData = false;
    [SerializeField] private bool showBrailleData = false;
    [SerializeField] private bool showJoystickData = false;

    private Thread scanThread;
    private bool isScanning = true;

    private static Vector3 imuRotationRaw = Vector3.zero; 
    public static Vector2 JoystickInput { get; private set; } = Vector2.zero;
    public static bool JoystickHeld = false;
    public static bool IsConnected = false;

    private static Vector2 joystickCenter = Vector2.zero;
    private static bool calibrated = false;

    private UnityEvent<string> _connectionStatusLog = new();
    private UnityEvent _characteristicsLoaded = new();
    private UnityEvent _joystickPressed = new();

    public UnityEvent<string> ConnectionStatusLog => _connectionStatusLog;
    public UnityEvent CharacteristicsLoaded => _characteristicsLoaded;
    public UnityEvent JoystickPressed => _joystickPressed;

    /// <summary>~
    /// Returns IMU rotation in ~~degrees~~
    /// </summary>
    public static Vector3 IMURotation => imuRotationRaw;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        QueueConnectionStatusLog("Restarting BLE Scanner...");

        BleApi.StopDeviceScan();
        Thread.Sleep(1000);
        BleApi.Quit();

        BleApi.StartDeviceScan();

        scanThread = new Thread(ScanForDevices);
        scanThread.Start();
    }
     void ScanForDevices()
    {
        QueueConnectionStatusLog("Scanning for FishingRodIMU...");

        while (isScanning)
        {
            DateTime scanStartTime = DateTime.Now;
            BleApi.StartDeviceScan();

            bool found = false;

            while ((DateTime.Now - scanStartTime).TotalMilliseconds < 2000)
            {
                BleApi.DeviceUpdate device = new BleApi.DeviceUpdate();
                BleApi.ScanStatus status = BleApi.PollDevice(ref device, true);

                if (status == BleApi.ScanStatus.AVAILABLE && 
                    !string.IsNullOrEmpty(device.name) && 
                    device.name.Contains(TARGET_DEVICE_NAME))
                {
                    QueueConnectionStatusLog($"Found {TARGET_DEVICE_NAME}! Connecting...");
                    targetDeviceId = device.id;
                    isScanning = false;
                    BleApi.StopDeviceScan();

                    Thread.Sleep(500);
                    ConnectToDevice(targetDeviceId);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                QueueConnectionStatusLog("Rescanning... Target device not found in this window.");
                BleApi.StopDeviceScan();
                Thread.Sleep(500);
            }
        }
    }

    void ConnectToDevice(string deviceId)
    {
        QueueConnectionStatusLog($"Connecting to {TARGET_DEVICE_NAME}...");
        BleApi.ScanServices(deviceId);
        IsConnected = true;

        BleApi.Service service;
        int characteristicsLoaded = 0;
        while (BleApi.PollService(out service, true) == BleApi.ScanStatus.AVAILABLE)
        {
            QueueConnectionStatusLog($"Service Found: {service.uuid}");

            BleApi.ScanCharacteristics(deviceId, service.uuid);
            BleApi.Characteristic characteristic;

            while (BleApi.PollCharacteristic(out characteristic, true) == BleApi.ScanStatus.AVAILABLE)
            {
                if (characteristic.uuid.ToLower().Contains(IMU_CHARACTERISTIC_UUID.ToLower()))
                {
                    QueueConnectionStatusLog("IMU Characteristic Found!");
                    SubscribeToIMU(deviceId, service.uuid, characteristic.uuid);
                    characteristicsLoaded++;
                }
                else if (characteristic.uuid.ToLower().Contains(JOY_CHARACTERISTIC_UUID.ToLower()))
                {
                    QueueConnectionStatusLog("JoystickCursor Characteristic Found!");
                    SubscribeToJoystick(deviceId, service.uuid, characteristic.uuid);
                    characteristicsLoaded++;
                }
                else if (characteristic.uuid.ToLower().Contains(BRAILLE_CHARACTERISTIC_UUID.ToLower()))
                {
                    QueueConnectionStatusLog("Braille Characteristic Found!");
                    brailleCharUUID = characteristic.uuid;
                    characteristicsLoaded++;
                }
                if (characteristicsLoaded >= 3)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => CharacteristicsLoaded.Invoke());
                    Debug.Log("All Characteristics Loaded!");
                    return;
                }
            }
        }
    }

    void SubscribeToIMU(string deviceId, string serviceUuid, string characteristicUuid)
    {
        for (int i = 0; i < 3; i++)
        {
            BleApi.SubscribeCharacteristic(deviceId, serviceUuid, characteristicUuid, false);
            Thread.Sleep(500);

            bool subscribed = BleApi.SubscribeCharacteristic(deviceId, serviceUuid, characteristicUuid, true);
            if (subscribed)
            {
                QueueConnectionStatusLog("Subscribed to IMU!");
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(ReadIMUData(deviceId, serviceUuid, characteristicUuid)));
                return;
            }
        }
        Debug.LogError("❌ Failed to subscribe to IMU after retries.");
    }

    void SubscribeToJoystick(string deviceId, string serviceUuid, string characteristicUuid)
    {
        for (int i = 0; i < 3; i++)
        {
            BleApi.SubscribeCharacteristic(deviceId, serviceUuid, characteristicUuid, false);
            Thread.Sleep(500);

            bool subscribed = BleApi.SubscribeCharacteristic(deviceId, serviceUuid, characteristicUuid, true);
            if (subscribed)
            {
                QueueConnectionStatusLog("Subscribed to JoystickCursor!");
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(ReadJoystickData(deviceId, serviceUuid, characteristicUuid)));
                return;
            }
        }
        Debug.LogError("❌ Failed to subscribe to JoystickCursor after retries.");
    }

    private IEnumerator ReadIMUData(string deviceId, string serviceUuid, string characteristicUuid)
    {
        BleApi.BLEData data;
        while (true)
        {
            bool hasData = BleApi.PollData(out data, false);

            if (hasData && data.characteristicUuid.ToLower().Contains(characteristicUuid.ToLower()))
            {
                if (data.size >= 6)
                {
                    short x = BitConverter.ToInt16(data.buf, 0);
                    short y = BitConverter.ToInt16(data.buf, 2);
                    short z = BitConverter.ToInt16(data.buf, 4);

                    //Debug.Log($"Raw IMU Data: X={x}, Y={y}, Z={z}");
                    imuRotationRaw = new Vector3(x / 1000f, y / 1000f, z / 1000f);

                    if (showIMUData) Debug.Log($"Processed IMU Rotation: {imuRotationRaw}");
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator ReadJoystickData(string deviceId, string serviceUuid, string characteristicUuid)
    {
        BleApi.BLEData data;
        while (true)
        {
            bool hasData = BleApi.PollData(out data, false);

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
                        QueueConnectionStatusLog($"JoystickCursor Center Calibrated: {joystickCenter}");
                    }

                    // Adjust for calibrated center
                    Vector2 adjustedInputRaw = rawInput - joystickCenter;

                    // Compute per-axis scaling, as center calibration may not be 0,0
                    // Some inversion here
                    float scaledY = ( adjustedInputRaw.x >= 0 )
                        ? adjustedInputRaw.x / (1f - joystickCenter.x)
                        : adjustedInputRaw.x / (1f + joystickCenter.x);

                    float scaledX = ( adjustedInputRaw.y >= 0 )
                        ? adjustedInputRaw.y / (1f - joystickCenter.y)
                        : adjustedInputRaw.y / (1f + joystickCenter.y);

                    Vector2 adjustedInput = new(scaledX, scaledY);

                    // Deadzone
                    if (adjustedInput.magnitude < 0.2f)
                        adjustedInput = Vector2.zero;

                    JoystickInput = adjustedInput;
                    bool wasJoystickPreviouslyPressed = JoystickHeld;
                    JoystickHeld = (sw == 1) || Input.GetKey(KeyCode.T);
                    //Debug.Log($"{wasJoystickPreviouslyPressed}, {JoystickHeld}");
                    if ( !wasJoystickPreviouslyPressed && JoystickHeld)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => JoystickPressed.Invoke());
                        Debug.Log("Joystick Pressed!");
                    }
                    if (showJoystickData) Debug.Log(JoystickInput);

                    //Debug.Log(Input.GetKey(KeyCode.T));

                    //Debug.Log($"JoystickCursor: ({normX:F2}, {normY:F2}), {JoystickInput}");
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    /// <summary>
    /// Resets the Braille display to a blank state (all cells off).
    /// </summary>
    public static void ResetBraille()
    {
        SendBrailleASCII(0, 0, 0, 0);
    }

    /// <summary>
    /// Sends a Braille ASCII message to the target controller.
    /// </summary>
    /// <param name="t0">Thumb Cell 0</param>
    /// <param name="t1">Thumb Cell 1</param>
    /// <param name="i0">Index Cell 0</param>
    /// <param name="i1">Index Cell 1</param>
    public static void SendBrailleASCII(int t0, int t1, int i0, int i1)
    {
        if (!IsConnected)
        {
            Debug.Log("Not yet connected to BLE.");
            return;
        }

        string message = $"<{t0:D3}{t1:D3}{i0:D3}{i1:D3}>"; // "<AAABBBCCCDDD>"
        byte[] payload = Encoding.ASCII.GetBytes(message); // Should be exactly ? bytes

        BleApi.BLEData bleData = new()
        {
            buf = new byte[512],
            size = (short)payload.Length,
            deviceId = targetDeviceId,
            serviceUuid = BRAILLE_SERVICE_UUID,
            characteristicUuid = brailleCharUUID
        };

        Array.Copy(payload, bleData.buf, payload.Length);

        //if (showBrailleData) Debug.Log($"Sending ASCII Braille payload: {message} (Raw Hex: {BitConverter.ToString(payload)})");

        bool success = BleApi.SendData(in bleData, false);
        if (success)
            Debug.Log("Braille ASCII data sent successfully.");
        else
        {
                BleApi.GetError(out BleApi.ErrorMessage error);
                //Debug.LogError("Failed to send Braille data: " + error.msg);
        }
    }

    /// <summary>
    /// Queues a connection status log and event to the main thread
    /// </summary>
    /// <param name="message"></param>
    private void QueueConnectionStatusLog(string message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => ConnectionStatusLog.Invoke(message));
        Debug.Log(message);
    }

    void OnApplicationQuit()
    {
        isScanning = false;

        if (!string.IsNullOrEmpty(targetDeviceId))
        {
            Debug.Log("🚨 Unsubscribing from BLE characteristic before quitting...");
            BleApi.SubscribeCharacteristic(targetDeviceId, IMU_SERVICE_UUID, IMU_CHARACTERISTIC_UUID, false);
        }

        Debug.Log("❌ Stopping BLE scan...");
        BleApi.StopDeviceScan();
        
        scanThread?.Abort();

        Debug.Log("♻️ Full BLE Reset...");
        BleApi.Quit(); 
    }

}
