using UnityEngine;
using UnityEngine.Events;

public class EndScoreGameState : GameState
{
    public EndScoreGameState(GameManager gameManager) : base(gameManager) { }

    // Flag to check if a new best score was achieved
    // Prevents race condition at the start of the end score state
    public bool NewBestAchieved { get; private set; } = false;

    /// <summary>
    /// Invoked when the score is processed and game saved
    /// </summary>
    public UnityEvent ScoreProcessed { get; private set; } = new();

    public override void Enter()
    {
        CameraController.Instance.SetCameraView(CameraController.CameraView.Menu);
        Debug.Log("Entering End Score State");
        ProcessScore();
    }

    public override void Update()
    {

    }

    /// <summary>
    /// Saves game data and checks if a new best score was achieved. Also invokes the ScoreProcessed event.
    /// </summary>
    public void ProcessScore()
    {
        NewBestAchieved = GameDataHandler.CurrentGameData.TryAddNewBestTime(gameManager.LevelName, gameManager.FishTotalToCatch, gameManager.Timer);
        GameDataHandler.SaveGameData();
        ScoreProcessed.Invoke();
    }

    public override void Exit()
    {
        NewBestAchieved = false; // Resetting here, if in Enter it might be read before properly set?
        Debug.Log("Exiting End Score State");
    }
}
