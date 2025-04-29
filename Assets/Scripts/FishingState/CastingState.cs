using UnityEngine;
using static FishingManager;

public class CastingState : FishingState
{
    public CastingState(FishingManager fishingManager) : base(fishingManager) { }

    private bool _hasCast;

    // Hook listener for bobber hitting water only once
    public override void Setup()
    {
        _hasCast = false;
        fishingManager.FishingBobber.HasHitWater.AddListener(() => {
            if (_hasCast)
            {
                fishingManager.TransitionToState(fishingManager.WaitingForBiteState);
                BraillePatternPlayer.Instance.StopPatternSequence(BraillePatternPlayer.Finger.BOTH);
                _hasCast = false;
            }});
    }

    public override void Enter()
    {
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.Casting);
        fishingManager.InputHelper.ClearRotationHistory(); // Clean read for casting
        fishingManager.ShowInputPrompt("ControllerDown");
        Debug.Log("Entering Casting State");
    }

    public override void Update()
    {
        if (!_hasCast && fishingManager.InputHelper.HasRotatedByDegrees(-fishingManager.RotationTriggerThreshold, InputDeviceManager.RotationAxis.y))
        {
            _hasCast = true;
            fishingManager.ShowInputPrompt("");
            fishingManager.CastLine();
            BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", BraillePatternPlayer.Finger.THUMB, true);
            BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", BraillePatternPlayer.Finger.INDEX, true);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}