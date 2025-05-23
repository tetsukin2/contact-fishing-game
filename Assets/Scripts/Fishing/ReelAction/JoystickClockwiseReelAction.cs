public class JoystickClockwiseReelAction : ReelAction
{
    public JoystickClockwiseReelAction(ReelingState reelingState) : base(reelingState) { }

    public override void Enter()
    {
        UIManager.Instance.ShowMainInputPrompt(fishingManager.ReelClockwisePromptName);
        //Debug.Log("JoystickClockwiseReelAction: Enter");

        // Input helper setup
        InputDeviceManager.Instance.RotationHelper.TrackJoystickClockwise = true;
        InputDeviceManager.Instance.RotationHelper.ResetJoystickRotationCount();
    }

    public override void Update()
    {
        if (InputDeviceManager.Instance.RotationHelper.GetJoystickRotationCount(true) > 0)
        {
            InputDeviceManager.Instance.RotationHelper.ResetJoystickRotationCount();
            FishingManager.Instance.ReelProgressBar.ProgressReel(); // Progress the reel
        }
    }

    public override void Exit()
    {
        //Debug.Log("JoystickClockwiseReelAction: Exit");
    }
}
