using UnityEngine;

public class RotateUpReelAction : ReelAction
{
    public RotateUpReelAction(ReelingState reelingState) : base(reelingState) { }

    public override void Enter()
    {
        Debug.Log("RotateUpReelAction: Enter");
    }

    public override void Update()
    {
        if (reelingState.FishingManager.InputHelper.HasRotatedByDegrees(
            reelingState.FishingManager.RotationTriggerThreshold, InputDeviceManager.RotationAxis.y))
        {
            reelingState.ProgressReel();
        }
    }

    public override void Exit()
    {
        Debug.Log("RotateUpReelAction: Exit");
    }
}
