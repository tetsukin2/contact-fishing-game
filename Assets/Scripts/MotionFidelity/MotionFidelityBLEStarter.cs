using System.Collections;
using UnityEngine;

public class MotionFidelityBLEStarter : MonoBehaviour
{
    [SerializeField] private float startDelaySeconds = 1.5f;
    [SerializeField] private bool autoStartConnection = true;

    private IEnumerator Start()
    {
        if (!autoStartConnection)
        {
            yield break;
        }

        // Wait so InputDeviceManager, BLEDevice, and MainThreadDispatcher can initialize first.
        yield return new WaitForSeconds(startDelaySeconds);

        if (InputDeviceManager.Instance == null)
        {
            Debug.LogError("MotionFidelityBLEStarter: InputDeviceManager.Instance is missing.");
            yield break;
        }

        if (InputDeviceManager.Instance.BLEDevice == null)
        {
            Debug.LogError("MotionFidelityBLEStarter: BLEDevice is missing on InputDeviceManager.");
            yield break;
        }

        Debug.Log("MotionFidelityBLEStarter: Starting BLE connection attempt.");
        InputDeviceManager.Instance.BLEDevice.StartConnectionAttempt();
    }
}