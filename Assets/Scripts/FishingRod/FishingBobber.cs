using Unity.VisualScripting;
using UnityEngine;

public class FishingBobber : MonoBehaviour
{
    public Transform BobberPivot;
    public float smoothFactor = 0.2f;

    [Space]

    [SerializeField] private bool _controllable = false;
    [SerializeField] private GameObject _unattachedLure;
    [SerializeField] private GameObject _attachedLure;

    private Vector3 bobberRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private Vector3 _defaultPosition;

    private FishingManager _fishingManager;

    private Rigidbody _bobberRigidbody;

    private void Awake()
    {
        _bobberRigidbody = GetComponent<Rigidbody>();
        _defaultPosition = transform.position;
    }

    public void Setup(FishingManager fishingManager)
    {
        _fishingManager = fishingManager;
    }

    private void Start()
    {
        
        // Always hide lures outside of main gameplay
        GameManager.Instance.GameStateEntered.AddListener((state) =>
        {
            if (state != GameManager.Instance.PlayingState) HideLures();            
        });
        HideLures();
    }

    void Update()
    {
        if (!InputDeviceManager.IsConnected) return;
        ProcessRotation();
    }

    void ProcessRotation()
    {
        if (!_controllable || !_fishingManager) return;

        Vector3 imuData = InputDeviceManager.IMURotation;

        bobberRotation.y = Mathf.SmoothDamp(bobberRotation.y, -imuData.y * _fishingManager.BobberSensitivity, ref velocity.y, smoothFactor);
        bobberRotation.y = Mathf.Clamp(bobberRotation.y, -90f, 90f);

        BobberPivot.localRotation = Quaternion.Euler(-bobberRotation.y, 0, -bobberRotation.x);
    }

    public void SetControllable(bool controllable)
    {
        _bobberRigidbody.isKinematic = controllable;

        // Reset position on toggle
        if (_controllable != controllable) 
        {
            BobberPivot.localRotation = Quaternion.Euler(0f, 0f, 0f);
            //_bobberRigidbody.Move(_defaultPosition, Quaternion.Euler(0f, 0f, 0f));
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            transform.position = _defaultPosition;
        }
        _controllable = controllable;
    }

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

    public void OnReel(float reelForce)
    {
        _bobberRigidbody.isKinematic = false;
        _bobberRigidbody.AddForce(Vector3.up * reelForce, ForceMode.Impulse);
    }

    public void OnCast()
    {
        _bobberRigidbody.isKinematic = true;
    }
}
