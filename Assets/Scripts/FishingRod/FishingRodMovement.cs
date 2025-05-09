using UnityEngine;

/// <summary>
/// Fishing Rod Object rotation based on IMU data.
/// </summary>
public class FishingRodMovement : MonoBehaviour
{
    public enum MovementMode
    {
        Idle,
        Normal,
        Menu,
        BaitLock,
        Free
    }

    public MovementMode CurrentMovementMode = MovementMode.Normal;

    public Transform FishingRodPivot;
    public float sensitivity = 1f;
    public float smoothFactor = 0.2f;

    [Space]

    [SerializeField] private float _menuOffsetRotation = -30f;
    [SerializeField] private float _baitLockAngle = 30f;

    private Vector3 rodRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private Vector3 _defaultRotation;
    
    public float MenuRotationMax => 0.33f;
    public float MenuRotationMin => -0.33f;

    private void Start()
    {
        _defaultRotation = FishingRodPivot.localRotation.eulerAngles;
        GameManager.Instance.GameStateUpdated.AddListener(OnGameStateUpdated);
    }

    void Update()
    {
        if (!InputDeviceManager.IsConnected) return;
        ReadIMUData();
    }

    void ReadIMUData()
    {
        Vector3 imuData = InputDeviceManager.IMURotation;

        //Debug.Log(imuData);

        if (CurrentMovementMode == MovementMode.Normal)
        {
            //rodRotation.x = Mathf.SmoothDamp(rodRotation.x, -imuData.x * sensitivity, ref velocity.x, smoothFactor);
            rodRotation.y = Mathf.SmoothDamp(rodRotation.y, Mathf.Lerp(-imuData.z, 0f, Mathf.Abs(imuData.y)) * sensitivity, ref velocity.y, smoothFactor);
            //rodRotation.z = Mathf.SmoothDamp(rodRotation.z, imuData.z * sensitivity, ref velocity.z, smoothFactor);

            //rodRotation.x = Mathf.Clamp(rodRotation.x, -30f, 30f);
            rodRotation.y = Mathf.Clamp(rodRotation.y, -30f, 30f);
            //rodRotation.z = Mathf.Clamp(rodRotation.z, -30f, 30f);

            FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y, 0f, -rodRotation.x);
        }
        else if (CurrentMovementMode == MovementMode.BaitLock)
        {
            FishingRodPivot.localRotation = Quaternion.Euler(_baitLockAngle, 0f, 0f);
        }
        //else if (CurrentMovementMode == MovementMode.Free)
        //{
        //    rodRotation.x = Mathf.SmoothDamp(rodRotation.x, -imuData.x * sensitivity, ref velocity.x, smoothFactor);
        //    rodRotation.y = Mathf.SmoothDamp(rodRotation.y, imuData.y * sensitivity, ref velocity.y, smoothFactor);
        //    rodRotation.z = Mathf.SmoothDamp(rodRotation.z, imuData.z * sensitivity, ref velocity.z, smoothFactor);
        //    rodRotation.x = Mathf.Clamp(rodRotation.x, -60f, 60f);
        //    rodRotation.y = Mathf.Clamp(rodRotation.y, -60f, 60f);
        //    rodRotation.z = Mathf.Clamp(rodRotation.z, -60f, 60f);
        //    FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y + _menuOffsetRotation, 0, -rodRotation.x);
        //}
        else if (CurrentMovementMode == MovementMode.Menu)
        {
            rodRotation.y = Mathf.SmoothDamp(rodRotation.y, Mathf.Lerp(-imuData.z, 0f, Mathf.Abs(imuData.y)) * sensitivity, ref velocity.y, smoothFactor);
            rodRotation.y = Mathf.Clamp(rodRotation.y, -30f, 30f);

            FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y + _menuOffsetRotation, 0, -rodRotation.x);
        }
        //else if (CurrentMovementMode == MovementMode.Idle)
        //{
        //    rodRotation.x = Mathf.SmoothDamp(rodRotation.x, _defaultRotation.x * sensitivity, ref velocity.x, smoothFactor);
        //    rodRotation.y = Mathf.SmoothDamp(rodRotation.y, _defaultRotation.y * sensitivity, ref velocity.y, smoothFactor);
        //    rodRotation.z = Mathf.SmoothDamp(rodRotation.z, _defaultRotation.z * sensitivity, ref velocity.z, smoothFactor);

        //    FishingRodPivot.localRotation = Quaternion.Euler(-rodRotation.y, 0f, -rodRotation.x);
        //}

    }

    private void OnGameStateUpdated(GameState newState)
    {
        Debug.Log("updating fishing rod movement mode");
        if (newState == GameManager.Instance.PlayingState)
        {
            CurrentMovementMode = MovementMode.Normal;
        }
        else if (newState == GameManager.Instance.EncyclopediaState)
        {
            CurrentMovementMode = MovementMode.BaitLock; // setting to Idle causes rod to be wonky rest of the time idk
        }
        else //if (GameManager.Instance.CurrentGameState == GameStateName.GameStart)
        {
            CurrentMovementMode = MovementMode.Menu;
        }
    }
}

