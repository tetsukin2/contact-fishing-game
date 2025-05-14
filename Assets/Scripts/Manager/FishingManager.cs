using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// FishingManager with class-based state machine
public class FishingManager : MonoBehaviour
{
    public enum FishingStateName
    {
        Idle,
        BaitPreparation,
        Casting,
        WaitingForBite,
        Reeling,
        FishInspection
    }

    [SerializeField] private FishTargeting _fishTargeting;

    [Space]
    [Header("Fishing Rod")]
    public float RotationTriggerThreshold = 15f;  // Rotation threshold in degrees
    public float BobberSensitivity = 1f;
    [SerializeField] private FishingBobber _fishingBobber;
    [SerializeField] private Transform _bobberLandTransform;
    [SerializeField] private InputDeviceRotationHelper _inputHelper;
    [SerializeField] private FishingRodMovement _rodMovement;
    public float RotateUpAngle = 30f;
    public float RotateDownAngle = -30f; // Y rod rotation thresholds

    [Space]
    [Header("UI")]
    [SerializeField] private FishingStateLabelPanel _stateLabelPanel; // Reference to the state label panel

    [Space]
    [Header("Bait Preparation")]
    public int BaitPreparationSteps = 1;
    public string BaitPrepPromptRightName;
    public string BaitPrepPromptLeftName;
    public float RollRightAngle = -0.8f;
    public float RollLeftAngle = 0.8f;

    [Space]
    [Header("Casting")]
    public float CastForce = 10f;  // Adjust casting strength
    public int CastSteps = 1;
    public float CastDuration = 1f; // From bobber release to landing
    public float CastHeight = 2f; // Additional height for the arc from bobber release point
    public string CastForwardPromptName;
    public string CastBackPromptName;
    public string CastSelectPromptName;

    [Space]
    [Header("WaitingForBite")]
    //public float FishBiteWaitDuration = 2f;

    [Space]
    [Header("Reeling")]
    public float ReelTotalProgress = 20;
    public float ReelProgressAmount = 5;
    public string ReelForwardPromptName;
    public string ReelBackPromptName;
    public string ReelClockwisePromptName;
    public float ReelForce = 1f; // Force applied to the bobber upward
    public float ReelDecayRate = 0.3f;
    [SerializeField] private GUIContainer _reelGUI;
    [SerializeField] private Slider _reelProgressSlider;
    public List<ReelingState.ReelActionName> ReelActionSequence; //Sequence of actions to follow

    [Space]
    [Header("FishData Inspection")]
    //public float SideRotateUpAngle = 30f;
    //public float SideRotateDownAngle = -30f; // Y rod rotation thresholds
    public string InspectReadyPromptName;
    public string InspectPromptName;
    public string ReleaseReadyPromptName;
    public string ReleasePromptName;
    [SerializeField] private FishInspectionPanel _fishInspectionGUI;
    [SerializeField] private GameObject _hookedFish; // show and hide in inspection

    private FishingState _currentState;
    private UnityEvent _bobberHitWater = new();
    private FishData _caughtFish;

    // Phase Accessors
    public BaitPreparationState BaitPreparationState { get; private set; }
    public CastingState CastingState { get; private set; }
    public WaitingForBiteState WaitingForBiteState { get; private set; }
    public ReelingState ReelingState { get; private set; }
    public FishInspectionState FishInspectionState { get; private set; }

    // Some properties, mostly for fish states to access
    public FishTargeting Targeting => _fishTargeting;
    public FishingBobber FishingBobber => _fishingBobber;
    public InputDeviceRotationHelper InputHelper => _inputHelper;
    public FishingRodMovement RodMovement => _rodMovement;
    public GameObject HookedFish => _hookedFish;
    public FishingStateLabelPanel StateLabelPanel => _stateLabelPanel;
    public UnityEvent BobberHitWater => _bobberHitWater;

    private void Awake()
    {
        // Setup states
        BaitPreparationState = new(this);
        CastingState = new(this);
        WaitingForBiteState = new(this);
        ReelingState = new(this);
        FishInspectionState = new(this);
    }

