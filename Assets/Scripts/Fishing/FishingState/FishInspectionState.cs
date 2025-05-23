using UnityEngine;
using UnityEngine.Events;

public class FishInspectionState : FishingState
{
    public FishInspectionState(FishingManager fishingManager) : base(fishingManager) { }

    private bool _reachedInitialRotation = false; // Rotation is weird on this axis, so we track an initial position first
    private bool _fishInspected = false; // FishData needs to be picked up first

    public UnityEvent FishInspected { get; private set; } = new();

    public override void Enter()
    {
        CameraController.Instance.SetCameraView(CameraController.CameraView.Gameplay);

        // Reset flags
        _reachedInitialRotation = false;
        _fishInspected = false;

        // Set UI
        fishingManager.StateLabelPanel.SetLabel(FishingManager.FishingStateName.FishInspection);
        UIManager.Instance.ShowMainInputPrompt(fishingManager.InspectReadyPromptName);

        // Fish visibility
        fishingManager.HookedFish.SetActive(true); // Show the fish in the inspection panel

        // Input reset
        fishingManager.InputHelper.ClearRotationHistory();
    }

    public override void Update()
    {
        if (!_fishInspected)
        {
            if (!_reachedInitialRotation && // Start at side neutral
                fishingManager.InputHelper.HasReachedRotationY(fishingManager.RollLeftAngle))
            {
                _reachedInitialRotation = true;
                UIManager.Instance.ShowMainInputPrompt(fishingManager.InspectPromptName);
            }
            else if (_reachedInitialRotation && // Now rotate up
                InputDeviceRotationHelper.IsLessThanRotation(InputDeviceManager.Instance.IMUInput.Rotation.x, 0.4f)
                && InputDeviceRotationHelper.IsLessThanRotation(InputDeviceManager.Instance.IMUInput.Rotation.y, 0.5f)
                && fishingManager.InputHelper.HasReachedRotationZ(-1.25f))
            {
                _reachedInitialRotation = false; // Reset for release rotation
                FishingManager.OnFishInspection();
                UIManager.Instance.ShowMainInputPrompt(fishingManager.ReleaseReadyPromptName);
                _fishInspected = true;
            }
        }
        else if (_fishInspected)
        {
            if (!_reachedInitialRotation && // Return to neutral
                fishingManager.InputHelper.HasReachedRotationY(fishingManager.RollLeftAngle))
            {
                _reachedInitialRotation = true;
                UIManager.Instance.ShowMainInputPrompt(fishingManager.ReleasePromptName);
            }
            else if (_reachedInitialRotation && // Now rotate down
                fishingManager.InputHelper.HasReachedRotationX(1f)
                && InputDeviceRotationHelper.IsLessThanRotation(InputDeviceManager.Instance.IMUInput.Rotation.y, 0.75f)
                && fishingManager.InputHelper.HasReachedRotationZ(0f))
            {
                GameManager.Instance.AddFish();
                BraillePatternPlayer.Instance.PlayPatternSequence("BasicPulse", false);
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