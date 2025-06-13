using UnityEngine;

/// <summary>
/// The fish is struggling
/// </summary>
public class ReelingState : IFishingState
{
    [System.Serializable]
    public enum ReelActionName
    {
        RotateUp,
        JoystickClockwise
    } // Mapping reel action classes to enum values

    // Reel Actions
    private RotateVerticalReelAction _rotateUpReelAction = new();
    private JoystickClockwiseReelAction _joystickClockwiseReelAction = new();

    // Reel Action State
    private int _currentReelActionIndex; // Current Action in sequence array
    private IReelAction _currentReelActionState;
    
    // References
    private ReelProgressBar _progressBar;

    public void Setup()
    {
        // References
        _progressBar = FishingManager.Instance.ReelProgressBar;

        if (_progressBar == null)
        {
            Debug.LogError("ReelProgressBar is not set in the FishingManager.");
            return;
        }

        _progressBar.ReelProgressed.AddListener(OnReelProgressed);
        _progressBar.ReelCompleted.AddListener(OnReelCompleted);
    }

    public void Enter()
    {
        //Debug.Log("Entering Reeling State");
        FishingManager.Instance.StateLabelPanel.SetLabel(FishingManager.FishingStateName.Reeling);
        _progressBar.StartReel();
        BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", true);

        // Set up reel progress and sequence
        _currentReelActionIndex = 0;
        SetReelAction(FishingManager.Instance.ReelActionSequence[_currentReelActionIndex]);
    }

    public void Update()
    {
        // Update the current reel acction
        _currentReelActionState?.Update();
    }

    public void Exit()
    {
        Debug.Log("Exiting Reeling State");
    }

    private void OnReelCompleted()
    {
        var fishingManager = FishingManager.Instance;

        fishingManager.ReelIn(); // Call the reel in function
        fishingManager.Targeting.CatchSelected(); // Catch the fish and do resets
        BraillePatternPlayer.Instance.PlayPatternSequence("Ripple", false);
        fishingManager.TransitionToState(fishingManager.FishInspectionState);

        // why?
        //if (LevelManager.Instance.FishCaught < LevelManager.Instance.FishTotalToCatch)
        //    fishingManager.TransitionToState(fishingManager.FishInspectionState);
    }

    private void OnReelProgressed()
    {
        _currentReelActionIndex = (_currentReelActionIndex + 1) % FishingManager.Instance.ReelActionSequence.Count;
        SetReelAction(FishingManager.Instance.ReelActionSequence[_currentReelActionIndex]);
    }

    // OnReel state maching switching
    private void SetReelAction(ReelActionName newAction)
    {
        _currentReelActionState?.Exit();
        switch (newAction)
        {
            case ReelActionName.RotateUp:
                _currentReelActionState = _rotateUpReelAction;
                break;
            case ReelActionName.JoystickClockwise:
                _currentReelActionState = _joystickClockwiseReelAction;
                break;
            default:
                Debug.LogError("Undefined OnReel Action");
                return;
        }
        _currentReelActionState?.Enter();
    }
}