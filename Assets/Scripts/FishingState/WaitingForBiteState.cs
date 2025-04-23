using UnityEngine;

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
            fishingManager.TransitionToState(new ReelingState(fishingManager));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Waiting For Bite State");
    }
}