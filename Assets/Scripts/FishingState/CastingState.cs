using UnityEngine;

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
                BraillePatternPlayer.Instance.StopPatternSequence();
                _hasCast = false;
            }});
    }

    public override void Enter()
    {
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
            BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", true);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}