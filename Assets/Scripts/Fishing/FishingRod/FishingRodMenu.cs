using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingRodMenu : StaticInstance<FishingRodMenu>
{
    [SerializeField] private float _menuOffsetRotation = 30f;

    public Transform FishingRodPivot;
    public float sensitivity = 90f;
    public float smoothFactor = 0.1f;

    public float MenuRotationMax => 0.33f;
    public float MenuRotationMin => -0.33f;

    private Vector3 rodRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (!InputDeviceManager.Instance.BLEDevice.IsConnected) return;

        Vector3 imuData = InputDeviceManager.Instance.IMUInput.Rotation;

        rodRotation.x = Mathf.SmoothDamp(rodRotation.x, -imuData.x * sensitivity, ref velocity.x, smoothFactor, Mathf.Infinity, Time.unscaledDeltaTime);
        rodRotation.x = Mathf.Clamp(rodRotation.x, -30f, 30f);

        FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.x + _menuOffsetRotation, 0f, 0f);
    }
}
