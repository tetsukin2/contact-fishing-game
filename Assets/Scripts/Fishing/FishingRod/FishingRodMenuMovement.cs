using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingRodMenuMovement : StaticInstance<FishingRodMenuMovement>
{
    [SerializeField] private float _menuOffsetRotation = -30f;

    public Transform FishingRodPivot;
    public float sensitivity = 1f;
    public float smoothFactor = 0.2f;

    public float MenuRotationMax => 0.33f;
    public float MenuRotationMin => -0.33f;

    private Vector3 rodRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (!InputDeviceManager.Instance.BLEDevice.IsConnected) return;

        Vector3 imuData = InputDeviceManager.Instance.IMUInput.Rotation;

        rodRotation.y = Mathf.SmoothDamp(rodRotation.y, imuData.x * sensitivity, ref velocity.y, smoothFactor, Mathf.Infinity, Time.unscaledDeltaTime);
        rodRotation.y = Mathf.Clamp(rodRotation.y, -30f, 30f);

        FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y + _menuOffsetRotation, 0, -rodRotation.x);
    }
}
