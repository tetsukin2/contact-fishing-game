using UnityEngine;

public class EndScoreGameState : GameState
{
    public EndScoreGameState(GameManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        Debug.Log("Entering End Score State");
        // NewBestScore I think relies on logic from after saving data
        if (gameManager.Timer < gameManager.CurrentGameData.BestTime)
        {
            gameManager.CurrentGameData.BestTime = gameManager.Timer;
            GameDataHandler.SaveGameData(gameManager.CurrentGameData, "data", $"{gameManager.FishTotalToCatch}");
            gameManager.NewBestScoreReached.Invoke();
        }
        else // In order to save discovered fishes
        {
            GameDataHandler.SaveGameData(gameManager.CurrentGameData, "data", $"{gameManager.FishTotalToCatch}");
        }
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        Debug.Log("Exiting End Score State");
    }
}
