using UnityEngine;

/// <summary>
/// Fishing Rod Object rotation based on IMU data.
/// </summary>
public class FishingRodMovement : MonoBehaviour
{
    public enum MovementMode
    {
        Normal,
        Menu
    }

    [SerializeField] private MovementMode _currentMovementMode = MovementMode.Normal;

    public Transform FishingRodPivot;
    public float sensitivity = 1f;
    public float smoothFactor = 0.2f;

    [Space]

    [SerializeField] private float _menuOffsetRotation = -30f;

    private Vector3 rodRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero; 
    

    void Update()
    {
        if (!InputDeviceManager.IsConnected) return;
        ReadIMUData();
    }

    void ReadIMUData()
    {
        Vector3 imuData = InputDeviceManager.IMURotation;

        //Debug.Log(imuData);

        if (_currentMovementMode == MovementMode.Normal)
        {
            rodRotation.x = Mathf.SmoothDamp(rodRotation.x, -imuData.x * sensitivity, ref velocity.x, smoothFactor);
            rodRotation.y = Mathf.SmoothDamp(rodRotation.y, imuData.y * sensitivity, ref velocity.y, smoothFactor);
            rodRotation.z = Mathf.SmoothDamp(rodRotation.z, imuData.z * sensitivity, ref velocity.z, smoothFactor);

            rodRotation.x = Mathf.Clamp(rodRotation.x, -30f, 30f);
            rodRotation.y = Mathf.Clamp(rodRotation.y, -30f, 30f);
            rodRotation.z = Mathf.Clamp(rodRotation.z, -30f, 30f);

            FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y, 0, -rodRotation.x);
        }
        else if (_currentMovementMode == MovementMode.Menu)
        {
            rodRotation.y = Mathf.SmoothDamp(rodRotation.y, imuData.y * sensitivity, ref velocity.y, smoothFactor);
            rodRotation.y = Mathf.Clamp(rodRotation.y, -30f, 30f);

            FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y + _menuOffsetRotation, 0, -rodRotation.x);
        }

        
    }
}

