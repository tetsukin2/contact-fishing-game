using UnityEngine;
using static FishingManager;

public class WaitingForBiteState : FishingState
{
    public WaitingForBiteState(FishingManager fishingManager) : base(fishingManager) { }

    //private float _elapsedTime = 0f;

    public override void Enter()
    {
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.WaitingForBite);
        fishingManager.Targeting.Selection.ReachedLureLocation.AddListener(OnFishReachLure);
        //_elapsedTime = 0f; //reset timer
        Debug.Log("Entering Waiting For Bite State");
    }

    public override void Update()
    {
        //_elapsedTime += Time.deltaTime;
        //if (_elapsedTime >= fishingManager.FishBiteWaitDuration)
        //{
        //    // Simulate a fish bite
        //    Debug.Log("FishData bit the bait!");
        //    fishingManager.TransitionToState(fishingManager.ReelingState);
        //}
    }

    private void OnFishReachLure()
    {
        // Don't immediately reel, have the fish bite
        fishingManager.Targeting.Selection.ReachedLureLocation.RemoveListener(OnFishReachLure);
        BraillePatternPlayer.Instance.PlayPatternSequence("RipplePulse", false);
        BraillePatternPlayer.Instance.PatternEnded.AddListener(OnBiteFinished);
    }

    private void OnBiteFinished(BraillePatternPlayer.Finger finger)
    {
        // More to ensure this only fires once
        if (finger== BraillePatternPlayer.Finger.INDEX)
        {
            BraillePatternPlayer.Instance.PatternEnded.RemoveListener(OnBiteFinished);
            fishingManager.TransitionToState(fishingManager.ReelingState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Waiting For Bite State");
    }
}