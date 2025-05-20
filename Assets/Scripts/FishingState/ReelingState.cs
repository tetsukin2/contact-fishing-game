using UnityEngine;
using static FishingManager;

/// <summary>
/// The fish is struggling
/// </summary>
public class ReelingState : FishingState
{
    [System.Serializable]
    public enum ReelActionName
    {
        RotateUp,
        JoystickClockwise
    } // Mapping reel action classes to enum values

    public ReelingState(FishingManager fishingManager) : base(fishingManager) { }

    // Reel Actions
    private RotateVerticalReelAction _rotateUpReelAction;
    private JoystickClockwiseReelAction _joystickClockwiseReelAction;

    // Reel Action State
    private int _currentReelActionIndex; // Current Action in sequence array
    private ReelAction _currentReelActionState;
    
    // References
    private ReelProgressBar _progressBar;

    public override void Setup()
    {
        // References
        _progressBar = FishingManager.Instance.ReelProgressBar;

        // Initialize reel actions
        _rotateUpReelAction = new(this);
        _joystickClockwiseReelAction = new(this);

        _progressBar.ReelProgressed.AddListener(OnReelProgressed);
        _progressBar.ReelCompleted.AddListener(OnReelCompleted);
    }

    public override void Enter()
    {
        //Debug.Log("Entering Reeling State");
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.Reeling);
        _progressBar.StartReel();
        BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", true);

        // Set up reel progress and sequence
        _currentReelActionIndex = 0;
        SetReelAction(fishingManager.ReelActionSequence[_currentReelActionIndex]);
    }

    public override void Update()
    {
        // Update the current reel acction
        _currentReelActionState?.Update();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Reeling State");
    }

    private void OnReelCompleted()
    {
        fishingManager.ReelIn(); // Call the reel in function
        fishingManager.Targeting.CatchSelected(); // Catch the fish and do resets
        BraillePatternPlayer.Instance.PlayPatternSequence("Ripple", false);

        // why?
        //if (GameManager.Instance.FishCaught < GameManager.Instance.FishTotalToCatch)
        //    fishingManager.TransitionToState(fishingManager.FishInspectionState);
    }

    private void OnReelProgressed()
    {
        _currentReelActionIndex = (_currentReelActionIndex + 1) % fishingManager.ReelActionSequence.Count;
        SetReelAction(fishingManager.ReelActionSequence[_currentReelActionIndex]);
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