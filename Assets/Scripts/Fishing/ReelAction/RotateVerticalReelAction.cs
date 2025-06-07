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
            InputDeviceRotationHelper.HasReachedRotation(
                Mathf.Lerp(InputDeviceManager.Instance.IMUInput.Rotation.z, 0f, Mathf.Abs(InputDeviceManager.Instance.IMUInput.Rotation.y)),
                FishingManager.Instance.RotateDownAngle))
        {
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelBackPromptName);
            _hasRotatedForward = true;
        }
        else if (_hasRotatedForward &&
            InputDeviceRotationHelper.HasReachedRotation(
                Mathf.Lerp(InputDeviceManager.Instance.IMUInput.Rotation.z, 0f, Mathf.Abs(InputDeviceManager.Instance.IMUInput.Rotation.y)),
                FishingManager.Instance.RotateUpAngle))
        {
            FishingManager.Instance.ReelProgressBar.ProgressReel(); // Progress the reel
        }
    }

    public void Exit()
    {
        //Debug.Log("RotateVerticalReelAction: Exit");
    }
}
