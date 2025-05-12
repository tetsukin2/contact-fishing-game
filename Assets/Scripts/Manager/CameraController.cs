using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] CinemachineVirtualCamera _gameplayVCam;
    [SerializeField] CinemachineVirtualCamera _menuVCam;
    [SerializeField] CinemachineVirtualCamera _fishSelectVCam;
    [SerializeField] CinemachineVirtualCamera _baitPrepVCam;

    public CinemachineVirtualCamera GameplayVCam => _gameplayVCam;
    public CinemachineVirtualCamera FishSelectVCam => _fishSelectVCam;
    public CinemachineVirtualCamera BaitPrepVCam => _baitPrepVCam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
            _menuVCam.Priority = 5;
        }
        else
        {
            _gameplayVCam.Priority = 5;
            _menuVCam.Priority = 0;
        }
        if (newState == GameManager.Instance.GameEndState)
        {
            _baitPrepVCam.Priority = 0;
            _fishSelectVCam.Priority = 0;
        }
    }
}
