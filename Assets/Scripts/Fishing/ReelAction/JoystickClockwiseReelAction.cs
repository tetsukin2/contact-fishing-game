public class JoystickClockwiseReelAction : IReelAction
{
    public void Enter()
    {
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelClockwisePromptName);
        //Debug.Log("JoystickClockwiseReelAction: Enter");

        // Input helper setup
        InputDeviceManager.Instance.RotationHelper.TrackJoystickClockwise = true;
        InputDeviceManager.Instance.RotationHelper.ResetJoystickRotationCount();
    }

    public void Update()
    {
        if (InputDeviceManager.Instance.RotationHelper.GetJoystickRotationCount(true) > 0)
        {
            InputDeviceManager.Instance.RotationHelper.ResetJoystickRotationCount();
            FishingManager.Instance.ReelProgressBar.ProgressReel(); // Progress the reel
        }
    }

    public void Exit()
    {
        //Debug.Log("JoystickClockwiseReelAction: Exit");
    }
}
