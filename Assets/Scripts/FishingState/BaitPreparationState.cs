using UnityEngine;
using static FishingManager;

public class BaitPreparationState : FishingState
{
    public BaitPreparationState(FishingManager fishingManager) : base(fishingManager) { }

    private int _currentStep = 0;

    public override void Enter()
    {
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.BaitPreparation);
        _currentStep = 0; // Reset step counter
        fishingManager.ShowInputPrompt("ControllerFullRight");
        Debug.Log("Entering Bait Preparation State");
    }

    public override void Update()
    {
        // Alternate directions, even (and start) directions go upward
        if (_currentStep % 2 == 0 && fishingManager.InputHelper.IsNearRotation(
            -90f, InputDeviceManager.RotationAxis.x))
        {
            fishingManager.ShowInputPrompt("ControllerFullLeft");
            //Debug.Log(_currentStep);
            _currentStep++;
        }
        else if (_currentStep % 2 != 0 && fishingManager.InputHelper.IsNearRotation(
            90f, InputDeviceManager.RotationAxis.x))
        {
            fishingManager.ShowInputPrompt("ControllerFullRight");
            //Debug.Log(_currentStep);
            _currentStep++;
        }
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
        Debug.Log("Exiting Bait Preparation State");
    }
}