using System.Text;
using UnityEngine;

/// <summary>
/// Handles the Braille output to the Braille display.
/// </summary>
public class BrailleOutput : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool showBrailleData = false;

    /// <summary>
    /// Resets the Braille display to a blank state (all cells off).
    /// </summary>
    public void ResetBraille()
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
    public void SendBrailleASCII(int t0, int t1, int i0, int i1)
    {
        if (!InputDeviceManager.Instance.BLEDevice.IsConnected)
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
            deviceId = InputDeviceManager.Instance.BLEDevice.ConnectedDeviceID,
            serviceUuid = BLEDevice.BRAILLE_SERVICE_UUID,
            characteristicUuid = BLEDevice.BRAILLE_CHARACTERISTIC_UUID
        };

        System.Array.Copy(payload, bleData.buf, payload.Length);

        if (showBrailleData) Debug.Log($"Sending ASCII Braille payload: {message} (Raw Hex: {System.BitConverter.ToString(payload)})");

        bool success = BleApi.SendData(in bleData, false);
        if (success)
            Debug.Log("Braille ASCII data sent successfully.");
        else
        {
            BleApi.GetError(out BleApi.ErrorMessage error);
            Debug.LogError("Failed to send Braille data: " + error.msg);
        }
    }
}
