using UnityEngine;

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