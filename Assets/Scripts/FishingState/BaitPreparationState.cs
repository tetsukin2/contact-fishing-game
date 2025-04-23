using UnityEngine;

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