    void Start()
    {
        //CastLine();
        CastingState.Setup();
        ReelingState.Setup();

        _fishingBobber.Setup(this);

        StopReel(); // Hide the reel GUI at the start
        _hookedFish.SetActive(false);

        // Only start bait prep after gameplay actually starts
        GameManager.Instance.GameStateEntered.AddListener((state) =>
        {
            if (state == GameManager.Instance.PlayingState)
            {
                TransitionToState(BaitPreparationState);
            }
        });
        
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.PlayingState) return;
        _currentState?.Update();
    }

    // Transition to a new state
    public void TransitionToState(FishingState newState)
    {
        _currentState?.Exit(); // Exit the current state
        _currentState = newState; // Set the new state
        _currentState?.Enter(); // Enter the new state
    }

    public void CastLine()
    {
        InputHelper.ClearRotationHistory();
        _fishingBobber.OnCast();

        // Start the parabolic trajectory coroutine
        StartCoroutine(MoveBobberToLanding(_fishTargeting.Selection.transform.position));

        Debug.Log("Casting Fishing Line!");
    }

    private IEnumerator MoveBobberToLanding(Vector3 landingPosition)
    {
        // Get the starting position of the bobber
        Vector3 startPosition = _fishingBobber.transform.position;

        // Calculate the peak height of the trajectory
        float peakHeight = Mathf.Max(startPosition.y, landingPosition.y) + CastHeight;
        //float duration = Mathf.Clamp(castForce / 10f, 1f, 3f); // Adjust scal

        float elapsedTime = 0f;

        while (elapsedTime < CastDuration)
        {
            // Calculate the normalized time (0 to 1)
            float t = elapsedTime / CastDuration;

            // Interpolate the horizontal position (x and z)
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, landingPosition, t);

            // Calculate the vertical position (y) using a parabolic equation
            float verticalPosition = Mathf.Lerp(startPosition.y, peakHeight, t) * (1 - t) + Mathf.Lerp(peakHeight, landingPosition.y, t) * t;

            // Update the bobber's position
            _fishingBobber.transform.position = new Vector3(horizontalPosition.x, verticalPosition, horizontalPosition.z);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the bobber ends exactly at the landing position
        _fishingBobber.transform.position = landingPosition;
        _bobberHitWater.Invoke();

        Debug.Log("Bobber has landed!");
    }

    public void ReelIn()
    {
        _fishingBobber.OnReel(ReelForce);

        _caughtFish = FishLootTable.Instance.GetFishFromTable();

        Debug.Log("Reeling In!");
    }

    public void StartReel()
    {
        _reelGUI.Show(true); // Show the reel GUI
        SetupReelBar(); // Setup the reel progress bar
    }

    public void StopReel()
    {
        _reelGUI.Show(false); // Hide the reel GUI
        _reelProgressSlider.value = 0f; // Reset the slider value
    }

    private void SetupReelBar()
    {
        _reelProgressSlider.maxValue = ReelTotalProgress;
        _reelProgressSlider.value = 0f; // Initialize the slider value
    }

    // Set the progress of the reel in slider
    public void SetReelProgress(float value)
    {
        _reelProgressSlider.value = Mathf.Min(value, _reelProgressSlider.maxValue);
    }

    public void OnFishInspection()
    {
        GameManager.Instance.CurrentGameData.AddFish(_caughtFish.FishID);
        ShowFishInspection();
        Debug.Log($"Has {_caughtFish.FishID} been discovered: {GameManager.Instance.CurrentGameData.HasDiscoveredFish(_caughtFish.FishID)}");
        HookedFish.SetActive(false);
    }

    public void ShowFishInspection()
    {
        GameManager gameManager = GameManager.Instance;
        _fishInspectionGUI.Show(true);
        _fishInspectionGUI.ShowFish(_caughtFish);
        _fishInspectionGUI.ShowDiscoveredBanner(gameManager.CurrentGameData.HasDiscoveredFish(_caughtFish.FishID));
    }

    public void HideFishInspection()
    {
        _fishInspectionGUI.Show(false);
    }

    
}