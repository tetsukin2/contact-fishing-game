using UnityEngine;

public class CastingState : FishingState
{
    public CastingState(FishingManager fishingManager) : base(fishingManager) { }

    private bool _hasCast;

    public override void Setup()
    {
        _hasCast = false;
        fishingManager.FishingBobber.HasHitWater.AddListener(() => {
            if (_hasCast)
            {
                fishingManager.TransitionToState(fishingManager.ReelingState);
                _hasCast = false;
            }});
    }

    public override void Enter()
    {
        Debug.Log("Entering Casting State");
    }

    public override void Update()
    {
        if (!_hasCast && fishingManager.InputHelper.HasRotatedByDegrees(-fishingManager.RotationTriggerThreshold, InputDeviceManager.RotationAxis.y))
        {
            _hasCast = true;
            fishingManager.CastLine();
            //fishingManager.TransitionToState(fishingManager.ReelingState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}