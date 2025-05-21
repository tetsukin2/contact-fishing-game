using UnityEngine;

public class RotateVerticalReelAction : ReelAction
{
    public RotateVerticalReelAction(ReelingState reelingState) : base(reelingState) { }

    private bool _hasRotatedForward = false; // Need the initial rotation for proper input

    public override void Enter()
    {
        _hasRotatedForward = false; // Reset for new action
        UIManager.Instance.ShowMainInputPrompt(fishingManager.ReelForwardPromptName);
        //Debug.Log("RotateVerticalReelAction: Enter");
    }

    public override void Update()
    {
        if (!_hasRotatedForward &&
            InputDeviceRotationHelper.HasReachedRotation(Mathf.Lerp(InputDeviceManager.IMURotation.z, 0f, Mathf.Abs(InputDeviceManager.IMURotation.y)), fishingManager.RotateDownAngle))
        {
            UIManager.Instance.ShowMainInputPrompt(fishingManager.ReelBackPromptName);
            _hasRotatedForward = true;
        }
        else if (_hasRotatedForward &&
            InputDeviceRotationHelper.HasReachedRotation(Mathf.Lerp(InputDeviceManager.IMURotation.z, 0f, Mathf.Abs(InputDeviceManager.IMURotation.y)), fishingManager.RotateUpAngle))
        {
            FishingManager.Instance.ReelProgressBar.ProgressReel(); // Progress the reel
        }
    }

    public override void Exit()
    {
        //Debug.Log("RotateVerticalReelAction: Exit");
    }
}
