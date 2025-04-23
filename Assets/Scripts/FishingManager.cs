using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FishingManager with class-based state machine
public class FishingManager : MonoBehaviour
{
    public float CastForce = 10f;  // Adjust casting strength
    public float RotationTriggerThreshold = 15f;  // Rotation threshold in degrees
    
    public IdleState IdleState { get; private set; }
    public CastingState CastingState { get; private set; }
    public BaitPreparationState BaitPreparationState { get; private set; }
    public ReelingState ReelingState { get; private set; }

    [SerializeField] private FishingBobber _fishingBobber; // Reference to the FishingBobber script
    [SerializeField] private FishingRodInputHelper _inputHelper;

    private FishingState _currentState;

    public FishingBobber FishingBobber => _fishingBobber;
    public FishingRodInputHelper InputHelper => _inputHelper;

    private void Awake()
    {
        // Setup states
        IdleState = new(this);
        CastingState = new(this);
        BaitPreparationState = new(this);
        ReelingState = new(this);
    }

    void Start()
    {
        CastingState.Setup();
        // Setup with the IdleState
        TransitionToState(IdleState);
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
}