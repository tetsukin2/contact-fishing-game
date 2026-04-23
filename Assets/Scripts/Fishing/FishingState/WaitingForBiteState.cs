using UnityEngine;

public class WaitingForBiteState : IFishingState
{
    private bool _fishReachedLure = false;
    private bool _biteCueFinished = false;
    private bool _waitingForHookInput = false;
    private bool _resolved = false;
    private bool _showingEscapeMessage = false;

    private float _reactionTimer = 0f;
    private float _escapeMessageTimer = 0f;

    public void Setup() { }

    public void Enter()
    {
        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.WaitingForBite);

        _fishReachedLure = false;
        _biteCueFinished = false;
        _waitingForHookInput = false;
        _resolved = false;
        _showingEscapeMessage = false;
        _reactionTimer = 0f;
        _escapeMessageTimer = 0f;

        UIManager.Instance.ShowMainInputPrompt(null as InputPrompt);
        UIManager.Instance.ShowSecondInputPrompt(null as InputPrompt);

        if (FishingManager.Instance.Targeting.Selection != null)
        {
            FishingManager.Instance.Targeting.Selection.ReachedLureLocation.AddListener(OnFishReachLure);
        }

        InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(OnJoystickPressedWhileWaiting);

        Debug.Log("Entering Waiting For Bite State");
    }

    public void Update()
    {
        if (_resolved)
            return;

        if (_waitingForHookInput)
        {
            _reactionTimer -= Time.deltaTime;

            if (_reactionTimer <= 0f)
            {
                HandleHookMissed();
            }
        }
        else if (_showingEscapeMessage)
        {
            _escapeMessageTimer -= Time.deltaTime;

            if (_escapeMessageTimer <= 0f)
            {
                _resolved = true;
                FishingManager.Instance.TransitionToState(FishingManager.Instance.BaitPreparationState);
            }
        }
    }

    private void OnFishReachLure()
    {
        if (_fishReachedLure || _resolved)
            return;

        _fishReachedLure = true;

        if (FishingManager.Instance.Targeting.Selection != null)
        {
            FishingManager.Instance.Targeting.Selection.ReachedLureLocation.RemoveListener(OnFishReachLure);
        }

        InputPromptPanel.MainInstance?.PlayStepFeedback();
        CameraImpulseManager.Instance?.TriggerImpulse();

        // Phase 1 user test: no haptics.
        // Since BraillePatternPlayer returns early and PatternEnded never fires,
        // start the hook reaction window immediately.
        if (UserTestConfig.IsUserTestMode && !UserTestConfig.HapticsEnabled)
        {
            _biteCueFinished = true;
            StartHookReactionWindow();
            return;
        }

        // Normal flow / haptics-on flow:
        BraillePatternPlayer.Instance.PlayPatternSequence("BiteCenterPulse", false);
        BraillePatternPlayer.Instance.PatternEnded.AddListener(OnBiteFinished);
    }

    private void OnBiteFinished(BraillePatternPlayer.Finger finger)
    {
        if (_resolved || _biteCueFinished)
            return;

        _biteCueFinished = true;
        BraillePatternPlayer.Instance.PatternEnded.RemoveListener(OnBiteFinished);

        StartHookReactionWindow();
    }

    private void StartHookReactionWindow()
    {
        _waitingForHookInput = true;
        _reactionTimer = FishingManager.Instance.BiteReactionWindow;

        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.HookIt);
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BiteNowPromptName);

        CameraImpulseManager.Instance?.TriggerImpulse();

        Transform bobber = FishingManager.Instance.FishingBobber != null
            ? FishingManager.Instance.FishingBobber.transform
            : null;

        Transform fish = FishingManager.Instance.Targeting.Selection != null
            ? FishingManager.Instance.Targeting.Selection.transform
            : null;

        if (bobber != null && fish != null)
        {
            CameraController.Instance?.StartFishBiteFocus(bobber, fish, 32f, 0.18f);
        }
        else
        {
            CameraController.Instance?.ZoomFishSelectCamera(32f, 0.18f);
        }

        Debug.Log($"Hook reaction window started: {_reactionTimer:F2}s");
    }

    private void OnJoystickPressedWhileWaiting()
    {
        if (_resolved || !_waitingForHookInput)
            return;

        _waitingForHookInput = false;
        _resolved = true;

        UIManager.Instance.ShowMainInputPrompt(null as InputPrompt);
        InputPromptPanel.MainInstance?.PlayCompletionFeedback();
        AudioManager.Instance?.PlaySuccess();

        CameraImpulseManager.Instance?.TriggerImpulse();

        CameraController.Instance?.StopFishBiteFocus(0.2f);

        FishingManager.Instance.TransitionToState(FishingManager.Instance.ReelingState);
    }

    private void HandleHookMissed()
    {
        if (_resolved)
            return;

        AudioManager.Instance?.PlayFishEscape();

        _waitingForHookInput = false;
        _showingEscapeMessage = true;
        _escapeMessageTimer = FishingManager.Instance.EscapeMessageDuration;

        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.FishEscaped);
        UIManager.Instance.ShowMainInputPrompt(null as InputPrompt);

        FishingManager.Instance.Targeting.EscapeSelected();

        BraillePatternPlayer.Instance.PlayPatternSequence("BasicPulse", false);
        InputPromptPanel.MainInstance?.PlayStepFeedback();

        CameraImpulseManager.Instance?.TriggerImpulse(new Vector3(0f, -0.5f, 0f));

        CameraController.Instance?.StopFishBiteFocus(0.2f);

        Debug.Log("Fish escaped.");
    }

    public void Exit()
    {
        if (FishingManager.Instance != null && FishingManager.Instance.Targeting.Selection != null)
        {
            FishingManager.Instance.Targeting.Selection.ReachedLureLocation.RemoveListener(OnFishReachLure);
        }

        if (InputDeviceManager.Instance != null && InputDeviceManager.Instance.JoystickInput != null)
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.RemoveListener(OnJoystickPressedWhileWaiting);
        }

        if (BraillePatternPlayer.Instance != null)
        {
            BraillePatternPlayer.Instance.PatternEnded.RemoveListener(OnBiteFinished);
        }

        CameraController.Instance?.StopFishBiteFocus(0.15f);

        Debug.Log("Exiting Waiting For Bite State");
    }
}