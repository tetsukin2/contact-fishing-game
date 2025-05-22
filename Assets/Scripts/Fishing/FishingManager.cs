using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// FishingManager with class-based state machine
public class FishingManager : StaticInstance<FishingManager>
{
    public enum FishingStateName
    {
        Idle,
        BaitPreparation,
        Casting,
        WaitingForBite,
        Reeling,
        FishInspection
    }

    [SerializeField] private FishTargeting _fishTargeting;

    [Space]
    [Header("Fishing Rod")]
    public float RotationTriggerThreshold = 15f;  // Rotation threshold in degrees
    public float BobberSensitivity = 1f;
    [SerializeField] private FishingBobber _fishingBobber;
    [SerializeField] private Transform _bobberLandTransform;
    [SerializeField] private InputDeviceRotationHelper _inputHelper;
    [SerializeField] private FishingRodMovement _rodMovement;
    public float RotateUpAngle = 30f;
    public float RotateDownAngle = -30f; // Y rod rotation thresholds

    [Space]
    [Header("UI")]
    [SerializeField] private FishingStateLabelPanel _stateLabelPanel; // Reference to the state label panel

    [Space]
    [Header("Bait Preparation")]
    public int BaitPreparationSteps = 1;
    public string BaitPrepPromptRightName;
    public string BaitPrepPromptLeftName;
    public float RollRightAngle = -0.8f;
    public float RollLeftAngle = 0.8f;

    [Space]
    [Header("Casting")]
    public float CastForce = 10f;  // Adjust casting strength
    public int CastSteps = 1;
    public float CastDuration = 1f; // From bobber release to landing
    public float CastHeight = 2f; // Additional height for the arc from bobber release point
    public string CastForwardPromptName;
    public string CastBackPromptName;
    public string CastSelectPromptName;

    [Space]
    [Header("Reeling")]
    public float ReelTotalProgress = 20;
    public float ReelProgressAmount = 5;
    public string ReelForwardPromptName;
    public string ReelBackPromptName;
    public string ReelClockwisePromptName;
    public float ReelForce = 1f; // Force applied to the bobber upward
    public float ReelDecayRate = 0.3f;
    [SerializeField] private ReelProgressBar _reelProgressBar;
    public List<ReelingState.ReelActionName> ReelActionSequence; //Sequence of actions to follow

    [Space]
    [Header("FishData Inspection")]
    //public float SideRotateUpAngle = 30f;
    //public float SideRotateDownAngle = -30f; // Y rod rotation thresholds
    public string InspectReadyPromptName;
    public string InspectPromptName;
    public string ReleaseReadyPromptName;
    public string ReleasePromptName;
    [SerializeField] private FishInspectionPanel _fishInspectionGUI;
    [SerializeField] private GameObject _hookedFish; // show and hide in inspection

    private FishingState _currentState;
    private FishData _caughtFish;

    // State change events
    public UnityEvent<FishingState> FishingStateExited { get; private set; } = new();
    public UnityEvent<FishingState> FishingStateEntered { get; private set; } = new();

    // State Accessors
    public BaitPreparationState BaitPreparationState { get; private set; }
    public CastingState CastingState { get; private set; }
    public WaitingForBiteState WaitingForBiteState { get; private set; }
    public ReelingState ReelingState { get; private set; }
    public FishInspectionState FishInspectionState { get; private set; }

    // Some exposed properties
    public ReelProgressBar ReelProgressBar => _reelProgressBar;
    public FishTargeting Targeting => _fishTargeting;
    public FishingBobber FishingBobber => _fishingBobber;
    public InputDeviceRotationHelper InputHelper => _inputHelper;
    public FishingRodMovement RodMovement => _rodMovement;
    public GameObject HookedFish => _hookedFish;
    public FishingStateLabelPanel StateLabelPanel => _stateLabelPanel;

    protected override void OnAwake()
    {
        // Initialize states
        BaitPreparationState = new(this);
        CastingState = new(this);
        WaitingForBiteState = new(this);
        ReelingState = new(this);
        FishInspectionState = new(this);
    }

    protected override void OnRegister()
    {
        // Setup states, these are in start to ensure all references are set
        BaitPreparationState.Setup();
        CastingState.Setup();
        WaitingForBiteState.Setup();
        ReelingState.Setup();
        FishInspectionState.Setup();

        // Only start bait prep after gameplay actually starts
        GameManager.Instance.GameStateEntered.AddListener((state) =>
        {
            if (state == GameManager.Instance.PlayingState)
            {
                TransitionToState(BaitPreparationState);
            }
        });        
    }

    protected override void OnSetup()
    {
        _reelProgressBar.StopReel(); // Hide the reel GUI at the start
        _hookedFish.SetActive(false);
    }

    void Update()
    {
        // Only update if the game is in the playing state
        if (GameManager.Instance.CurrentState != GameManager.Instance.PlayingState) return;
        _currentState?.Update();
    }

    /// <summary>
    /// Handles fishing state transition
    /// </summary>
    /// <param name="newState">New fishing state to transition to</param>
    public void TransitionToState(FishingState newState)
    {
        _currentState?.Exit(); // Exit the current state
        FishingStateExited.Invoke(_currentState); // Notify listeners of the state exit
        _currentState = newState; // Set the new state
        _currentState?.Enter(); // Enter the new state
        FishingStateEntered.Invoke(_currentState); // Notify listeners of the state entry
    }

    public void ReelIn()
    {
        _fishingBobber.OnReel(ReelForce);

        _caughtFish = FishLootTable.Instance.GetFishFromTable();

        Debug.Log("Reeling In!");
    }

    public void OnFishInspection()
    {
        ShowFishInspection(); // Goes first so we can properly display if fish hasn't been discovered yet
        Debug.Log($"Has {_caughtFish.FishID} been discovered: {GameDataHandler.CurrentGameData.HasDiscoveredFish(_caughtFish.FishID)}");
        GameDataHandler.CurrentGameData.AddDiscoveredFish(_caughtFish.FishID);
        HookedFish.SetActive(false);
    }

    private void ShowFishInspection()
    {
        _fishInspectionGUI.Show(true);
        _fishInspectionGUI.ShowFish(_caughtFish);
        _fishInspectionGUI.ShowDiscoveredBanner(!GameDataHandler.CurrentGameData.HasDiscoveredFish(_caughtFish.FishID));
    }

    public void HideFishInspection()
    {
        _fishInspectionGUI.Show(false);
    }
}