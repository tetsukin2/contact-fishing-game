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
        if (InputDeviceManager.joystickPressed)
        {
            fishingManager.ReelIn();
            fishingManager.TransitionToState(fishingManager.CastingState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Reeling State");
    }
}