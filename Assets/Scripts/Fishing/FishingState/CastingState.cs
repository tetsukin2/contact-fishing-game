using UnityEngine;
using UnityEngine.Events;
using static FishingManager;

public class CastingState : FishingState
{
    public CastingState(FishingManager fishingManager) : base(fishingManager) { }

    private int _currentCastSteps = 0;
    private bool _hasCastBack = false; // Flag to check if the cast back has been completed
    private bool _hasCast;

    /// <summary>
    /// Invoked when the line is cast
    /// </summary>
    public UnityEvent LineCast { get; private set; } = new(); 

    // Hook listener for bobber hitting water only once
    public override void Setup()
    {
        _hasCast = false;
        fishingManager.FishingBobber.BobberHitWater.AddListener(OnBobberHitWater);
    }

    public override void Enter()
    {
        // Fish selection setup
        CameraController.Instance.SetCameraView(CameraController.CameraView.FishSelect);
        fishingManager.Targeting.CanChangeSelection = true;
        fishingManager.Targeting.SetRandomFishAsSelected();

        // Cast tracking
        _currentCastSteps = 0;
        _hasCast = false;

        // Cast labels
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.Casting);
        UIManager.Instance.ShowMainInputPrompt(fishingManager.CastBackPromptName);
        UIManager.Instance.ShowSecondInputPrompt(fishingManager.CastSelectPromptName);

        InputDeviceManager.Instance.RotationHelper.ClearRotationHistory(); // Clean read for casting
        Debug.Log("Entering Casting State");
    }

    public override void Update()
    {
        if (_hasCast) // Don't do any more of this stuff if line alreaddy cast
            return;

        //Debug.Log(Mathf.Lerp(fishingManager.RotateUpAngle, 0f, Mathf.Abs(InputDeviceManager.Rotation.y)));

        if (!_hasCastBack 
            && InputDeviceRotationHelper.HasReachedRotation(
                Mathf.Lerp(InputDeviceManager.Instance.IMUInput.Rotation.z, 0f, Mathf.Abs(InputDeviceManager.Instance.IMUInput.Rotation.y)), 
                fishingManager.RotateUpAngle))
        {
            OnCastBack();
        }
        // OnCast forward
        else if (_hasCastBack 
            && InputDeviceRotationHelper.HasReachedRotation(
                Mathf.Lerp(InputDeviceManager.Instance.IMUInput.Rotation.z, 0f, Mathf.Abs(InputDeviceManager.Instance.IMUInput.Rotation.y)), 
                fishingManager.RotateDownAngle))
        {
            OnCastForward();
        }
    }

    private void OnCastBack()
    {
        _hasCastBack = true;
        UIManager.Instance.ShowMainInputPrompt(fishingManager.CastForwardPromptName);
    }

    private void OnCastForward()
    {
        fishingManager.Targeting.CanChangeSelection = false; // Disable fish selection while casting

        // Reset for return to cast forward
        _hasCastBack = false;
        _currentCastSteps++;
        if (_currentCastSteps >= fishingManager.CastSteps) // cast proper if steps reached
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
            UIManager.Instance.ShowMainInputPrompt(fishingManager.CastBackPromptName);
        }
    }

    private void OnBobberHitWater()
    {
        if (!_hasCast) return; // Flag as this object exists even when in another state

        fishingManager.Targeting.LureFish(); // Lure the fish
        fishingManager.TransitionToState(fishingManager.WaitingForBiteState);
        BraillePatternPlayer.Instance.PlayPatternSequence("Ripple", false);
        _hasCast = false;
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}