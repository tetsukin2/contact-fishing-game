using UnityEngine;
using UnityEngine.Events;

public class FishInspectionState : IFishingState
{
    private bool _reachedInitialRotation = false;
    private bool _fishInspected = false;

    /*
     * Prevents the pronation prepare step from repeatedly triggering
     * while the controller is still held in the pronation posture.
     */
    private bool _prepareArmed = true;

    private float _prepareReachedTime = 0f;

    public UnityEvent FishInspected { get; private set; } = new();

    public void Setup() { }

    public void Enter()
    {
        var fishingManager = FishingManager.Instance;
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        CameraController.Instance.SetCameraView(CameraController.CameraView.Gameplay);

        ResetLocalMotionState();
        _fishInspected = false;

        /*
         * Only arm prepare if the controller is already near neutral.
         * If the controller enters this state while still pronated, it must return
         * near neutral first before the prepare step can be triggered.
         */
        //_prepareArmed = FishInspectionMotionRules.HasLostPronationPosture(rotationHelper);
        _prepareArmed = true;

        fishingManager.StateLabelPanel.SetLabel(FishingManager.FishingStateName.FishInspection);
        UIManager.Instance.ShowMainInputPrompt(fishingManager.InspectReadyPromptName);

        fishingManager.HookedFish.SetActive(true);

        rotationHelper.ClearRotationHistory();
        InputPromptPanel.MainInstance?.ResetProgress();

        ActionTelemetryHandler.Instance.StartActionTimer("InspectPrepare");

        Debug.Log(
            $"FishInspectionState: Entered. " +
            $"PrepareArmed={_prepareArmed}, " +
            $"CurrentY={rotationHelper.CurrentY:F3}, " +
            $"PrepareY={FishInspectionMotionRules.GetPronationPrepareYThreshold():F3}, " +
            $"HoldY={FishInspectionMotionRules.GetPronationHoldYThreshold():F3}, " +
            $"ResetY={FishInspectionMotionRules.GetPronationLostResetYThreshold():F3}"
        );
    }

    public void Update()
    {
        if (!_fishInspected)
        {
            FishNotInspectedCheck();
        }
        else
        {
            FishInspectedCheck();
        }
    }

    private void FishNotInspectedCheck()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        if (!_reachedInitialRotation)
        {
            UpdatePrepareArming(rotationHelper);
            CheckInspectPrepare(rotationHelper);
            return;
        }

        if (FishInspectionMotionRules.HasLostPronationPosture(rotationHelper))
        {
            ResetInspectPrepareAfterPoseLost(rotationHelper);
            return;
        }

