using UnityEngine;
using UnityEngine.Events;

public class FishInspectionState : IFishingState
{
    private bool _reachedInitialRotation = false; // Rotation is weird on this axis, so we track an initial position first
    private bool _fishInspected = false; // FishData needs to be picked up first

    public UnityEvent FishInspected { get; private set; } = new();

    public void Setup() { }

    public void Enter()
    {
        var fishingManager = FishingManager.Instance;

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
        InputDeviceManager.Instance.RotationHelper.ClearRotationHistory();
    }

    public void Update()
    {
        if (!_fishInspected)
        {
            FishNotInspectedCheck();
        }
        else if (_fishInspected)
        {
            FishInspectedCheck();
        }
    }

    // Checks while the fish is not inspected
    private void FishNotInspectedCheck()
    {
        if (!_reachedInitialRotation && // Start at side neutral
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(FishingManager.Instance.RollLeftAngle))
        {
            _reachedInitialRotation = true;
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.InspectPromptName);
        }
        else if (_reachedInitialRotation && // Now rotate up
            InputDeviceRotationHelper.IsLessThanRotation(InputDeviceManager.Instance.IMUInput.Rotation.x, 0.4f)
            && InputDeviceRotationHelper.IsLessThanRotation(InputDeviceManager.Instance.IMUInput.Rotation.y, 0.5f)
            && InputDeviceManager.Instance.RotationHelper.HasReachedRotationZ(-1.25f))
        {
            _reachedInitialRotation = false; // Reset for release rotation
            FishingManager.Instance.OnFishInspection();
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReleaseReadyPromptName);
            _fishInspected = true;
        }
    }

    // Checks while the fish has already been inspected
    private void FishInspectedCheck()
    {
        if (!_reachedInitialRotation && // Return to neutral
                InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(FishingManager.Instance.RollLeftAngle))
        {
            _reachedInitialRotation = true;
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.ReleasePromptName);
        }
        else if (_reachedInitialRotation && // Now rotate down
            InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(1f)
            && InputDeviceRotationHelper.IsLessThanRotation(InputDeviceManager.Instance.IMUInput.Rotation.y, 0.75f)
            && InputDeviceManager.Instance.RotationHelper.HasReachedRotationZ(0f))
        {
            HandleFishAdding();
        }
    }

    // What happens when a fish is to be added to the score after inspection
    private void HandleFishAdding()
    {
        GameManager.Instance.AddFish();
        BraillePatternPlayer.Instance.PlayPatternSequence("BasicPulse", false);
        if (GameManager.Instance.FishCaught < GameManager.Instance.FishTotalToCatch)
        {
            // If not all fish caught, return to bait preparation
            FishingManager.Instance.TransitionToState(FishingManager.Instance.BaitPreparationState);
        }
        else
        {
            // If all fish caught, move to game end
            FishingManager.Instance.TransitionToState(FishingManager.Instance.IdleFishingState);
            GameManager.Instance.TransitionToState(GameManager.Instance.GameEndState);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting FishData Inspection State");
        FishingManager.Instance.HideFishInspection();

    }
}