using UnityEngine;

public class WaitingForBiteState : IFishingState
{
    public void Setup() { }

    public void Enter()
    {
        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.WaitingForBite);
        FishingManager.Instance.Targeting.Selection.ReachedLureLocation.AddListener(OnFishReachLure);
        //_elapsedTime = 0f; //reset timer
        Debug.Log("Entering Waiting For Bite State");
    }

    public void Update() { }

    private void OnFishReachLure()
    {
        // Don't immediately reel, have the fish bite
        FishingManager.Instance.Targeting.Selection.ReachedLureLocation.RemoveListener(OnFishReachLure);
        BraillePatternPlayer.Instance.PlayPatternSequence("RipplePulse", false);
        BraillePatternPlayer.Instance.PatternEnded.AddListener(OnBiteFinished);
    }

    private void OnBiteFinished(BraillePatternPlayer.Finger finger)
    {
        // More to ensure this only fires once
        if (finger== BraillePatternPlayer.Finger.INDEX)
        {
            BraillePatternPlayer.Instance.PatternEnded.RemoveListener(OnBiteFinished);
            FishingManager.Instance.TransitionToState(FishingManager.Instance.ReelingState);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Waiting For Bite State");
    }
}