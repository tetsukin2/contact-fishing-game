using TMPro;
using UnityEngine;

/// <summary>
/// Debug overlay to show IMU rotation.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;

    private void Update()
    {
        if (!InputDeviceManager.IsConnected) return;
        debugText.text = $"IMU Rotation: {InputDeviceManager.IMURotation}";
    }
}
