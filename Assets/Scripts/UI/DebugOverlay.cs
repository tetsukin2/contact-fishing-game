using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;

    private void Update()
    {
        if (!InputDeviceManager.IsConnected) return;
        debugText.text = $"IMU Rotation: {InputDeviceManager.IMURotation}";
    }
}
