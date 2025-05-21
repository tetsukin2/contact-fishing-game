using System.Collections;
using System.Collections.Generic;
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

        GameManager.Instance.GameStateEntered.AddListener(HandleGameStateGUI);

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

    private void HandleGameStateGUI(GameState gameState)
    {
        // Show the appropriate GUI based on the game state
        _gameStartGUI.Show(gameState == GameManager.Instance.GameStartState);
        _gameplayGUI.Show(gameState == GameManager.Instance.PlayingState);
        _gameEndGUI.Show(gameState == GameManager.Instance.GameEndState);
        _endScoreGUI.Show(gameState == GameManager.Instance.EndScoreState);
    }
}
