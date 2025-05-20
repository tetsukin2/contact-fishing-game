using UnityEngine;

public class GameEndGameState : GameState
{
    public GameEndGameState(GameManager gameManager) : base(gameManager) { }
    private float _gameEndTimer = 0f;

    public override void Enter()
    {
        CameraController.Instance.SetCameraView(CameraController.CameraView.Menu);
        Debug.Log("Entering Game End State");
    }

    public override void Update()
    {
        _gameEndTimer += Time.deltaTime;
        if (_gameEndTimer >= gameManager.GameEndDuration)
        {
            gameManager.TransitionToState(gameManager.EndScoreState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Game End State");
    }

    
}
