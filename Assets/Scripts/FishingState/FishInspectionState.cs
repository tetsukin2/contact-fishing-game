using UnityEngine;
using static FishingManager;

public class FishInspectionState : FishingState
{
    public FishInspectionState(FishingManager fishingManager) : base(fishingManager) { }

    private bool _fishInspected = false; // Fish needs to be picked up first

    public override void Enter()
    {
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.FishInspection);
        fishingManager.InputHelper.ClearRotationHistory();
        fishingManager.ShowInputPrompt("ControllerSideUp");
        fishingManager.HookedFish.SetActive(true); // Show the fish in the inspection panel
        _fishInspected = false;
        Debug.Log("Entering Fish Inspection State");
    }

    public override void Update()
    {
        // Not yet inspected, read a positive y and negative x value
        if (!_fishInspected
            && fishingManager.InputHelper.HasRotatedByDegrees(
            fishingManager.RotationTriggerThreshold, InputDeviceManager.RotationAxis.y)
            && fishingManager.InputHelper.HasRotatedByDegrees(
            fishingManager.RotationTriggerThreshold, InputDeviceManager.RotationAxis.x)
            )
        {
            fishingManager.InputHelper.ClearRotationHistory(); // Clean read for fish release
            FishingManager.ShowFishInspection(FishLootTable.Instance.GetFishFromTable());
            fishingManager.ShowInputPrompt("ControllerSideDown");
            fishingManager.HookedFish.SetActive(false); // Show the fish in the inspection panel
            _fishInspected = true;
            Debug.Log("Fish inspected");
        }
        // Already inspected, read a negative y and positive x value
        else if (fishingManager.InputHelper.HasRotatedByDegrees(
            -fishingManager.RotationTriggerThreshold, InputDeviceManager.RotationAxis.y)
            && fishingManager.InputHelper.HasRotatedByDegrees(
            -fishingManager.RotationTriggerThreshold, InputDeviceManager.RotationAxis.x))
        {
            Debug.Log("Fish released");
            fishingManager.TransitionToState(fishingManager.BaitPreparationState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Fish Inspection State");
        fishingManager.HideFishInspection();

    }
}