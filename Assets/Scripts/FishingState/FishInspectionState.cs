using UnityEngine;
using static FishingManager;

public class FishInspectionState : FishingState
{
    public FishInspectionState(FishingManager fishingManager) : base(fishingManager) { }

    private bool _reachedInitialRotation = false; // Rotation is weird on this axis
    private bool _fishInspected = false; // FishData needs to be picked up first

    public override void Enter()
    {
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.FishInspection);
        fishingManager.InputHelper.ClearRotationHistory();
        fishingManager.ShowInputPrompt(fishingManager.InspectNeutralPromptName);
        fishingManager.HookedFish.SetActive(true); // Show the fish in the inspection panel
        _reachedInitialRotation = false;
        _fishInspected = false;
        Debug.Log("Entering FishData Inspection State");
    }

    public override void Update()
    {
        if (!_fishInspected)
        {
            if (!_reachedInitialRotation && // Start at side neutral
                fishingManager.InputHelper.HasReachedRotationX(80f))
            {
                _reachedInitialRotation = true;
                fishingManager.ShowInputPrompt(fishingManager.InspectPromptName);
            }
            else if (_reachedInitialRotation && // Now rotate up
                fishingManager.InputHelper.HasReachedRotationY(fishingManager.SideRotateUpAngle))
            {
                _reachedInitialRotation = false; // Reset for release rotation
                fishingManager.InputHelper.ClearRotationHistory(); // Clean read for fish release
                FishingManager.ShowFishInspection(FishLootTable.Instance.GetFishFromTable());
                fishingManager.ShowInputPrompt(fishingManager.InspectNeutralPromptName);
                fishingManager.HookedFish.SetActive(false); // Hide the fish, we caught it
                _fishInspected = true;
                Debug.Log("FishData inspected");
            }
        }
        else if (_fishInspected)
        {
            if (!_reachedInitialRotation && // Return to neutral
                fishingManager.InputHelper.HasReachedRotationX(80f))
            {
                _reachedInitialRotation = true;
                fishingManager.ShowInputPrompt(fishingManager.InspectReleasePromptName);
            }
            else if (_reachedInitialRotation && // Now rotate down
                fishingManager.InputHelper.HasReachedRotationY(fishingManager.SideRotateDownAngle))
            {
                Debug.Log("FishData caught");
                GameManager.Instance.AddFish();
                fishingManager.TransitionToState(fishingManager.BaitPreparationState);
            }
        
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting FishData Inspection State");
        fishingManager.HideFishInspection();

    }
}