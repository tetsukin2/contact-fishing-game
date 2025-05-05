using UnityEngine;
using static FishingManager;

public class CastingState : FishingState
{
    public CastingState(FishingManager fishingManager) : base(fishingManager) { }

    private int _currentCastSteps = 0;
    private bool _hasCastBack = false; // Flag to check if the cast back has been completed
    private bool _hasCast;

    // Hook listener for bobber hitting water only once
    public override void Setup()
    {
        _hasCast = false;
        fishingManager.BobberHitWater.AddListener(() => {
            if (_hasCast) // Flag as this object exists even when in another state
            {
                fishingManager.Targeting.LureFish(); // Lure the fish
                fishingManager.TransitionToState(fishingManager.WaitingForBiteState);
                BraillePatternPlayer.Instance.StopPatternSequence();
                _hasCast = false;
            }});
    }

    public override void Enter()
    {
        // Fish selection setup
        CameraController.Instance.FishSelectVCam.Priority = 10;
        fishingManager.Targeting.CanChangeSelection = true;
        fishingManager.Targeting.SetRandomFishAsSelected();

        // Cast tracking
        _currentCastSteps = 0;
        _hasCast = false;

        // Cast labels
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.Casting);
        fishingManager.ShowInputPrompt(fishingManager.CastBackPromptName);
        fishingManager.ShowSecondInputPrompt(fishingManager.CastSelectPromptName);

        fishingManager.InputHelper.ClearRotationHistory(); // Clean read for casting
        Debug.Log("Entering Casting State");
    }

    public override void Update()
    {
        if (_hasCast) // Don't do any more of this stuff if line alreaddy cast
            return;

        if (!_hasCastBack 
            && fishingManager.InputHelper.HasReachedRotationY(fishingManager.RotateUpAngle))
        {
            OnCastBack();
        }
        // OnCast forward
        else if (_hasCastBack 
            && fishingManager.InputHelper.HasReachedRotationY(fishingManager.RotateDownAngle))
        {
            OnCastForward();
        }
    }

    private void OnCastBack()
    {
        _hasCastBack = true;
        fishingManager.ShowInputPrompt(fishingManager.CastForwardPromptName);
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
            fishingManager.CastLine();
            fishingManager.ShowInputPrompt("");
            fishingManager.ShowSecondInputPrompt("");
            BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", true);
            //BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", BraillePatternPlayer.Finger.INDEX, true);
        }
        else // Update prompt otherwise
        {
            fishingManager.ShowInputPrompt(fishingManager.CastBackPromptName);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}