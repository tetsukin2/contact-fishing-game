using UnityEngine;

public class EndScoreGameState : GameState
{
    public EndScoreGameState(GameManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        Debug.Log("Entering End Score State");
        bool newBest = false;
        if (gameManager.Timer < gameManager.CurrentGameData.BestTime)
        {
            gameManager.CurrentGameData.BestTime = gameManager.Timer;
            GameDataHandler.SaveGameData(gameManager.CurrentGameData, "data", $"{gameManager.FishTotalToCatch}");
            newBest = true;
        }
        if (newBest) gameManager.NewBestScoreReached.Invoke();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        Debug.Log("Exiting End Score State");
    }
}
