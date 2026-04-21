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

        // Get smooth progress toward one full clockwise rotation (0 to 1)
        float clockwiseProgress = rotationHelper.GetJoystickRotationProgress01(true);

        // Update the visual feedback ring while rotating.
        if (InputPromptPanel.MainInstance != null)
        {
            float normalizedProgress = Mathf.Clamp01(clockwiseProgress);
            InputPromptPanel.MainInstance.SetProgress(normalizedProgress);
        }

        // Check if a full clockwise rotation has been completed
        if (rotationHelper.GetJoystickRotationCount(true) > 0)
        {
            // Force progress to full to trigger completion feedback (shine)
            if (InputPromptPanel.MainInstance != null)
                InputPromptPanel.MainInstance.SetProgress(1f);

            // Play success SFX when a full rotation is completed
            AudioManager.Instance?.PlaySuccess();

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