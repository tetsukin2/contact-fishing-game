using UnityEngine;

public class JoystickClockwiseReelAction : IReelAction
{
    public void Enter()
    {
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelClockwisePromptName);

        // Input helper setup
        InputDeviceManager.Instance.RotationHelper.TrackJoystickClockwise = true;
        InputDeviceManager.Instance.RotationHelper.ResetJoystickRotationCount();

        // Reset the prompt progress ring for this action
        if (InputPromptPanel.MainInstance != null)
            InputPromptPanel.MainInstance.ResetProgress();

        ActionTelemetryHandler.Instance.StartActionTimer("ReelClockwise");
    }

    public void Update()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        // Assumes this returns a value that can be used as progress toward one clockwise rotation.
        float clockwiseProgress = rotationHelper.GetJoystickRotationCount(true);

        // Update the visual feedback ring while rotating.
        if (InputPromptPanel.MainInstance != null)
        {
            float normalizedProgress = Mathf.Clamp01(clockwiseProgress);
            InputPromptPanel.MainInstance.SetProgress(normalizedProgress);
        }

        if (clockwiseProgress >= 1f)
        {
            rotationHelper.ResetJoystickRotationCount();
            FishingManager.Instance.ReelProgressBar.ProgressReel();

            ActionTelemetryHandler.Instance.RecordRepetition("ReelClockwise");
            ActionTelemetryHandler.Instance.RecordAngle("ReelClockwise", 360f);
        }
    }

    public void Exit()
    {
        InputDeviceManager.Instance.RotationHelper.TrackJoystickClockwise = false;

        if (InputPromptPanel.MainInstance != null)
            InputPromptPanel.MainInstance.ResetProgress();

        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReelClockwise");
    }
}