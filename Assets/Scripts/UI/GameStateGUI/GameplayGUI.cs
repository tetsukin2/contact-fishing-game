using TMPro;
using UnityEngine;

/// <summary>
/// Overall GUI for main gameplay
/// </summary>
public class GameplayGUI : GUIContainer
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _fishCaughtNumberText;

    private void Start()
    {
        GameManager.Instance.GameStateEntered.AddListener(OnGameStateEntered);
        GameManager.Instance.FishCaughtUpdated.AddListener(OnFishCaughtUpdated);

        // Initialize the fish caught number text
        _fishCaughtNumberText.text = $"{GameManager.Instance.FishCaught}/{GameManager.Instance.FishTotalToCatch}";
    }

    private void Update()
    {
        //only run if timer object exists
        if (_timerText)
            _timerText.text = GameDataHandler.ConvertToTimeFormat(GameManager.Instance.Timer);
    }

    // Game Start UI Setup
    private void OnGameStateEntered(GameState newState)
    {
        Show(newState == GameManager.Instance.PlayingState || newState == GameManager.Instance.GameEndState);
    }

    // Update the fish caught number text
    private void OnFishCaughtUpdated(int caught)
    {
        _fishCaughtNumberText.text = $"{caught}/{GameManager.Instance.FishTotalToCatch}";
    }
}
