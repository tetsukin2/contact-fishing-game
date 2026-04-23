using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Each "step" is one set of cast back and cast forward
public class CastingState : IFishingState
{
    private int _currentCastSteps = 0;
    private bool _hasCastBack = false;
    private bool _hasCast;

    public UnityEvent LineCast { get; private set; } = new();

    public void Setup()
    {
        _hasCast = false;
        FishingManager.Instance.FishingBobber.BobberHitWater.AddListener(OnBobberHitWater);
    }

    public void Enter()
    {
        var fishingManager = FishingManager.Instance;

        CameraController.Instance.SetCameraView(CameraController.CameraView.FishSelect);
        fishingManager.Targeting.CanChangeSelection = true;
        fishingManager.Targeting.SetRandomFishAsSelected();
        fishingManager.Targeting.ResetSelectionInput();

        _currentCastSteps = 0;
        _hasCast = false;
        _hasCastBack = false;

        fishingManager.StateLabelPanel.SetLabel(FishingManager.FishingStateName.Casting);
        UIManager.Instance.ShowMainInputPrompt(fishingManager.CastBackPromptName);
        UIManager.Instance.ShowSecondInputPrompt(fishingManager.CastSelectPromptName);

        InputDeviceManager.Instance.RotationHelper.ClearRotationHistory();
        InputPromptPanel.MainInstance?.ResetProgress();

        ActionTelemetryHandler.Instance.StartActionTimer("FishSelection");
        ActionTelemetryHandler.Instance.StartActionTimer("CastBack");

        Debug.Log("Entering Casting State");
    }

    public void Update()
    {
        if (_hasCast)
            return;

        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        if (!_hasCastBack &&
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.RotateUpAngle))
        {
            OnCastBack(rotationHelper);
        }
        else if (_hasCastBack &&
                 InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(ResourceSystem.Instance.GameplayConfig.RotateDownAngle))
        {
            OnCastForward(rotationHelper);
        }
    }

    private void OnCastBack(InputDeviceRotationHelper rotationHelper)
    {
        InputPromptPanel.MainInstance?.PlayStepFeedback();
        AudioManager.Instance?.PlaySuccess();

        _hasCastBack = true;
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.CastForwardPromptName);

        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("CastBack");
        ActionTelemetryHandler.Instance.RecordRepetition("CastBack");
        ActionTelemetryHandler.Instance.RecordAngle("CastBack", Mathf.Abs(rotationHelper.CurrentX));

        ActionTelemetryHandler.Instance.StartActionTimer("CastForward");
    }

    private void OnCastForward(InputDeviceRotationHelper rotationHelper)
    {
        FishingManager.Instance.Targeting.CanChangeSelection = false;

        _hasCastBack = false;
        _currentCastSteps++;

        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("CastForward");
        ActionTelemetryHandler.Instance.RecordRepetition("CastForward");
        ActionTelemetryHandler.Instance.RecordAngle("CastForward", Mathf.Abs(rotationHelper.CurrentX));

        if (_currentCastSteps >= FishingManager.Instance.CastSteps)
        {
            _hasCast = true;
            _currentCastSteps = 0;

            InputPromptPanel.MainInstance?.PlayCompletionFeedback();
            AudioManager.Instance?.PlaySuccess();
            AudioManager.Instance?.PlayRodCast();

            LineCast.Invoke();
            UIManager.Instance.ShowMainInputPrompt(null as InputPrompt);
            UIManager.Instance.ShowSecondInputPrompt(null as InputPrompt);

            // Casting motion tactile feedback
            BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", true);

            ActionTelemetryHandler.Instance.EndAndRecordActionTimer("FishSelection");
        }
        else
        {
            InputPromptPanel.MainInstance?.PlayStepFeedback();
            AudioManager.Instance?.PlaySuccess();

            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.CastBackPromptName);
            ActionTelemetryHandler.Instance.StartActionTimer("CastBack");
        }
    }

    private void OnBobberHitWater()
    {
        if (!_hasCast) return;

        AudioManager.Instance?.PlayFishSplash();

        FishingManager.Instance.Targeting.LureFish();

        // Play ripple sequence that already contains 3 repeats, then stop automatically.
        BraillePatternPlayer.Instance.PlayPatternSequence("Ripple", false);

        FishingManager.Instance.TransitionToState(FishingManager.Instance.WaitingForBiteState);
        _hasCast = false;
    }

    public void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}