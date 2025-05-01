using UnityEngine;
using Android.BLE.Commands; // Correct BLE namespace

public class BLETest : MonoBehaviour
{
    private DiscoverDevices discoverDevices;

    void Start()
    {
        Debug.Log("⚡ Testing BLE Scan...");
        discoverDevices = new DiscoverDevices(OnDeviceFound, OnScanComplete);
        discoverDevices.Start();
    }

    void OnDeviceFound(string deviceAddress, string deviceName)
    {
        Debug.Log($"🔍 FOUND DEVICE: {deviceName} at {deviceAddress}");
    }

    void OnScanComplete()
    {
        Debug.Log("🔎 BLE Scan Finished.");
    }
}
