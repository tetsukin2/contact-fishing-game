using UnityEngine;

public class RotateVerticalReelAction : ReelAction
{
    public RotateVerticalReelAction(ReelingState reelingState) : base(reelingState) { }

    private bool _hasRotatedForward = false; // Need the initial rotation for proper input

    public override void Enter()
    {
        _hasRotatedForward = false; // Reset for new action
        fishingManager.ShowInputPrompt(fishingManager.ReelForwardPromptName);
        Debug.Log("RotateVerticalReelAction: Enter");
    }

    public override void Update()
    {
        if (!_hasRotatedForward && 
            reelingState.FishingManager.InputHelper.HasReachedRotationZ(Mathf.Lerp(fishingManager.RotateDownAngle, 0f, InputDeviceManager.IMURotation.y)))
        {
            fishingManager.ShowInputPrompt(fishingManager.ReelBackPromptName);
            _hasRotatedForward = true;
        }
        else if (_hasRotatedForward &&
            reelingState.FishingManager.InputHelper.HasReachedRotationZ(Mathf.Lerp(fishingManager.RotateUpAngle, 0f, InputDeviceManager.IMURotation.y)))
        {
            reelingState.ProgressReel();
        }
    }

    public override void Exit()
    {
        Debug.Log("RotateVerticalReelAction: Exit");
    }
}
