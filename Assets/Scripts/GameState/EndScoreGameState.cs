using UnityEngine;

public class EndScoreGameState : GameState
{
    public EndScoreGameState(GameManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        CameraController.Instance.SetCameraView(CameraController.CameraView.Menu);
        Debug.Log("Entering End Score State");
        gameManager.ProcessScore();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        Debug.Log("Exiting End Score State");
    }
}
