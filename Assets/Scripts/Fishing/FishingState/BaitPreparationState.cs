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
        //if (GameManager.Instance.FishCaught < GameManager.Instance.FishTotalToCatch)
        if (GameManager.Instance.CurrentState != GameManager.Instance.PlayingState) return;
        CameraController.Instance.SetCameraView(CameraController.CameraView.BaitPrep);

        // UI
        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.BaitPreparation);
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptRightName);

        _currentStep = 0; // Reset step counter

        // Fishing bobber setup
        FishingManager.Instance.FishingBobber.SetControllable(true);
        FishingManager.Instance.FishingBobber.SetupLureAttach();
        
        Debug.Log("Entering Bait Preparation State");
    }

    public void Update()
    {
        // Alternate directions, even (and start) directions go upward
        if (_currentStep % 2 == 0 
            && InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(FishingManager.Instance.RollRightAngle))
        {
            UIManager.Instance?.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptLeftName);
            _currentStep++;
        }
        else if (_currentStep % 2 != 0 && InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(FishingManager.Instance.RollLeftAngle))
        {
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.BaitPrepPromptRightName);
            _currentStep++;
        }

        if (_currentStep == 1)
            FishingManager.Instance.FishingBobber.OnAttachLure();
        //Debug.Log("current step modulo: " + _currentStep % 2);
        //Debug.Log(fishingManager.InputHelper.IsNearRotation(
        //    -90f, InputDeviceManager.RotationAxis.x));

        if (_currentStep >= FishingManager.Instance.BaitPreparationSteps)
        {
            FishingManager.Instance.TransitionToState(FishingManager.Instance.CastingState);
        }
    }

    public void Exit()
    {
        FishingRodGameplayMovement.Instance.SetMovementMode(FishingRodGameplayMovement.MovementMode.Normal);
        FishingManager.Instance.FishingBobber.SetControllable(false);
        Debug.Log("Exiting Bait Preparation State");
    }
}