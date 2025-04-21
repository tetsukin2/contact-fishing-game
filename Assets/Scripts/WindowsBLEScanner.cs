using System;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;

public class WindowsBLEScanner : MonoBehaviour
{
    private const string TARGET_DEVICE_NAME = "FishingRodIMU";

    private const string IMU_SERVICE_UUID = "19b10000-e8f2-537e-4f6c-d104768a1214"; 
    private const string IMU_CHARACTERISTIC_UUID = "19b10001-e8f2-537e-4f6c-d104768a1214"; 

    private const string JOY_SERVICE_UUID = "19b20000-e8f2-537e-4f6c-d104768a1214";
    private const string JOY_CHARACTERISTIC_UUID = "19b20001-e8f2-537e-4f6c-d104768a1214";

    private const string BRAILLE_SERVICE_UUID = "19b30000-e8f2-537e-4f6c-d104768a1214";
    private const string BRAILLE_CHARACTERISTIC_UUID = "19b30001-e8f2-537e-4f6c-d104768a1214";

    private static string targetDeviceId = null;
    private string imuCharUUID = null;
    private string joyCharUUID = null;
    private static string brailleCharUUID = null;


    private Thread scanThread;
    private bool isScanning = true;

    public static Vector3 imuRotation = Vector3.zero; 
    public static Vector2 joystickInput = Vector2.zero;
    public static bool joystickPressed = false;
    public static bool IsConnected = false;

    private static Vector2 joystickCenter = Vector2.zero;
    private static bool calibrated = false;


    void Start()
    {
        Debug.Log("⚡ Restarting BLE Scanner...");

        BleApi.StopDeviceScan();
        Thread.Sleep(1000);
        BleApi.Quit();

        BleApi.StartDeviceScan();

        scanThread = new Thread(ScanForDevices);
        scanThread.Start();
    }
 void ScanForDevices()
{
    Debug.Log("🔍 Scanning for FishingRodIMU...");

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
                Debug.Log($"✅ Found {TARGET_DEVICE_NAME}! Connecting...");
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
            Debug.Log("⏳ Rescanning... Target device not found in this window.");
            BleApi.StopDeviceScan();
            Thread.Sleep(500);
        }
    }
}

    void ConnectToDevice(string deviceId)
    {
        Debug.Log($"🔗 Connecting to {TARGET_DEVICE_NAME}...");
        BleApi.ScanServices(deviceId);
        IsConnected = true;

        BleApi.Service service;
        while (BleApi.PollService(out service, true) == BleApi.ScanStatus.AVAILABLE)
        {
            Debug.Log($"📡 Service Found: {service.uuid}");

            BleApi.ScanCharacteristics(deviceId, service.uuid);
            BleApi.Characteristic characteristic;

            while (BleApi.PollCharacteristic(out characteristic, true) == BleApi.ScanStatus.AVAILABLE)
            {
                if (characteristic.uuid.ToLower().Contains(IMU_CHARACTERISTIC_UUID.ToLower()))
                {
                    Debug.Log("✅ IMU Characteristic Found!");
                    SubscribeToIMU(deviceId, service.uuid, characteristic.uuid);
                }
                else if (characteristic.uuid.ToLower().Contains(JOY_CHARACTERISTIC_UUID.ToLower()))
                {
                    Debug.Log("✅ Joystick Characteristic Found!");
                    SubscribeToJoystick(deviceId, service.uuid, characteristic.uuid);
                }
                else if (characteristic.uuid.ToLower().Contains(BRAILLE_CHARACTERISTIC_UUID.ToLower()))
                {
                    Debug.Log("✅ Braille Characteristic Found!");
                    brailleCharUUID = characteristic.uuid;
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
                Debug.Log("✅ Subscribed to IMU!");
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
                Debug.Log("✅ Subscribed to Joystick!");
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(ReadJoystickData(deviceId, serviceUuid, characteristicUuid)));
                return;
            }
        }
        Debug.LogError("❌ Failed to subscribe to Joystick after retries.");
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

                    imuRotation = new Vector3(x / 1000f, y / 1000f, z / 1000f);
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

                    float normX = (rawX - 128) / 128f;
                    float normY = (rawY - 128) / 128f;

                    Vector2 rawInput = new Vector2(normX, normY);

                    if (!calibrated && rawInput != Vector2.zero)
                    {
                        joystickCenter = rawInput;
                        calibrated = true;
                        Debug.Log($"🎯 Joystick Center Calibrated: {joystickCenter}");
                    }

                    Vector2 adjustedInput = rawInput - joystickCenter;

                    if (adjustedInput.magnitude < 0.2f)
                        adjustedInput = Vector2.zero;

                    joystickInput = adjustedInput;
                    joystickPressed = (sw == 1);

                    Debug.Log($"🎮 Joystick: ({normX:F2}, {normY:F2}), Pressed: {joystickPressed}");
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
    
    public static void SendBrailleASCII(int val1, int val2)
{
    if (!IsConnected)
    {
        Debug.LogWarning("🟡 Not connected to BLE.");
        return;
    }

    string message = $"<{val1:D3}{val2:D3}>"; // "<255255>" - 8 chars
    byte[] payload = Encoding.ASCII.GetBytes(message); // Should be exactly 8 bytes

    BleApi.BLEData bleData = new BleApi.BLEData
    {
        buf = new byte[512],
        size = (short)payload.Length,
        deviceId = targetDeviceId,
        serviceUuid = BRAILLE_SERVICE_UUID,
        characteristicUuid = brailleCharUUID
    };

    Array.Copy(payload, bleData.buf, payload.Length);

    Debug.Log($"📤 Sending ASCII Braille payload: {message}");
    Debug.Log($"📦 Raw Payload (hex): {BitConverter.ToString(payload)}");

    bool success = BleApi.SendData(in bleData, false);
    if (success)
        Debug.Log("✅ Braille ASCII data sent successfully.");
    else
    {
        BleApi.ErrorMessage error;
        BleApi.GetError(out error);
        Debug.LogError("❌ Failed to send Braille data: " + error.msg);
    }
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
