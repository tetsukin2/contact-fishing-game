using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Android; // Required for runtime permissions
using Android.BLE.Commands; // Import the correct BLE namespace

public class BLEScanner : MonoBehaviour
{
    private string targetDeviceName = "FishingRodIMU";  // Change this to match your Arduino BLE name
    private DiscoverDevices discoverDevices;  // BLE scanning class
    private List<string> foundDevices = new(); // Store discovered devices

    void Start()
    {
        RequestBLEPermissions();

        Debug.Log("Initializing BLE Scan...");
        discoverDevices = new DiscoverDevices(OnDeviceDiscovered, OnScanFinished);

        Debug.Log("DiscoverDevices.Start() was called!");
        discoverDevices.Start(); // Start BLE scanning

        Debug.Log("Scanning for BLE devices... Waiting for results...");
    }

    void RequestBLEPermissions()
    {
        Debug.Log("Checking BLE Permissions...");

        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
        {
            Debug.Log("Requesting BLUETOOTH_SCAN permission...");
            Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
        }

        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {
            Debug.Log("Requesting BLUETOOTH_CONNECT permission...");
            Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
        }
    }

    void OnDeviceDiscovered(string deviceAddress, string deviceName)
    {
        Debug.Log($"BLE Device Discovered: {deviceName} at {deviceAddress}");
        foundDevices.Add(deviceAddress); // Store discovered device

        // Log devices without a name
        if (string.IsNullOrEmpty(deviceName))
        {
            Debug.Log("Found a device, but it has no name!");
        }

        // Check if the found device matches our target (FishingRodIMU)
        if (!string.IsNullOrEmpty(deviceName) && deviceName.Contains(targetDeviceName))
        {
            Debug.Log($"Found Target BLE Device: {deviceName}, Connecting...");
            ConnectToDevice(deviceAddress);
            discoverDevices.End(); // Stop scanning after finding the target
        }
    }

    void OnScanFinished()
    {
        Debug.Log("BLE Scan Finished.");
        if (foundDevices.Count == 0)
        {
            Debug.Log("No BLE devices were found. Make sure the Arduino is powered on and advertising.");
        }
        else
        {
            Debug.Log($"Total Devices Found: {foundDevices.Count}");
        }
    }

    void ConnectToDevice(string deviceAddress)
    {
        Debug.Log($"Attempting to connect to {targetDeviceName}...");
        ConnectToDevice connectCommand = new ConnectToDevice(deviceAddress);
        connectCommand.Start(); // Start connection process
    }
}
