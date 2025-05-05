using UnityEngine;
using static FishingManager;

public class WaitingForBiteState : FishingState
{
    public WaitingForBiteState(FishingManager fishingManager) : base(fishingManager) { }

    private float _elapsedTime = 0f;

    public override void Enter()
    {
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.WaitingForBite);
        _elapsedTime = 0f; //reset timer
        Debug.Log("Entering Waiting For Bite State");
    }

    public override void Update()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= fishingManager.FishBiteWaitDuration)
        {
            // Simulate a fish bite
            Debug.Log("FishData bit the bait!");
            fishingManager.TransitionToState(fishingManager.ReelingState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Waiting For Bite State");
    }
}