        CheckInspectExecution(rotationHelper);
    }

    private void FishInspectedCheck()
    {
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;

        if (!_reachedInitialRotation)
        {
            UpdatePrepareArming(rotationHelper);
            CheckReleasePrepare(rotationHelper);
            return;
        }

        if (FishInspectionMotionRules.HasLostPronationPosture(rotationHelper))
        {
            ResetReleasePrepareAfterPoseLost(rotationHelper);
            return;
        }

        CheckReleaseExecution(rotationHelper);
    }

    private void UpdatePrepareArming(InputDeviceRotationHelper rotationHelper)
    {
        if (_prepareArmed)
        {
            return;
        }

        /*
         * Re-arm only when the controller has returned close enough to neutral.
         * This avoids repeated prepare triggering while still pronated.
         */
        if (!FishInspectionMotionRules.HasLostPronationPosture(rotationHelper))
        {
            return;
        }

        _prepareArmed = true;

        Debug.Log(
            $"FishInspectionState: Prepare re-armed after returning near neutral. " +
            $"CurrentY={rotationHelper.CurrentY:F3}, " +
            $"ResetY={FishInspectionMotionRules.GetPronationLostResetYThreshold():F3}"
        );
    }

    private void CheckInspectPrepare(InputDeviceRotationHelper rotationHelper)
    {
        if (!_prepareArmed)
        {
            return;
        }

        if (!FishInspectionMotionRules.HasReachedPronationPrepare(rotationHelper))
        {
            return;
        }

        InputPromptPanel.MainInstance?.PlayStepFeedback();
        AudioManager.Instance?.PlaySuccess();

        _prepareArmed = false;
        _reachedInitialRotation = true;
        _prepareReachedTime = Time.time;

        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.InspectPromptName);

        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("InspectPrepare");
        ActionTelemetryHandler.Instance.RecordRepetition("InspectPrepare");
        ActionTelemetryHandler.Instance.RecordAngle("InspectPrepare", Mathf.Abs(rotationHelper.CurrentY));

        ActionTelemetryHandler.Instance.StartActionTimer("InspectFish");

        Debug.Log(
            $"FishInspectionState: InspectPrepare reached. " +
            $"CurrentX={rotationHelper.CurrentX:F3}, " +
            $"CurrentY={rotationHelper.CurrentY:F3}, " +
            $"PrepareY={FishInspectionMotionRules.GetPronationPrepareYThreshold():F3}, " +
            $"HoldY={FishInspectionMotionRules.GetPronationHoldYThreshold():F3}, " +
            $"ResetY={FishInspectionMotionRules.GetPronationLostResetYThreshold():F3}"
        );
    }

    private void CheckInspectExecution(InputDeviceRotationHelper rotationHelper)
    {
        if (!FishInspectionMotionRules.HasReachedInspectExtension(rotationHelper, _prepareReachedTime))
        {
            return;
        }

        float currentX = rotationHelper.CurrentX;
        float currentY = rotationHelper.CurrentY;

        ResetLocalMotionState();

        InputPromptPanel.MainInstance?.PlayCompletionFeedback();
        AudioManager.Instance?.PlaySuccess();

        FishingManager.Instance.OnFishInspection();

        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReleaseReadyPromptName);
        _fishInspected = true;

        /*
         * Do not immediately allow ReleasePrepare unless the controller
         * has already returned near neutral.
         */
        //_prepareArmed = FishInspectionMotionRules.HasLostPronationPosture(rotationHelper);
        _prepareArmed = true;

        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("InspectFish");
        ActionTelemetryHandler.Instance.RecordRepetition("InspectFish");
        ActionTelemetryHandler.Instance.RecordAngle("InspectFish", Mathf.Abs(currentX));

        ActionTelemetryHandler.Instance.StartActionTimer("ReleasePrepare");

        Debug.Log(
            $"FishInspectionState: InspectFish completed using Extension classifier. " +
            $"CurrentX={currentX:F3}, " +
            $"CurrentY={currentY:F3}, " +
            $"CurrentZ={rotationHelper.CurrentZ:F3}, " +
            $"DetectedMotion={rotationHelper.GetCurrentMotionClassification()}, " +
            $"NextPrepareArmed={_prepareArmed}"
        );
    }

    private void CheckReleasePrepare(InputDeviceRotationHelper rotationHelper)
    {
        if (!_prepareArmed)
        {
            return;
        }

        if (!FishInspectionMotionRules.HasReachedPronationPrepare(rotationHelper))
        {
            return;
        }

        InputPromptPanel.MainInstance?.PlayStepFeedback();
        AudioManager.Instance?.PlaySuccess();

        _prepareArmed = false;
        _reachedInitialRotation = true;
        _prepareReachedTime = Time.time;

        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReleasePromptName);

        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReleasePrepare");
        ActionTelemetryHandler.Instance.RecordRepetition("ReleasePrepare");
        ActionTelemetryHandler.Instance.RecordAngle("ReleasePrepare", Mathf.Abs(rotationHelper.CurrentY));

        ActionTelemetryHandler.Instance.StartActionTimer("ReleaseFish");

        Debug.Log(
            $"FishInspectionState: ReleasePrepare reached. " +
            $"CurrentX={rotationHelper.CurrentX:F3}, " +
            $"CurrentY={rotationHelper.CurrentY:F3}, " +
            $"PrepareY={FishInspectionMotionRules.GetPronationPrepareYThreshold():F3}, " +
            $"HoldY={FishInspectionMotionRules.GetPronationHoldYThreshold():F3}, " +
            $"ResetY={FishInspectionMotionRules.GetPronationLostResetYThreshold():F3}"
        );
    }

    private void CheckReleaseExecution(InputDeviceRotationHelper rotationHelper)
    {
        if (!FishInspectionMotionRules.HasReachedDropFlexion(rotationHelper, _prepareReachedTime))
        {
            return;
        }

        float currentX = rotationHelper.CurrentX;
        float currentY = rotationHelper.CurrentY;

        ResetLocalMotionState();

        InputPromptPanel.MainInstance?.PlayCompletionFeedback();
        AudioManager.Instance?.PlaySuccess();

        ActionTelemetryHandler.Instance.EndAndRecordActionTimer("ReleaseFish");
        ActionTelemetryHandler.Instance.RecordRepetition("ReleaseFish");
        ActionTelemetryHandler.Instance.RecordAngle("ReleaseFish", Mathf.Abs(currentX));

        Debug.Log(
            $"FishInspectionState: ReleaseFish completed using Flexion classifier. " +
            $"CurrentX={currentX:F3}, " +
            $"CurrentY={currentY:F3}, " +
            $"CurrentZ={rotationHelper.CurrentZ:F3}, " +
            $"DetectedMotion={rotationHelper.GetCurrentMotionClassification()}"
        );

        HandleFishAdding();
    }

    private void ResetInspectPrepareAfterPoseLost(InputDeviceRotationHelper rotationHelper)
    {
        ResetLocalMotionState();

        /*
         * Require neutral return before prepare can trigger again.
         */
        _prepareArmed = false;

        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.InspectReadyPromptName);
        InputPromptPanel.MainInstance?.ResetProgress();

        Debug.Log(
            $"FishInspectionState: Inspect pronation posture lost. " +
            $"Returning to InspectReady. " +
            $"CurrentX={rotationHelper.CurrentX:F3}, " +
            $"CurrentY={rotationHelper.CurrentY:F3}, " +
            $"ResetY={FishInspectionMotionRules.GetPronationLostResetYThreshold():F3}"
        );
    }

    private void ResetReleasePrepareAfterPoseLost(InputDeviceRotationHelper rotationHelper)
    {
        ResetLocalMotionState();

        /*
         * Require neutral return before prepare can trigger again.
         */
        _prepareArmed = false;

        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReleaseReadyPromptName);
        InputPromptPanel.MainInstance?.ResetProgress();

        Debug.Log(
            $"FishInspectionState: Release pronation posture lost. " +
            $"Returning to ReleaseReady. " +
            $"CurrentX={rotationHelper.CurrentX:F3}, " +
            $"CurrentY={rotationHelper.CurrentY:F3}, " +
            $"ResetY={FishInspectionMotionRules.GetPronationLostResetYThreshold():F3}"
        );
    }

    private void ResetLocalMotionState()
    {
        _reachedInitialRotation = false;
        _prepareReachedTime = 0f;
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