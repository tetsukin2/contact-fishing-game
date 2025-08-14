using TMPro;
using UnityEngine;

/// <summary>
/// Debug overlay to show IMU rotation.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _imuRotation;
    [SerializeField] private TextMeshProUGUI _gameTime;
    [SerializeField] private TextMeshProUGUI _lastDataReceived;
    [SerializeField] private TextMeshProUGUI _lastHealthCheck;

    private void Update()
    {
        _imuRotation.text = $"IMU Rotation: {InputDeviceManager.Instance.IMUInput.Rotation}";
        _gameTime.SetText($"Game Time: {Time.time:F2}");
        _lastDataReceived.SetText($"| Lst Rcv: {InputDeviceManager.Instance.BLEDevice.LastDeviceUpdateTime:F2}");
        _lastHealthCheck.SetText($"| Lst Hlth Chk: {InputDeviceManager.Instance.BLEDevice.LastHealthCheckTime:F2}");
    }
}
