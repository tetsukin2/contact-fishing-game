using UnityEngine;
using UnityEngine.Events;
using static FishingManager;

public class BaitPreparationState : FishingState
{
    public BaitPreparationState(FishingManager fishingManager) : base(fishingManager) { }

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

    public override void Enter()
    {
        // Only go back to start if need more fish, otherwise cam may mess up
        if (GameManager.Instance.FishCaught < GameManager.Instance.FishTotalToCatch)
            CameraController.Instance.SetCameraView(CameraController.CameraView.BaitPrep); // Set camera priority for bait preparation

        // UI
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.BaitPreparation);
        UIManager.Instance.ShowMainInputPrompt(fishingManager.BaitPrepPromptRightName);

        _currentStep = 0; // Reset step counter
        
        // Fishing bobber setup
        fishingManager.FishingBobber.SetControllable(true);
        fishingManager.FishingBobber.SetupLureAttach();
        
        Debug.Log("Entering Bait Preparation State");
    }

    public override void Update()
    {
        // Placeholder as game state transition messes this up
        if (fishingManager.RodMovement.CurrentMovementMode != FishingRodMovement.MovementMode.BaitLock)
            fishingManager.RodMovement.CurrentMovementMode = FishingRodMovement.MovementMode.BaitLock;

        // Alternate directions, even (and start) directions go upward
        if (_currentStep % 2 == 0 
            && fishingManager.InputHelper.HasReachedRotationY(fishingManager.RollRightAngle))
        {
            UIManager.Instance?.ShowMainInputPrompt(fishingManager.BaitPrepPromptLeftName);
            _currentStep++;
        }
        else if (_currentStep % 2 != 0 && fishingManager.InputHelper.HasReachedRotationY(fishingManager.RollLeftAngle))
        {
            UIManager.Instance.ShowMainInputPrompt(fishingManager.BaitPrepPromptRightName);
            _currentStep++;
        }

        if (_currentStep == 1)
            fishingManager.FishingBobber.OnAttachLure();
        //Debug.Log("current step modulo: " + _currentStep % 2);
        //Debug.Log(fishingManager.InputHelper.IsNearRotation(
        //    -90f, InputDeviceManager.RotationAxis.x));

        if (_currentStep >= fishingManager.BaitPreparationSteps)
        {
            fishingManager.TransitionToState(fishingManager.CastingState);
        }
    }

    public override void Exit()
    {
        fishingManager.RodMovement.CurrentMovementMode = FishingRodMovement.MovementMode.Normal;
        fishingManager.FishingBobber.SetControllable(false);
        Debug.Log("Exiting Bait Preparation State");
    }
}