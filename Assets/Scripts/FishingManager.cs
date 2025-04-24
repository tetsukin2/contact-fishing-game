using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// FishingManager with class-based state machine
public class FishingManager : MonoBehaviour
{
    [Header("General")]
    public float RotationTriggerThreshold = 15f;  // Rotation threshold in degrees

    [Space]

    [Header("Casting")]
    public float CastForce = 10f;  // Adjust casting strength

    [Space]

    [Header("WaitingForBite")]
    public float FishBiteWaitDuration = 2f;

    [Space]
    [Header("Reeling")]
    public int ReelTotalProgress = 20;
    public int ReelProgressAmount = 5;
    public GameObject _reelGUI;
    [SerializeField] private Slider _reelProgressSlider;

    public BaitPreparationState BaitPreparationState { get; private set; }
    public CastingState CastingState { get; private set; }
    public WaitingForBiteState WaitingForBiteState { get; private set; }
    public ReelingState ReelingState { get; private set; }

    [SerializeField] private FishingBobber _fishingBobber; // Reference to the FishingBobber script
    [SerializeField] private FishingRodInputHelper _inputHelper;

    private FishingState _currentState;

    public FishingBobber FishingBobber => _fishingBobber;
    public FishingRodInputHelper InputHelper => _inputHelper;

    private void Awake()
    {
        // Setup states
        BaitPreparationState = new(this);
        CastingState = new(this);
        WaitingForBiteState = new(this);
        ReelingState = new(this);
    }

    void Start()
    {
        CastingState.Setup();

        //setup reeling progress bar
        _reelProgressSlider.wholeNumbers = true;
        _reelProgressSlider.maxValue = ReelTotalProgress;
        _reelProgressSlider.value = 0f; // Initialize the slider value

        TransitionToState(CastingState);
    }

    void Update()
    {
        // Delegate update logic to the current state
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
        // Apply velocity to the hook based on the rod tip's forward direction
        _fishingBobber.Cast(CastForce * Mathf.Abs(InputHelper.LastMeasuredAngle), 0.1f);
        Debug.Log("Casting Fishing Line!");
    }

    public void ReelIn()
    {
        _fishingBobber.Reel();
        Debug.Log("Reeling In!");
    }

    public void ProgressReel()
    {
        // Update the slider value
        _reelProgressSlider.value = Mathf.Min(_reelProgressSlider.value + ReelProgressAmount, _reelProgressSlider.maxValue);
        // Check if the reel progress is complete
        if (_reelProgressSlider.value >= ReelTotalProgress)
        {
            Debug.Log("Reel Progress Complete!");
            _reelProgressSlider.value = 0f; // Reset the slider value
            TransitionToState(CastingState); // Transition back to casting state
        }
    }
}