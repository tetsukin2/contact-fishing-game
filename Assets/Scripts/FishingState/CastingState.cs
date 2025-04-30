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
                fishingManager.TransitionToState(fishingManager.WaitingForBiteState);
                BraillePatternPlayer.Instance.StopPatternSequence(BraillePatternPlayer.Finger.BOTH);
                _hasCast = false;
            }});
    }

    public override void Enter()
    {
        _currentCastSteps = 0;
        _hasCast = false;
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.Casting);
        fishingManager.InputHelper.ClearRotationHistory(); // Clean read for casting
        fishingManager.ShowInputPrompt(fishingManager.CastBackPromptName);
        Debug.Log("Entering Casting State");
    }

    public override void Update()
    {
        if (_hasCast) // Don't do any more of this stuff if line alreaddy cast
            return;

        // Cast back
        if (!_hasCastBack 
            && fishingManager.InputHelper.HasReachedRotationY(fishingManager.RotateUpAngle))
        {
            _hasCastBack = true;
            fishingManager.ShowInputPrompt(fishingManager.CastForwardPromptName);
        }
        // Cast forward
        else if (_hasCastBack 
            && fishingManager.InputHelper.HasReachedRotationY(fishingManager.RotateDownAngle))
        {
            // Reset for return to cast forward
            _hasCastBack = false;
            fishingManager.ShowInputPrompt("");
            _currentCastSteps++;
            if (_currentCastSteps >= fishingManager.CastSteps) // cast proper if steps reached
            {
                _hasCast = true;
                _currentCastSteps = 0;
                fishingManager.CastLine();
                BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", BraillePatternPlayer.Finger.THUMB, true);
                //BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", BraillePatternPlayer.Finger.INDEX, true);
            }
            else // Update prompt otherwise
            {
                fishingManager.ShowInputPrompt(fishingManager.CastBackPromptName);
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}