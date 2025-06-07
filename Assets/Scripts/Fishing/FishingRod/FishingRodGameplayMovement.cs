using UnityEngine;

/// <summary>
/// Fishing Rod Object rotation based on IMU data.
/// </summary>
public class FishingRodGameplayMovement : StaticInstance<FishingRodGameplayMovement>
{
    public enum MovementMode
    {
        Idle,
        Normal,
        BaitLock,
        Free
    }

    public MovementMode CurrentMovementMode { get; private set; } = MovementMode.Normal;

    public Transform FishingRodPivot;
    public float sensitivity = 1f;
    public float smoothFactor = 0.2f;

    [Space]

    [SerializeField] private float _baitLockAngle = 30f;

    private Vector3 rodRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (!InputDeviceManager.Instance.BLEDevice.IsConnected) return;
        ReadIMUData();
    }

    void ReadIMUData()
    {
        Vector3 imuData = InputDeviceManager.Instance.IMUInput.Rotation;

        //Debug.Log(imuData);

        if (CurrentMovementMode == MovementMode.Normal)
        {
            rodRotation.y = Mathf.SmoothDamp(rodRotation.y, Mathf.Lerp(-imuData.z, 0f, Mathf.Abs(imuData.y)) * sensitivity, ref velocity.y, smoothFactor);
            rodRotation.y = Mathf.Clamp(rodRotation.y, -30f, 30f);

            FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y, 0f, 0f);
        }
        else if (CurrentMovementMode == MovementMode.BaitLock)
        {
            FishingRodPivot.localRotation = Quaternion.Euler(_baitLockAngle, 0f, 0f);
        }
    }

    public void SetMovementMode(MovementMode movementMode)
    {
        Debug.Log("updating fishing rod movement mode");
        CurrentMovementMode = movementMode;
    }
}

