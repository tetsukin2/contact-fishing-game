public class RotateVerticalReelAction : IReelAction
{
    private bool _hasRotatedForward = false; // Need the initial rotation for proper input

    public void Enter()
    {
        _hasRotatedForward = false; // Reset for new action
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelForwardPromptName);

        ActionTelemetryHandler.Instance.StartActionTimer("ReelForward");
    }

    public void Update()
    {
        if (!_hasRotatedForward &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.RotateDownAngle))
        {
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReelBackPromptName);
            _hasRotatedForward = true;

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReelForward");
            ActionTelemetryHandler.Instance.StartActionTimer("ReelBack");
        }
        else if (_hasRotatedForward &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.RotateUpAngle))
        {
            FishingManager.Instance.ReelProgressBar.ProgressReel();

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReelBack");
        }
    }

    public void Exit() { }
}
