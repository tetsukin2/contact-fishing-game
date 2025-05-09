using UnityEngine;
using static FishingManager;

public class BaitPreparationState : FishingState
{
    public BaitPreparationState(FishingManager fishingManager) : base(fishingManager) { }

    private int _currentStep = 0;

    public override void Enter()
    {
        // Only go back to start if need more fish, otherwise cam may mess up
        if (GameManager.Instance.FishCaught < GameManager.Instance.FishTotalToCatch)
            CameraController.Instance.BaitPrepVCam.Priority = 5; // Set camera priority for bait preparation

        fishingManager.StateLabelPanel.SetLabel(FishingStateName.BaitPreparation);
        _currentStep = 0; // Reset step counter
        fishingManager.ShowInputPrompt(fishingManager.BaitPrepPromptRightName);
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
            fishingManager.ShowInputPrompt(fishingManager.BaitPrepPromptLeftName);
            //Debug.Log(_currentStep);
            _currentStep++;
        }
        else if (_currentStep % 2 != 0 && fishingManager.InputHelper.HasReachedRotationY(fishingManager.RollLeftAngle))
        {
            fishingManager.ShowInputPrompt(fishingManager.BaitPrepPromptRightName);
            //Debug.Log(_currentStep);
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
        CameraController.Instance.BaitPrepVCam.Priority = 0; // Set camera priority for bait preparation
        Debug.Log("Exiting Bait Preparation State");
    }
}