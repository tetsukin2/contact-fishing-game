using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaitPreparationState : IFishingState
{
    private int _currentStep = 0;

    // Events for each bobber action
    // Nomenclature could be improved?
    /// <summary>
    /// Representation of even and starting rotations (such as the initial lure hooking)
    /// </summary>
    public UnityEvent CompletedEvenRotation { get; private set; } = new();
    /// <summary>
    /// Representation of odd rotations (bobber turning away)
    /// </summary>
    public UnityEvent CompletedOddRotation { get; private set; } = new();

    public void Setup() { }

    public void Enter()
    {
        // Do not adjust cam if we already ending
        //if (LevelManager.Instance.FishCaught < LevelManager.Instance.FishTotalToCatch)
        if (LevelManager.Instance.CurrentState != LevelManager.Instance.PlayingState) return;
        CameraController.Instance.SetCameraView(CameraController.CameraView.BaitPrep);

        FishingRodGameplay.Instance.SetMovementMode(FishingRodGameplay.MovementMode.BaitLock);

        // UI
        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.BaitPreparation);
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptRightName);

        _currentStep = 0; // Reset step counter

        // Fishing bobber setup
        FishingManager.Instance.FishingBobber.SetControllable(true);
        FishingManager.Instance.FishingBobber.SetupLureAttach();

        ActionTelemetryHandler.Instance.StartActionTimer("BaitPreparationRight"); // Start telemetry timer

        Debug.Log("Entering Bait Preparation State");
    }

    public void Update()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;
        // Alternate directions, even (and start) directions go upward
        // TODO: Investigate why these prompts are messed up
        if (_currentStep % 2 == 0 
            && InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(-ResourceSystem.Instance.GameplayConfig.RollRightAngle))
        {
            Debug.Log("has reached rotation");
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptLeftName);
            _currentStep++;

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("BaitPreparationRight");
            ActionTelemetryHandler.Instance.RecordRepetition("BaitPreparationRight");
            ActionTelemetryHandler.Instance.RecordAngle("BaitPreparationRight", Mathf.Abs(rotationHelper.CurrentY));

            if (!IsBaitPreparationComplete())
                ActionTelemetryHandler.Instance.StartActionTimer("BaitPreparationLeft");
        }
        else if (_currentStep % 2 != 0 && InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(-ResourceSystem.Instance.GameplayConfig.RollLeftAngle))
        {
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptRightName);
            _currentStep++;

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("BaitPreparationLeft");
            ActionTelemetryHandler.Instance.RecordRepetition("BaitPreparationLeft");
            ActionTelemetryHandler.Instance.RecordAngle("BaitPreparationLeft", Mathf.Abs(rotationHelper.CurrentY));

            if (!IsBaitPreparationComplete())
                ActionTelemetryHandler.Instance.StartActionTimer("BaitPreparationRight");
        }

        if (_currentStep == 1)
            FishingManager.Instance.FishingBobber.OnAttachLure();
        //Debug.Log("current step modulo: " + _currentStep % 2);
        //Debug.Log(fishingManager.InputHelper.IsNearRotation(
        //    -90f, InputDeviceManager.RotationAxis.x));

        if (IsBaitPreparationComplete())
            FishingManager.Instance.TransitionToState(FishingManager.Instance.CastingState);
    }

    public void Exit()
    {
        FishingRodGameplay.Instance.SetMovementMode(FishingRodGameplay.MovementMode.Normal);
        FishingManager.Instance.FishingBobber.SetControllable(false);
        Debug.Log("Exiting Bait Preparation State");
    }

    // Have we reached the steps needed to complete bait preparation?
    private bool IsBaitPreparationComplete()
    {
        return _currentStep >= ResourceSystem.Instance.GameplayConfig.BaitPreparationSteps;
    }
}