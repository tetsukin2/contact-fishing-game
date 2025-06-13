using System.Threading;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles BLE scanning, connecting, service/characteristic discovery
/// </summary>
public class BLEDevice : MonoBehaviour
{
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

    private Thread scanThread;
    private bool isScanning = true;

    private bool imuCharacteristicLoaded = false;
    private bool joystickCharacteristicLoaded = false;
    private bool brailleCharacteristicLoaded = false;

    public bool IsConnected { get; private set; } = false;

    public UnityEvent CharacteristicsLoaded { get; private set; } = new();

    private void Start()
    {
        InputDeviceManager.Instance.QueueStatusLog("Restarting BLE Scanner...");

        BleApi.StopDeviceScan();
        Thread.Sleep(1000);
        BleApi.Quit();

        BleApi.StartDeviceScan();

        scanThread = new Thread(ScanForDevices);
        scanThread.Start();
    }

    void ScanForDevices()
    {
        InputDeviceManager.Instance.QueueStatusLog("Scanning for FishingRodIMU...");

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
                    InputDeviceManager.Instance.ButtonInput.StartReadingButtonData(""); // placeholder until proper button
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
