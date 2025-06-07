using UnityEngine;
using UnityEngine.Events;

public class CastingState : IFishingState
{
    private int _currentCastSteps = 0;
    private bool _hasCastBack = false; // Flag to check if the cast back has been completed
    private bool _hasCast;

    /// <summary>
    /// Invoked when the line is cast
    /// </summary>
    public UnityEvent LineCast { get; private set; } = new(); 

    // Hook listener for bobber hitting water only once
    public void Setup()
    {
        _hasCast = false;
        FishingManager.Instance.FishingBobber.BobberHitWater.AddListener(OnBobberHitWater);
    }

    public void Enter()
    {
        var fishingManager = FishingManager.Instance;

        // Fish selection setup
        CameraController.Instance.SetCameraView(CameraController.CameraView.FishSelect);
        fishingManager.Targeting.CanChangeSelection = true;
        fishingManager.Targeting.SetRandomFishAsSelected();

        // Cast tracking
        _currentCastSteps = 0;
        _hasCast = false;

        // Cast labels
        fishingManager.StateLabelPanel.SetLabel(FishingManager.FishingStateName.Casting);
        UIManager.Instance.ShowMainInputPrompt(fishingManager.CastBackPromptName);
        UIManager.Instance.ShowSecondInputPrompt(fishingManager.CastSelectPromptName);

        InputDeviceManager.Instance.RotationHelper.ClearRotationHistory(); // Clean read for casting
        Debug.Log("Entering Casting State");
    }

    public void Update()
    {
        if (_hasCast) // Don't do any more of this stuff if line alreaddy cast
            return;

        //Debug.Log(Mathf.Lerp(fishingManager.RotateUpAngle, 0f, Mathf.Abs(InputDeviceManager.Rotation.y)));

        if (!_hasCastBack 
            && InputDeviceRotationHelper.HasReachedRotation(
                Mathf.Lerp(InputDeviceManager.Instance.IMUInput.Rotation.z, 0f, Mathf.Abs(InputDeviceManager.Instance.IMUInput.Rotation.y)),
                FishingManager.Instance.RotateUpAngle))
        {
            OnCastBack();
        }
        // OnCast forward
        else if (_hasCastBack 
            && InputDeviceRotationHelper.HasReachedRotation(
                Mathf.Lerp(InputDeviceManager.Instance.IMUInput.Rotation.z, 0f, Mathf.Abs(InputDeviceManager.Instance.IMUInput.Rotation.y)),
                FishingManager.Instance.RotateDownAngle))
        {
            OnCastForward();
        }
    }

    private void OnCastBack()
    {
        _hasCastBack = true;
        UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.CastForwardPromptName);
    }

    private void OnCastForward()
    {
        FishingManager.Instance.Targeting.CanChangeSelection = false; // Disable fish selection while casting

        // Reset for return to cast forward
        _hasCastBack = false;
        _currentCastSteps++;
        if (_currentCastSteps >= FishingManager.Instance.CastSteps) // cast proper if steps reached
        {
            _hasCast = true;
            _currentCastSteps = 0;
            LineCast.Invoke();
            UIManager.Instance.ShowMainInputPrompt(null as InputPrompt);
            UIManager.Instance.ShowSecondInputPrompt(null as InputPrompt);
            BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", true);
        }
        else // Update prompt otherwise
        {
            UIManager.Instance.ShowMainInputPrompt(FishingManager.Instance.CastBackPromptName);
        }
    }

    private void OnBobberHitWater()
    {
        if (!_hasCast) return; // Flag as this object exists even when in another state

        FishingManager.Instance.Targeting.LureFish(); // Lure the fish
        FishingManager.Instance.TransitionToState(FishingManager.Instance.WaitingForBiteState);
        BraillePatternPlayer.Instance.PlayPatternSequence("Ripple", false);
        _hasCast = false;
    }

    public void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}