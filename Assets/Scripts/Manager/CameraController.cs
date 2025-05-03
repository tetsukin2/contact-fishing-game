using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _gameplayVCam;
    [SerializeField] CinemachineVirtualCamera _menuVCam;

    private void Start()
    {
        GameManager.Instance.GameStateUpdated.AddListener(OnGameStateUpdated);
    }

    private void OnGameStateUpdated(GameState newState)
    {
        if (newState == GameManager.Instance.MainMenuState
            || newState == GameManager.Instance.EndScoreState
            || newState == GameManager.Instance.EncyclopediaState)
        {
            _gameplayVCam.Priority = 0;
            _menuVCam.Priority = 10;
        }
        else
        {
            _gameplayVCam.Priority = 10;
            _menuVCam.Priority = 0;
        }
    }
}
