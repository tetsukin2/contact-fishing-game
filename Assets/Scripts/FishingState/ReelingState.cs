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

    private int _currentReelActionIndex; // Current Action in sequence array
    
    // Defining all the reel actions
    private RotateVerticalReelAction _rotateUpReelAction;
    private JoystickClockwiseReelAction _joystickClockwiseReelAction;

    private ReelAction _currentReelActionState;
    private float _currentReelProgress;

    public override void Setup()
    {
        // Initialize reel actions
        _rotateUpReelAction = new(this);
        _joystickClockwiseReelAction = new(this);
    }

    public override void Enter()
    {
        fishingManager.StateLabelPanel.SetLabel(FishingStateName.Reeling);
        Debug.Log("Entering Reeling State");
        fishingManager.StartReel();
        BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", true); // Start the reel pattern

        // Set up reel progress and sequence
        _currentReelActionIndex = 0;
        SetReelAction(fishingManager.ReelActionSequence[_currentReelActionIndex]);

        _currentReelProgress = 0f;
    }

    public override void Update()
    {
        _currentReelActionState?.Update();
        _currentReelProgress -= Mathf.Max(0f, Time.deltaTime * fishingManager.ReelDecayRate); // Decay progress over time
        fishingManager.SetReelProgress(_currentReelProgress);
    }

    public override void Exit()
    {
        CameraController.Instance.FishSelectVCam.Priority = 0; // Reset camera priority
        Debug.Log("Exiting Reeling State");
        fishingManager.StopReel();
    }

    public void ProgressReel()
    {
        // Update reel progress
        _currentReelProgress += fishingManager.ReelProgressAmount;
        Debug.Log(_currentReelProgress);
        //fishingManager.SetReelProgress(_currentReelProgress);

        // Check if the reel progress is complete
        if (_currentReelProgress >= fishingManager.ReelTotalProgress)
        {
            Debug.Log("OnReel Progress Complete!");
            fishingManager.ReelIn(); // Call the reel in function
            fishingManager.Targeting.CatchSelected(); // Catch the fish and do resets
            // Transition back to bait prep only if not fully caught, so as not to mess camera
            BraillePatternPlayer.Instance.PlayPatternSequence("Ripple", false);
            if (GameManager.Instance.FishCaught < GameManager.Instance.FishTotalToCatch)
                fishingManager.TransitionToState(fishingManager.FishInspectionState); 
        }
        else
        {
            // Set the next reel action state
            _currentReelActionIndex = (_currentReelActionIndex + 1) % fishingManager.ReelActionSequence.Count;
            SetReelAction(fishingManager.ReelActionSequence[_currentReelActionIndex]);
        }
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