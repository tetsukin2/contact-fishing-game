using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : Singleton<UIManager>
{

    [Header("Loading Screen")]
    [SerializeField] private GUIContainer _loadingScreen;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Space]
    // Prompt list is out here for centralized access
    // idk if this is final
    [Header("Input Prompts")]
    [SerializeField] private InputPrompt _mainMenuInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _mainMenuSecondInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _encyclopediaInput;
    [SerializeField] private InputPrompt _encyclopediaSecondInput;

    [Space]
    [Header("Game State GUI")]
    // Major game "views" that are state dependent
    [SerializeField] private MainMenuGUI _mainMenuGUI;
    [SerializeField] private EncyclopediaGUI _encyclopediaGUI;
    [SerializeField] private GameStartGUI _gameStartGUI;
    [SerializeField] private GameplayGUI _gameplayGUI;
    [SerializeField] private GameEndGUI _gameEndGUI;
    [SerializeField] private EndScoreGUI _endScoreGUI;

    [Space]
    [SerializeField] private JoystickCursor _joystickCursor;

    // There are separate explicit input prompts because of how the video render textures work
    public UnityEvent<InputPrompt> MainInputPromptShown { get; private set; } = new();
    public UnityEvent<InputPrompt> SecondInputPromptShown { get; private set; } = new();

    // Input prompt accessors, doing it this way allows you to see references in Visual Studio
    public InputPrompt MainMenuInput => _mainMenuInput;
    public InputPrompt MainMenuSecondInput => _mainMenuSecondInput;
    public InputPrompt EncyclopediaInput => _encyclopediaInput;
    public InputPrompt EncyclopediaSecondInput => _encyclopediaSecondInput;

    public JoystickCursor JoystickCursor => _joystickCursor;

    protected override void OnRegister()
    {
        HideAllGUI();

        // Loading screen things
        _loadingScreen.Show(true);
        InputDeviceManager.Instance.ConnectionStatusLog.AddListener((string message) => _loadingText.SetText(message));
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => _loadingScreen.Show(false));

        GameManager.Instance.GameStateEntered.AddListener(HandleGameStateGUI);

        SetupFishingStateInputPromptListeners();
    }

    private void SetupFishingStateInputPromptListeners()
    {
        FishingManager fishingManager = FishingManager.Instance;

        // Bait Preparation
        fishingManager.FishInspectionState.FishInspected.AddListener(() => ShowMainInputPrompt(fishingManager.BaitPrepPromptLeftName));
    }

    private void HideAllGUI()
    {
        // Hide all GUI elements
        _mainMenuGUI.Show(false);
        _encyclopediaGUI.Show(false);
        _gameStartGUI.Show(false);
        _gameplayGUI.Show(false);
        _gameEndGUI.Show(false);
        _endScoreGUI.Show(false);
    }

    private void HandleGameStateGUI(GameState gameState)
    {
        // Show the appropriate GUI based on the game state
        _mainMenuGUI.Show(gameState == GameManager.Instance.MainMenuState);
        _encyclopediaGUI.Show(gameState == GameManager.Instance.EncyclopediaState);
        _gameStartGUI.Show(gameState == GameManager.Instance.GameStartState);
        _gameplayGUI.Show(gameState == GameManager.Instance.PlayingState);
        _gameEndGUI.Show(gameState == GameManager.Instance.GameEndState);
        _endScoreGUI.Show(gameState == GameManager.Instance.EndScoreState);
    }

    #region Input Prompts
    //Ideally, only handle prompt showing or hiding at the start or end of each game or fishing state.

    /// <summary>
    /// Show primary input prompt
    /// </summary>
    /// <param name="name">Input prompt to show. Values with no match are treated as null</param>
    public void ShowMainInputPrompt(string name)
    {
        Debug.Log($"Showing prompt {name}");
        MainInputPromptShown.Invoke(ResourceSystem.Instance.GetInputPrompt(name));
    }

    /// <summary>
    /// Show primary input prompt
    /// </summary>
    /// <param name="prompt">Input prompt to show. Pass null to hide the input prompt panel</param>
    public void ShowMainInputPrompt(InputPrompt prompt)
    {
        MainInputPromptShown.Invoke(prompt);
    }

    /// <summary>
    /// Show secondary input prompt
    /// </summary>
    /// <param name="name">Input prompt to show. Values with no match are treated as null</param>
    public void ShowSecondInputPrompt(string name)
    {
        Debug.Log($"2nd prompt {name}");
        SecondInputPromptShown.Invoke(ResourceSystem.Instance.GetInputPrompt(name));
    }

    /// <summary>
    /// Show secondary input prompt
    /// </summary>
    /// <param name="prompt">Input prompt to show. Pass null to hide the input prompt panel</param>
    public void ShowSecondInputPrompt(InputPrompt prompt)
    {
        SecondInputPromptShown.Invoke(prompt);
    }
    #endregion
}

