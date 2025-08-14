using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles BLE scanning, connecting, service/characteristic discovery
/// </summary>
public class BLEDevice : MonoBehaviour
{
    // === IDS ===
    public const string TARGET_DEVICE_NAME = "FishingRodIMU";

    public const string IMU_SERVICE_UUID = "19b10000-e8f2-537e-4f6c-d104768a1214";
    public const string IMU_CHARACTERISTIC_UUID = "19b10001-e8f2-537e-4f6c-d104768a1214";

    //private const string JOY_SERVICE_UUID = "19b20000-e8f2-537e-4f6c-d104768a1214";
    public const string JOY_CHARACTERISTIC_UUID = "19b20001-e8f2-537e-4f6c-d104768a1214";

    public const string BRAILLE_SERVICE_UUID = "19b30000-e8f2-537e-4f6c-d104768a1214";
    public const string BRAILLE_CHARACTERISTIC_UUID = "19b30001-e8f2-537e-4f6c-d104768a1214";

    /// <summary>
    /// The ID of the currently connected device.
    /// </summary>
    public string ConnectedDeviceID { get; private set; } = null;
    //private string imuCharUUID = null;
    //private string joyCharUUID = null;
    //public string BrailleCharacteristicUUID { get; private set; } = null; // characteristic uuid rn are hardcoded

    // === CONNECTION HEALTH ===
    [SerializeField] private bool _showConnectionDebug = false;
    [Tooltip("Time in seconds after which a ping is sent if no IMU data is received.")]
    [SerializeField] private float _healthPingInterval = 1.0f;
    [SerializeField] private float _healthPingTimeout = 5.0f;

    public float LastDeviceUpdateTime { get; private set; } = 0f;
    public float LastHealthCheckTime { get; private set; } = 0f;
    private byte pingByte = 1;
    private Coroutine _healthCheckRoutine;

    public bool IsConnected { get; private set; } = false;

    // === MISC ===
    private Thread scanThread;
    private bool isScanning = false;

    private bool imuCharacteristicLoaded = false;
    private bool joystickCharacteristicLoaded = false;
    private bool brailleCharacteristicLoaded = false;

    /// <summary>
    /// Event triggered when a connection attempt to the BLE Device starts.
    /// </summary>
    public UnityEvent ConnectionAttemptStarted { get; private set; } = new();
    public UnityEvent CharacteristicsLoaded { get; private set; } = new();

    private void Start()
    {
        // Data received event subscription
        InputDeviceManager.Instance.IMUInput.DataReceived += OnDataReceived;

        InputDeviceManager.Instance.QueueStatusLog("Resetting BLE Scanner...");

        BleApi.StopDeviceScan();
        Thread.Sleep(1000);
        BleApi.Quit();
    }

    #region Connection Health

    private IEnumerator CheckConnectionHealth()
    {
        while (IsConnected)
        {
            // No data or ping, send ping
            if (Time.time - LastDeviceUpdateTime > _healthPingInterval
                && Time.time - LastHealthCheckTime > _healthPingInterval)
            {
                SendIMUPing(ConnectedDeviceID);
                LastHealthCheckTime = Time.time; // Prevent flooding
            }

            // Nothing at all, try reconnect
            if (Time.time - LastDeviceUpdateTime > _healthPingTimeout)
            {
                if (_showConnectionDebug) Debug.Log("IMU data timeout exceeded. No data received for a long time.");
                HandleReconnect();
                break;
            }

            yield return new WaitForSecondsRealtime(_healthPingInterval);
        }
    }

    private void OnDataReceived(BleApi.BLEData _)
    {
        LastDeviceUpdateTime = Time.time;
        //if (_showConnectionDebug) Debug.Log("Received BLE ping response.");
    }

