using UnityEngine;

public class RotateVerticalReelAction : IReelAction
{
    private bool _hasRotatedForward = false;

    public void Enter()
    {
        _hasRotatedForward = false;
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelForwardPromptName);

        // Tilt down / forward = WaveOut
        BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", true);

        ActionTelemetryHandler.Instance.StartActionTimer("ReelForward");
    }

    public void Update()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        /*
         * Reeling should accept radial/ulnar deviation,
         * not generic X-axis movement.
         *
         * This prevents flexion/extension from being accepted here,
         * because flexion/extension may also affect X but have different Z values.
         */

        if (!_hasRotatedForward && rotationHelper.HasReachedUlnarDeviation())
        {
            InputPromptPanel.MainInstance?.PlayStepFeedback();
            AudioManager.Instance?.PlaySuccess();

            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelBackPromptName);
            _hasRotatedForward = true;

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReelForward");
            ActionTelemetryHandler.Instance.RecordRepetition("ReelForward");
            ActionTelemetryHandler.Instance.RecordAngle("ReelForward", Mathf.Abs(rotationHelper.CurrentX));

            // Tilt up / back = WaveIn
            BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", true);

            ActionTelemetryHandler.Instance.StartActionTimer("ReelBack");
        }
        else if (_hasRotatedForward)
        {
            Debug.Log(
                $"WAITING RADIAL | " +
                $"X:{rotationHelper.CurrentX:F2} " +
                $"Y:{rotationHelper.CurrentY:F2} " +
                $"Z:{rotationHelper.CurrentZ:F2} " +
                $"Class:{rotationHelper.GetCurrentMotionClassification()} " +
                $"Radial:{rotationHelper.HasReachedRadialDeviation()}"
            );

             if (rotationHelper.HasReachedRadialDeviationForReeling())
            {

            InputPromptPanel.MainInstance?.PlayCompletionFeedback();
            AudioManager.Instance?.PlaySuccess();

            FishingManager.Instance.ReelProgressBar.ProgressReel();

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReelBack");
            ActionTelemetryHandler.Instance.RecordRepetition("ReelBack");
            ActionTelemetryHandler.Instance.RecordAngle("ReelBack", Mathf.Abs(rotationHelper.CurrentX));
            }
        }
    }

    public void Exit()
    {
        BraillePatternPlayer.Instance.StopPatternSequence(BraillePatternPlayer.Finger.BOTH);
    }
}