using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FishingBobber : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float smoothFactor = 0.2f; // For rotation animation smoothness

    [Space]
    [Header("References")]
    [SerializeField] private Transform _bobberPivot;
    // We're not really moving the lure, we just have one already on the hook and one flying
    [SerializeField] private GameObject _unattachedLure;
    [SerializeField] private GameObject _attachedLure;
    private Rigidbody _bobberRigidbody;

    private bool _controllable = false;

    private Vector3 bobberRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 _defaultPosition;

    /// <summary>
    /// Invoked when the bobber hits the water
    /// </summary>
    public UnityEvent BobberHitWater { get; private set; } = new();

    private void Awake()
    {
        _bobberRigidbody = GetComponent<Rigidbody>();
        _defaultPosition = transform.position;
    }

    private void Start()
    {
        // Always hide lures outside of main gameplay
        LevelManager.Instance.GameStateEntered.AddListener((state) =>
        {
            if (state != LevelManager.Instance.PlayingState) HideLures();            
        });
        HideLures();

        FishingManager.Instance.CastingState.LineCast.AddListener(OnCast);
    }

    void Update()
    {
        if (InputDeviceManager.Instance.BLEDevice.IsConnected) ProcessRotation();
    }

    /// <summary>
    /// Handles the rotation of the bobber based on IMU data.
    /// </summary>
    void ProcessRotation()
    {
        // Pause check is stopgap
        if (!_controllable || LevelManager.Instance.IsGamePaused) return;

        Vector3 imuData = InputDeviceManager.Instance.IMUInput.Rotation;

        bobberRotation.y = Mathf.SmoothDamp(bobberRotation.y, -imuData.y * FishingManager.Instance.BobberSensitivity, ref velocity.y, smoothFactor);
        bobberRotation.y = Mathf.Clamp(bobberRotation.y, -90f, 90f);

        _bobberPivot.localRotation = Quaternion.Euler(bobberRotation.y, 0f, 0f);
    }

    /// <summary>
    /// Whether bobber is controlled by IMU data.
    /// </summary>
    /// <param name="controllable"></param>
    public void SetControllable(bool controllable)
    {
        _bobberRigidbody.isKinematic = controllable;

        // Reset position on toggle
        if (_controllable != controllable) 
        {
            _bobberPivot.localRotation = Quaternion.Euler(0f, 0f, 0f);
            transform.SetPositionAndRotation(_defaultPosition, Quaternion.Euler(0f, 0f, 0f));
        }
        _controllable = controllable;
    }

    #region Lure Visibility
    public void SetupLureAttach()
    {
        _unattachedLure.SetActive(true);
        _attachedLure.SetActive(false);
    }

    public void OnAttachLure()
    {
        _unattachedLure.SetActive(false);
        _attachedLure.SetActive(true);
    }

    public void HideLures()
    {
        _unattachedLure.SetActive(false);
        _attachedLure.SetActive(false);
    }
    #endregion

    public void OnReel(float reelForce)
    {
        _bobberRigidbody.isKinematic = false;
        _bobberRigidbody.AddForce(Vector3.up * reelForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Handle bobber casting
    /// </summary>
    private void OnCast()
    {
        _bobberRigidbody.isKinematic = true; // do not let physics touch this
        StartCoroutine(LaunchBobberToTarget(FishingManager.Instance.Targeting.Selection.transform.position));
    }

    // Programatically move the bobber from current bobber position to the target position
    private IEnumerator LaunchBobberToTarget(Vector3 landingPosition)
    {
        var fishingManager = FishingManager.Instance;

        // Get the starting position of the bobber
        Vector3 startPosition = transform.position;

        // Calculate the peak height of the trajectory
        float peakHeight = Mathf.Max(startPosition.y, landingPosition.y) + fishingManager.CastHeight;

        float elapsedTime = 0f;

        while (elapsedTime < fishingManager.CastDuration)
        {
            // Calculate the normalized time (0 to 1)
            float t = elapsedTime / fishingManager.CastDuration;

            // Interpolate the horizontal position (x and z)
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, landingPosition, t);

            // Calculate the vertical position (y) using a parabolic equation
            float verticalPosition = Mathf.Lerp(startPosition.y, peakHeight, t) * (1 - t) + Mathf.Lerp(peakHeight, landingPosition.y, t) * t;

            // Update the bobber's position
            transform.position = new Vector3(horizontalPosition.x, verticalPosition, horizontalPosition.z);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Snap bobber to landing position at end
        transform.position = landingPosition;
        BobberHitWater.Invoke();

        Debug.Log("Bobber has landed!");
    }
}
