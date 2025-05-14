using UnityEngine;

public class GameStartGameState : GameState
{
    public GameStartGameState(GameManager gameManager) : base(gameManager) { }

    private float _gameStartTimer = 0;

    public override void Enter()
    {
        //Debug.Log("Entering Game Start State");

        _gameStartTimer = 0;
        gameManager.ResetFish();
        gameManager.Timer = 0f;

        // Reset input prompts
        UIManager.Instance.ShowMainInputPrompt(null as InputPrompt);
        UIManager.Instance.ShowSecondInputPrompt(null as InputPrompt);

        //gameManager.GameStarting.Invoke(0);
    }

    public override void Update()
    {
        _gameStartTimer += Time.deltaTime;
        //Debug.Log(_gameStartTimer);
        // Must be in this order or lowest always triggers
        //if (_gameStartTimer >= gameManager.GameStartDuration)
        //{
        //    gameManager.TransitionToState(gameManager.PlayingState);
        //}
        //else if (_gameStartTimer >= gameManager.GameStartDuration * 2 / 3)
        //{
        //    gameManager.GameStarting.Invoke(2);
        //}
        //else if (_gameStartTimer >= gameManager.GameStartDuration / 3)
        //{
        //    gameManager.GameStarting.Invoke(1);
        //}
    }

    public override void Exit()
    {
        Debug.Log("Exiting Game Start State");
    }
}
