using System.Text;
using UnityEngine;

public class LatencyCommandOutput : MonoBehaviour
{
    public void SendLatencyCommand(int latencyId)
    {
        if (!InputDeviceManager.Instance.BLEDevice.IsConnected)
        {
            Debug.LogWarning("LatencyCommandOutput: BLE is not connected.");
            return;
        }

        string message = $"<LAT{latencyId:D3}>";
        byte[] payload = Encoding.ASCII.GetBytes(message);

        BleApi.BLEData bleData = new()
        {
            buf = new byte[512],
            size = (short)payload.Length,
            deviceId = InputDeviceManager.Instance.BLEDevice.ConnectedDeviceID,
            serviceUuid = BLEDevice.BRAILLE_SERVICE_UUID,
            characteristicUuid = BLEDevice.BRAILLE_CHARACTERISTIC_UUID
        };

        System.Array.Copy(payload, bleData.buf, payload.Length);

        bool success = BleApi.SendData(in bleData, false);

        if (!success)
        {
            BleApi.GetError(out BleApi.ErrorMessage error);
            Debug.LogError("Failed to send latency command: " + error.msg);
        }
        else
        {
            Debug.Log($"Latency command sent: {message}");
        }
    }
}