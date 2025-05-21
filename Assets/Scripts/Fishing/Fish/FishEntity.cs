using UnityEngine;
using UnityEngine.Events;

public class FishEntity : MonoBehaviour
{
    [SerializeField] private GameObject _highlight;

    [Space]
    [Header("Swim Behavior")]
    public Transform Fish; // Reference to the object that moves and turns
    public FishData Data;
    public float SwimMaxRadius = 3f;
    public float SwimDepth = 2f;
    public float moveSpeed = 1.5f;
    public float maxTurnSpeed = 90f;   // max degrees/sec
    public float turnAcceleration = 180f; // how fast it ramps up/down

    [Space]
    [Header("Struggle")]
    public float struggleAngle = 20f;     // degrees
    public float struggleSpeed = 6f;

    /// <summary>
    /// Event to trigger when the fish reaches the lure location
    /// </summary>
    public UnityEvent ReachedLureLocation { get; private set; } = new();

    private float _struggleTimer = 0f;
    private bool _isBeingLured = false; // Flag to check if the fish is being lured
    private bool _hasReachedLureLocation = false;
    private float currentTurnSpeed = 0f;
    private Vector3 targetPoint;

    void Start()
    {
        _highlight.SetActive(false);
        PickNewTarget();
    }

    public void SetSelected(bool selected)
    {
        _highlight.SetActive(selected);
    }

    void Update()
    {
        if (Fish == null) return;

        if (!_isBeingLured) DoFishRandomSwim();
        else if (_isBeingLured) DoFishLuredBehaviour();
    }

    public void LureFish()
    {
        _isBeingLured = true;
    }

    public void ReleaseFish()
    {
        _hasReachedLureLocation = false;
        _isBeingLured = false;
        _struggleTimer = 0f; // Reset struggle timer
    }

    private void DoFishRandomSwim()
    {
        Vector3 toTarget = targetPoint - Fish.position;
        Vector3 flatToTarget = new(toTarget.x, 0f, toTarget.z);

        // Check if close to target
        if (flatToTarget.magnitude < 0.2f)
        {
            PickNewTarget();
            return;
        }

        // Turn gradually toward target
        // Calculate angle difference
        float angleToTarget = Vector3.SignedAngle(Fish.forward, flatToTarget.normalized, Vector3.up);
        float absAngle = Mathf.Abs(angleToTarget);

        // Easing: accelerate when misaligned, decelerate when aligned
        float desiredTurnSpeed = Mathf.Lerp(0f, maxTurnSpeed, Mathf.InverseLerp(0f, 90f, absAngle));
        currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, desiredTurnSpeed, turnAcceleration * Time.deltaTime);

        // Turn toward target
        float turnStep = Mathf.Sign(angleToTarget) * currentTurnSpeed * Time.deltaTime;
        Fish.Rotate(Vector3.up, turnStep);

        // Move forward along local forward
        Vector3 nextPosition = Fish.position + moveSpeed * Time.deltaTime * Fish.forward;
        nextPosition.y = transform.position.y - SwimDepth; // force fixed swim depth
        Fish.position = nextPosition;
    }

    private void DoFishLuredBehaviour()
    {
        Vector3 toTarget = transform.position - Fish.position;
        if (Vector3.Distance(Fish.position, transform.position) < 0.2f)
        {
            if (!_hasReachedLureLocation)
            {
                Debug.Log("Fish reached lure location");
                _hasReachedLureLocation = true;
                Fish.position = transform.position; // snap to lure position
                ReachedLureLocation.Invoke(); // trigger event
            }
            DoFishStruggle();
        }

        // Calculate angle in XZ plane for rotation
        Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
        float angleToTarget = Vector3.SignedAngle(Fish.forward, flatToTarget.normalized, Vector3.up);
        float absAngle = Mathf.Abs(angleToTarget);

        // Easing: accelerate when misaligned, decelerate when aligned
        float desiredTurnSpeed = Mathf.Lerp(0f, maxTurnSpeed, Mathf.InverseLerp(0f, 90f, absAngle));
        currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, desiredTurnSpeed, turnAcceleration * Time.deltaTime);

        // Turn gradually toward the XZ direction (yaw only)
        float turnStep = Mathf.Sign(angleToTarget) * currentTurnSpeed * Time.deltaTime;
        Fish.Rotate(Vector3.up, turnStep);

        // Move forward in 3D space (Y included)
        Vector3 nextPosition = Fish.position + Fish.forward * moveSpeed * Time.deltaTime;

        // Allow Y movement toward lure
        nextPosition.y = Mathf.MoveTowards(Fish.position.y, transform.position.y, moveSpeed * Time.deltaTime);

        Fish.position = nextPosition;
    }

    private void DoFishStruggle()
    {
        _struggleTimer += Time.deltaTime;

        // Wiggle side-to-side (rotation around Y)
        float oscillationAngle = Mathf.Sin(_struggleTimer * Mathf.PI * struggleSpeed) * struggleAngle;
        Quaternion baseRotation = Quaternion.LookRotation((transform.position - Fish.position).normalized);
        Fish.rotation = baseRotation * Quaternion.Euler(0f, oscillationAngle, 0f);

        // Tug back and forth (position oscillation along forward)
        float tugOffset = Mathf.Sin(_struggleTimer * Mathf.PI * struggleSpeed) * 0.05f; // ~5cm tug
        Vector3 tugVector = Fish.forward * tugOffset;

        Vector3 newPosition = transform.position - Fish.forward * 0.2f + tugVector;
        newPosition.y = Mathf.Lerp(Fish.position.y, transform.position.y, 0.1f); // settle Y gradually
        Fish.position = newPosition;
    }

    void PickNewTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * SwimMaxRadius;
        targetPoint = transform.position + new Vector3(randomOffset.x, -SwimDepth, randomOffset.y);
        //Debug.Log("picked new point at " + targetPoint);
    }
}
