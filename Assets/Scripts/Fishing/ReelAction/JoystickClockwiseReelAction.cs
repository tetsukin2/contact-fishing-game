using System.Collections.Generic;

public class JoystickClockwiseReelAction : IReelAction
{
    public void Enter()
    {
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelClockwisePromptName);
        //Debug.Log("JoystickClockwiseReelAction: Enter");

        // Input helper setup
        InputDeviceManager.Instance.RotationHelper.TrackJoystickClockwise = true;
        InputDeviceManager.Instance.RotationHelper.ResetJoystickRotationCount();

        ActionTelemetryHandler.Instance.StartActionTimer("ReelClockwise");
    }

    public void Update()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        if (InputDeviceManager.Instance.RotationHelper.GetJoystickRotationCount(true) > 0)
        {
            InputDeviceManager.Instance.RotationHelper.ResetJoystickRotationCount();
            FishingManager.Instance.ReelProgressBar.ProgressReel(); // Progress the reel

            ActionTelemetryHandler.Instance.RecordRepetition("ReelClockwise");
            ActionTelemetryHandler.Instance.RecordAngle("ReelClockwise", 360f); 
        }
    }

    public void Exit()
    {
        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReelClockwise");
    }
}
