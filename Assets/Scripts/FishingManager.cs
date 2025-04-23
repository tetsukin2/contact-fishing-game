using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FishingManager with class-based state machine
public class FishingManager : MonoBehaviour
{
    public IdleState IdleState { get; private set; }
    public CastingState CastingState { get; private set; }
    public BaitPreparationState BaitPreparationState { get; private set; }
    public ReelingState ReelingState { get; private set; }

    [SerializeField] public FishingRodInputManager FishingRodInput { get; private set; }

    private FishingState _currentState;

    private void Awake()
    {
        // Initialize states
        IdleState = new(this);
        CastingState = new(this);
        BaitPreparationState = new(this);
        ReelingState = new(this);
    }

    void Start()
    {
        // Initialize with the IdleState
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
}