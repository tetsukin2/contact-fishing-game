using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaitPreparationState : IFishingState
{
    private int _currentStep = 0;

    public UnityEvent CompletedEvenRotation { get; private set; } = new();
    public UnityEvent CompletedOddRotation { get; private set; } = new();

    public void Setup() { }

    public void Enter()
    {
        if (LevelManager.Instance.CurrentState != LevelManager.Instance.PlayingState) return;

        CameraController.Instance.SetCameraView(CameraController.CameraView.BaitPrep);
        FishingRodGameplay.Instance.SetMovementMode(FishingRodGameplay.MovementMode.BaitLock);

        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.BaitPreparation);
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptRightName);

        _currentStep = 0;

        FishingManager.Instance.FishingBobber.SetControllable(true);
        FishingManager.Instance.FishingBobber.SetupLureAttach();

        // Clean UI/input state so the first prompt behaves consistently
        InputDeviceManager.Instance.RotationHelper.ClearRotationHistory();
        InputPromptPanel.MainInstance?.ResetProgress();

        ActionTelemetryHandler.Instance.StartActionTimer("BaitPreparationRight");

        Debug.Log("Entering Bait Preparation State");
    }

    public void Update()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        if (_currentStep % 2 == 0 &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(-ResourceSystem.Instance.GameplayConfig.RollRightAngle))
        {
            InputPromptPanel.MainInstance?.PlayStepFeedback();
            AudioManager.Instance?.PlaySuccess();

            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptLeftName);
            _currentStep++;

            CompletedEvenRotation.Invoke();

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("BaitPreparationRight");
            ActionTelemetryHandler.Instance.RecordRepetition("BaitPreparationRight");
            ActionTelemetryHandler.Instance.RecordAngle("BaitPreparationRight", Mathf.Abs(rotationHelper.CurrentY));

            if (!IsBaitPreparationComplete())
                ActionTelemetryHandler.Instance.StartActionTimer("BaitPreparationLeft");
        }
        else if (_currentStep % 2 != 0 &&
                 InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(-ResourceSystem.Instance.GameplayConfig.RollLeftAngle))
        {
            InputPromptPanel.MainInstance?.PlayStepFeedback();
            AudioManager.Instance?.PlaySuccess();

            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptRightName);
            _currentStep++;

            CompletedOddRotation.Invoke();

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("BaitPreparationLeft");
            ActionTelemetryHandler.Instance.RecordRepetition("BaitPreparationLeft");
            ActionTelemetryHandler.Instance.RecordAngle("BaitPreparationLeft", Mathf.Abs(rotationHelper.CurrentY));

            if (!IsBaitPreparationComplete())
                ActionTelemetryHandler.Instance.StartActionTimer("BaitPreparationRight");
        }

        if (_currentStep == 1)
            FishingManager.Instance.FishingBobber.OnAttachLure();

        if (IsBaitPreparationComplete())
        {
            InputPromptPanel.MainInstance?.PlayCompletionFeedback();
            AudioManager.Instance?.PlaySuccess();
            FishingManager.Instance.TransitionToState(FishingManager.Instance.CastingState);
        }
    }

    public void Exit()
    {
        FishingRodGameplay.Instance.SetMovementMode(FishingRodGameplay.MovementMode.Normal);
        FishingManager.Instance.FishingBobber.SetControllable(false);
        Debug.Log("Exiting Bait Preparation State");
    }

    private bool IsBaitPreparationComplete()
    {
        return _currentStep >= ResourceSystem.Instance.GameplayConfig.BaitPreparationSteps;
    }
}