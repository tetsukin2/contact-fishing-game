using UnityEngine;

public class GameEndGameState : GameState
{
    public GameEndGameState(GameManager gameManager) : base(gameManager) { }
    private float _gameEndTimer = 0f;

    public override void Enter()
    {
        _gameEndTimer = 0f; // Reset timer
        CameraController.Instance.SetCameraView(CameraController.CameraView.Gameplay);

        // Allow player to skip
        InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(EndState);
    }

    public override void Update()
    {
        _gameEndTimer += Time.deltaTime;
        if (_gameEndTimer >= gameManager.GameEndDuration)
        {
            EndState();
        }
    }

    // Ending the state either after the timer or when player skips
    private void EndState()
    {
        gameManager.TransitionToState(gameManager.EndScoreState);
    }

    public override void Exit()
    {
        InputDeviceManager.Instance.JoystickInput.JoystickPressed.RemoveListener(EndState);
        Debug.Log("Exiting Game End State");
    }
}
