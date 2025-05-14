using UnityEngine;
using UnityEngine.Events;

public class GameStartGameState : GameState
{
    public GameStartGameState(GameManager gameManager) : base(gameManager) { }

    private float _gameStartTimer = 0;
    private int _gameStage = 0;

    /// <summary>
    /// Invoked for each game start stage being reached. Passes the stage number as parameter.
    /// </summary>
    // Mostly for UI to listen to
    public UnityEvent<int> GameStartStageReached { get; private set; } = new();

    public override void Enter()
    {
        //Debug.Log("Entering Game Start State");

        // Game Manager resets
        gameManager.ResetFish();
        gameManager.Timer = 0f;

        // Reset input prompts
        UIManager.Instance.ShowMainInputPrompt(null as InputPrompt);
        UIManager.Instance.ShowSecondInputPrompt(null as InputPrompt);

        // Game start setup
        _gameStage = 0;
        _gameStartTimer = 0;
        GameStartStageReached.Invoke(0);
    }

    public override void Update()
    {
        _gameStartTimer += Time.deltaTime;
        //Must be in this order or lowest always triggers
        if (_gameStage >= 2 && _gameStartTimer >= gameManager.GameStartDuration)
        {
            gameManager.TransitionToState(gameManager.PlayingState);
        }
        else if (_gameStage < 2 && _gameStartTimer >= gameManager.GameStartDuration * 2 / 3)
        {
            GameStartStageReached.Invoke(2);
        }
        else if (_gameStage < 1 && _gameStartTimer >= gameManager.GameStartDuration / 3)
        {
            GameStartStageReached.Invoke(1);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Game Start State");
    }
}
