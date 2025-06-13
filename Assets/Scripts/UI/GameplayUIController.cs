using UnityEngine;

/// <summary>
/// Handles UI specific to gameplay.
/// </summary>
public class GameplayUIController : Singleton<GameplayUIController>
{
    [Space]
    [Header("Game State GUI")]
    [SerializeField] private GameStartGUI _gameStartGUI;
    [SerializeField] private GameplayGUI _gameplayGUI;
    [SerializeField] private GameEndGUI _gameEndGUI;
    [SerializeField] private EndScoreGUI _endScoreGUI;

    protected override void OnRegister()
    {
        HideAllGUI();

        LevelManager.Instance.GameStateEntered.AddListener(HandleGameStateGUI);
        LevelManager.Instance.GamePaused.AddListener(OnGamePaused);

        SetupFishingStateInputPromptListeners();
    }

    private void SetupFishingStateInputPromptListeners()
    {
        FishingManager fishingManager = FishingManager.Instance;

        // Bait Preparation
        fishingManager.FishInspectionState.FishInspected.AddListener(() => 
            UIManager.Instance.ShowMainInputPrompt(fishingManager.BaitPrepPromptLeftName));
    }

    private void HideAllGUI()
    {
        // Hide all GUI elements
        _gameStartGUI.Show(false);
        _gameplayGUI.Show(false);
        _gameEndGUI.Show(false);
        _endScoreGUI.Show(false);
    }

    // GUI visibility when pausing
    private void OnGamePaused(bool isPaused)
    {
        if 
            (isPaused) _gameplayGUI.Show(false);
        else // Double checking, juuuust in case
            _gameplayGUI.Show(LevelManager.Instance.CurrentState == LevelManager.Instance.PlayingState);
    }

    private void HandleGameStateGUI(LevelState gameState)
    {
        // Show the appropriate GUI based on the game state
        _gameStartGUI.Show(gameState == LevelManager.Instance.GameStartState);
        _gameplayGUI.Show(gameState == LevelManager.Instance.PlayingState);
        _gameEndGUI.Show(gameState == LevelManager.Instance.GameEndState);
        _endScoreGUI.Show(gameState == LevelManager.Instance.EndScoreState);
    }
}
