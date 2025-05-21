public class JoystickClockwiseReelAction : ReelAction
{
    public JoystickClockwiseReelAction(ReelingState reelingState) : base(reelingState) { }

    public override void Enter()
    {
        UIManager.Instance.ShowMainInputPrompt(fishingManager.ReelClockwisePromptName);
        //Debug.Log("JoystickClockwiseReelAction: Enter");

        // Input helper setup
        fishingManager.InputHelper.TrackJoystickClockwise = true;
        fishingManager.InputHelper.ResetJoystickRotationCount();
    }

    public override void Update()
    {
        if (fishingManager.InputHelper.GetJoystickRotationCount(true) > 0)
        {
            fishingManager.InputHelper.ResetJoystickRotationCount();
            FishingManager.Instance.ReelProgressBar.ProgressReel(); // Progress the reel
        }
    }

    public override void Exit()
    {
        //Debug.Log("JoystickClockwiseReelAction: Exit");
    }
}
