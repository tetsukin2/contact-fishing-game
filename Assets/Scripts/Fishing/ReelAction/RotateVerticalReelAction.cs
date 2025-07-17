using UnityEngine;

public class RotateVerticalReelAction : IReelAction
{
    private bool _hasRotatedForward = false; // Need the initial rotation for proper input

    public void Enter()
    {
        _hasRotatedForward = false; // Reset for new action
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelForwardPromptName);
        //Debug.Log("RotateVerticalReelAction: Enter");
    }

    public void Update()
    {
        if (!_hasRotatedForward &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.RotateDownAngle))
        {
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelBackPromptName);
            _hasRotatedForward = true;
        }
        else if (_hasRotatedForward &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.RotateUpAngle))
        {
            FishingManager.Instance.ReelProgressBar.ProgressReel(); // Progress the reel
        }
    }

    public void Exit()
    {
        //Debug.Log("RotateVerticalReelAction: Exit");
    }
}
