using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for all fishing states
public abstract class FishingState
{
    protected FishingManager _fishingManager;

    public FishingState(FishingManager fishingManager)
    {
        _fishingManager = fishingManager;
    }

    // Called when entering the state
    public virtual void Enter() { }

    // Called on every frame while in this state
    public virtual void Update() { }

    // Called when transitioning out of the state
    public virtual void Exit() { }
}

// FishingManager with class-based state machine
public class FishingManager : MonoBehaviour
{
    private FishingState _currentState;

    void Start()
    {
        // Initialize with the IdleState
        TransitionToState(new IdleState(this));
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

// Idle state
public class IdleState : FishingState
{
    public IdleState(FishingManager fishingManager) : base(fishingManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Idle State");
    }

    public override void Update()
    {
        // Example: Transition to CastingState when a key is pressed
        if (Input.GetKeyDown(KeyCode.C))
        {
            _fishingManager.TransitionToState(new CastingState(_fishingManager));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Idle State");
    }
}

// Casting state
public class CastingState : FishingState
{
    public CastingState(FishingManager fishingManager) : base(fishingManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Casting State");
    }

    public override void Update()
    {
        // Example: Transition to BaitPreparationState after casting
        if (Input.GetKeyDown(KeyCode.B))
        {
            _fishingManager.TransitionToState(new BaitPreparationState(_fishingManager));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}

// Bait preparation state
public class BaitPreparationState : FishingState
{
    public BaitPreparationState(FishingManager fishingManager) : base(fishingManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Bait Preparation State");
    }

    public override void Update()
    {
        // Example: Transition to WaitingForBiteState after bait preparation
        if (Input.GetKeyDown(KeyCode.W))
        {
            _fishingManager.TransitionToState(new WaitingForBiteState(_fishingManager));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Bait Preparation State");
    }
}

// Waiting for bite state
public class WaitingForBiteState : FishingState
{
    public WaitingForBiteState(FishingManager fishingManager) : base(fishingManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Waiting For Bite State");
    }

    public override void Update()
    {
        // Example: Transition to ReelingState when a bite is detected
        if (Input.GetKeyDown(KeyCode.R))
        {
            _fishingManager.TransitionToState(new ReelingState(_fishingManager));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Waiting For Bite State");
    }
}

// Reeling state
public class ReelingState : FishingState
{
    public ReelingState(FishingManager fishingManager) : base(fishingManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Reeling State");
    }

    public override void Update()
    {
        // Example: Transition to IdleState after reeling
        if (Input.GetKeyDown(KeyCode.I))
        {
            _fishingManager.TransitionToState(new IdleState(_fishingManager));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Reeling State");
    }
}