using Cinemachine;
using UnityEngine;

/// <summary>
/// Handles camera views
/// </summary>
public class CameraController : MonoBehaviour
{
    public enum CameraView
    {
        Gameplay,
        Menu,
        FishSelect,
        BaitPrep
    }

    public static CameraController Instance { get; private set; }

    [SerializeField] CinemachineVirtualCamera _gameplayVCam;
    [SerializeField] CinemachineVirtualCamera _menuVCam;
    [SerializeField] CinemachineVirtualCamera _fishSelectVCam;
    [SerializeField] CinemachineVirtualCamera _baitPrepVCam;

    public CinemachineVirtualCamera FishSelectVCam => _fishSelectVCam;

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

    /// <summary>
    /// Set the highest priority cinemachine
    /// </summary>
    /// <param name="view"></param>
    public void SetCameraView(CameraView view)
    {
        _gameplayVCam.Priority = 0;
        _menuVCam.Priority = 0;
        _fishSelectVCam.Priority = 0;
        _baitPrepVCam.Priority = 0;

        switch (view)
        {
            case CameraView.Gameplay:
                _gameplayVCam.Priority = 5;
                break;
            case CameraView.Menu:
                _menuVCam.Priority = 5;
                break;
            case CameraView.FishSelect:
                _fishSelectVCam.Priority = 5;
                break;
            case CameraView.BaitPrep:
                _baitPrepVCam.Priority = 5;
                break;
        }
    }
}