    private void SendIMUPing(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId)) return;

        byte[] payload = new byte[] { pingByte };

        BleApi.BLEData bleData = new BleApi.BLEData
        {
            buf = new byte[512],
            size = (short)payload.Length,
            deviceId = deviceId,
            serviceUuid = IMU_SERVICE_UUID,
            characteristicUuid = IMU_CHARACTERISTIC_UUID
        };

        System.Array.Copy(payload, bleData.buf, payload.Length);

        BleApi.SendData(in bleData, false);
        if (_showConnectionDebug) Debug.Log("Pinging idle BLE Device...");
    }

    private void HandleReconnect()
    {
        IsConnected = false;
        imuCharacteristicLoaded = false;
        joystickCharacteristicLoaded = false;
        brailleCharacteristicLoaded = false;

        InputDeviceManager.Instance.QueueStatusLog("BLE device disconnected. Attempting to reconnect...");

        // You can choose to rescan after delay or immediately
        Thread.Sleep(1000);
        StartConnectionAttempt();
    }

    #endregion

    /// <summary>
    /// Starts the connection attempt to the BLE device. Only runs if not currently scanning.
    /// </summary>
    public void StartConnectionAttempt()
    {
        if (isScanning) return; // Only start scanning once

        BleApi.StopDeviceScan();
        //BleApi.StartDeviceScan();
        scanThread = new Thread(ScanForDevices);
        scanThread.Start();
        //isScanning = true;
        RunWhenConnected(() => LastDeviceUpdateTime = Time.time); // Reset last update time on successful connection
        ConnectionAttemptStarted.Invoke();
        Debug.Log("Starting Scan");
    }

    void ScanForDevices()
    {
        InputDeviceManager.Instance.QueueStatusLog("Scanning for FishingRodIMU...");
        isScanning = true;

        while (isScanning)
        {
            System.DateTime scanStartTime = System.DateTime.Now;
            BleApi.StartDeviceScan();

            bool found = false;

            while ((System.DateTime.Now - scanStartTime).TotalMilliseconds < 2000)
            {
                BleApi.DeviceUpdate device = new BleApi.DeviceUpdate();
                BleApi.ScanStatus status = BleApi.PollDevice(ref device, true);

                if (status == BleApi.ScanStatus.AVAILABLE &&
                    !string.IsNullOrEmpty(device.name) &&
                    device.name.Contains(TARGET_DEVICE_NAME))
                {
                    InputDeviceManager.Instance.QueueStatusLog($"Found {TARGET_DEVICE_NAME}! Connecting...");
                    ConnectedDeviceID = device.id;
                    isScanning = false;
                    BleApi.StopDeviceScan();

                    Thread.Sleep(500);
                    ConnectToDevice(ConnectedDeviceID);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                InputDeviceManager.Instance.QueueStatusLog("Rescanning... Target device not found in this window.");
                BleApi.StopDeviceScan();
                Thread.Sleep(500);
            }
        }
    }

    void ConnectToDevice(string deviceId)
    {
        InputDeviceManager.Instance.QueueStatusLog($"Connecting to {TARGET_DEVICE_NAME}...");
        BleApi.ScanServices(deviceId);

        while (BleApi.PollService(out BleApi.Service service, true) == BleApi.ScanStatus.AVAILABLE)
        {
            InputDeviceManager.Instance.QueueStatusLog($"Service Found: {service.uuid}");

            BleApi.ScanCharacteristics(deviceId, service.uuid);

            while (BleApi.PollCharacteristic(out BleApi.Characteristic characteristic, true) == BleApi.ScanStatus.AVAILABLE)
            {
                Debug.Log(characteristic.uuid.ToLower());

                if (!imuCharacteristicLoaded &&
                    characteristic.uuid.ToLower().Contains(IMU_CHARACTERISTIC_UUID.ToLower()))
                {
                    imuCharacteristicLoaded = true;
                    InputDeviceManager.Instance.QueueStatusLog("IMU Characteristic Found!");
                    SubscribeToIMU(deviceId, service.uuid, characteristic.uuid);
                }
                else if (!joystickCharacteristicLoaded &&
                    characteristic.uuid.ToLower().Contains(JOY_CHARACTERISTIC_UUID.ToLower()))
                {
                    joystickCharacteristicLoaded = true;
                    InputDeviceManager.Instance.QueueStatusLog("JoystickCursor Characteristic Found!");
                    SubscribeToJoystick(deviceId, service.uuid, characteristic.uuid);
                }
                else if (!brailleCharacteristicLoaded &&
                    characteristic.uuid.ToLower().Contains(BRAILLE_CHARACTERISTIC_UUID.ToLower()))
                {
                    brailleCharacteristicLoaded = true;
                    InputDeviceManager.Instance.QueueStatusLog("Braille Characteristic Found!");
                }
                if (imuCharacteristicLoaded && joystickCharacteristicLoaded && brailleCharacteristicLoaded)
                {
                    InputDeviceManager.Instance.QueueStatusLog("All Characteristics Loaded!");
                    UnityMainThreadDispatcher.Instance.Enqueue(() => CharacteristicsLoaded.Invoke());
                    IsConnected = true;
                    UnityMainThreadDispatcher.Instance.Enqueue(StartConnectionHealthRoutine);
                    //LastDeviceUpdateTime = Time.time; // Reset last update time on successful connection
                    InputDeviceManager.Instance.ButtonInput.StartReadingButtonData(""); // placeholder until proper button
                    return;
                }
            }
        }
    }

    private void StartConnectionHealthRoutine()
    {
        if (_showConnectionDebug) Debug.Log("Starting connection health check routine...");
        if (_healthCheckRoutine != null)
        {
            StopCoroutine(_healthCheckRoutine);
        }
        _healthCheckRoutine = StartCoroutine(CheckConnectionHealth());
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
                InputDeviceManager.Instance.QueueStatusLog("Subscribed to IMU!");
                InputDeviceManager.Instance.IMUInput.StartReadingIMUData(characteristicUuid);
                return;
            }
        }
        Debug.LogError("Failed to subscribe to IMU after retries.");
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
                InputDeviceManager.Instance.QueueStatusLog("Subscribed to JoystickCursor!");
                InputDeviceManager.Instance.JoystickInput.StartReadingJoystickData(characteristicUuid);
                return;
            }
        }
        Debug.LogError("Failed to subscribe to JoystickCursor after retries.");
    }

    /// <summary>
    /// Executes the given action if connected, or waits until characteristics are Loaded before execution.
    /// </summary>
    public void RunWhenConnected(UnityAction action)
    {
        if (IsConnected)
        {
            action();
        }
        else
        {
            // Stop listening once event fires and action is executed
            void HandleActionSubscription()
            {
                CharacteristicsLoaded.RemoveListener(HandleActionSubscription);
                action();
            }
            CharacteristicsLoaded.AddListener(HandleActionSubscription);
        }
    }

    private void OnApplicationQuit()
    {
        isScanning = false;

        // Disconnect data received events
        InputDeviceManager.Instance.IMUInput.DataReceived -= OnDataReceived;

        if (!string.IsNullOrEmpty(ConnectedDeviceID))
        {
            Debug.Log("Unsubscribing from BLE characteristic before quitting...");
            BleApi.SubscribeCharacteristic(ConnectedDeviceID, IMU_SERVICE_UUID, IMU_CHARACTERISTIC_UUID, false);
        }

        Debug.Log("Stopping BLE scan...");
        BleApi.StopDeviceScan();

        scanThread?.Abort();

        Debug.Log("Full BLE Reset...");
        BleApi.Quit();
    }
}
