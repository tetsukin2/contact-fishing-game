using UnityEngine;
using UnityEngine.Events;

public class FishInspectionState : IFishingState
{
    private bool _reachedInitialRotation = false;
    private bool _fishInspected = false;

    public UnityEvent FishInspected { get; private set; } = new();

    public void Setup() { }

    public void Enter()
    {
        var fishingManager = FishingManager.Instance;

        CameraController.Instance.SetCameraView(CameraController.CameraView.Gameplay);

        _reachedInitialRotation = false;
        _fishInspected = false;

        fishingManager.StateLabelPanel.SetLabel(FishingManager.FishingStateName.FishInspection);
        UIManager.Instance.ShowMainInputPrompt(fishingManager.InspectReadyPromptName);

        fishingManager.HookedFish.SetActive(true);

        InputDeviceManager.Instance.RotationHelper.ClearRotationHistory();
        InputPromptPanel.MainInstance?.ResetProgress();

        ActionTelemetryHandler.Instance.StartActionTimer("InspectPrepare");
    }

    public void Update()
    {
        if (!_fishInspected)
            FishNotInspectedCheck();
        else
            FishInspectedCheck();
    }

    private void FishNotInspectedCheck()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        if (!_reachedInitialRotation &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(-0.8f))
        {
            InputPromptPanel.MainInstance?.PlayStepFeedback();
            AudioManager.Instance?.PlaySuccess();

            _reachedInitialRotation = true;
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.InspectPromptName);

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("InspectPrepare");
            ActionTelemetryHandler.Instance.RecordRepetition("InspectPrepare");
            ActionTelemetryHandler.Instance.RecordAngle("InspectPrepare", Mathf.Abs(rotationHelper.CurrentY));

            ActionTelemetryHandler.Instance.StartActionTimer("InspectFish");
        }
        else if (_reachedInitialRotation &&
                 InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.SideRotateUpAngle))
        {
            _reachedInitialRotation = false;

            InputPromptPanel.MainInstance?.PlayCompletionFeedback();
            AudioManager.Instance?.PlaySuccess();

            FishingManager.Instance.OnFishInspection();

            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReleaseReadyPromptName);
            _fishInspected = true;

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("InspectFish");
            ActionTelemetryHandler.Instance.RecordRepetition("InspectFish");
            ActionTelemetryHandler.Instance.RecordAngle("InspectFish", Mathf.Abs(rotationHelper.CurrentX));
            ActionTelemetryHandler.Instance.StartActionTimer("ReleasePrepare");
        }
    }

    private void FishInspectedCheck()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        if (!_reachedInitialRotation &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(-0.8f))
        {
            InputPromptPanel.MainInstance?.PlayStepFeedback();
            AudioManager.Instance?.PlaySuccess();

            _reachedInitialRotation = true;
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReleasePromptName);

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReleasePrepare");
            ActionTelemetryHandler.Instance.RecordRepetition("ReleasePrepare");
            ActionTelemetryHandler.Instance.RecordAngle("ReleasePrepare", Mathf.Abs(rotationHelper.CurrentY));

            ActionTelemetryHandler.Instance.StartActionTimer("ReleaseFish");
        }
        else if (_reachedInitialRotation &&
                 InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.SideRotateDownAngle))
        {
            InputPromptPanel.MainInstance?.PlayCompletionFeedback();
            AudioManager.Instance?.PlaySuccess();
            HandleFishAdding();

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReleaseFish");
            ActionTelemetryHandler.Instance.RecordRepetition("ReleaseFish");
            ActionTelemetryHandler.Instance.RecordAngle("ReleaseFish", Mathf.Abs(rotationHelper.CurrentX));
        }
    }

    private void HandleFishAdding()
    {
        LevelManager.Instance.AddFish();
        BraillePatternPlayer.Instance.PlayPatternSequence("BasicPulse", false);

        if (LevelManager.Instance.FishCaught < LevelManager.Instance.FishTotalToCatch)
        {
            FishingManager.Instance.TransitionToState(FishingManager.Instance.BaitPreparationState);
        }
        else
        {
            FishingManager.Instance.TransitionToState(FishingManager.Instance.IdleFishingState);
            LevelManager.Instance.TransitionToState(LevelManager.Instance.GameEndState);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting FishData Inspection State");
        FishingManager.Instance.HideFishInspection();
    }
}