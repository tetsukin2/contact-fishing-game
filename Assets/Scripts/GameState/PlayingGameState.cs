using UnityEngine;

public class PlayingGameState : GameState
{
    public PlayingGameState(GameManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Playing State");
    }

    public override void Update()
    {
        gameManager.Timer += Time.deltaTime;

        // Testing/Cheat
        if (Input.GetKeyDown(KeyCode.E))
        {
            gameManager.AddFish();
            Debug.Log("Debug: Adding FishData");
        }
        
    }

    public override void Exit()
    {
        Debug.Log("Exiting Playing State");
    }
}