using UnityEngine;

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