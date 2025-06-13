using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Handles visibility for any pause screen elements.
/// </summary>
public class PauseUIController : StaticInstance<PauseUIController>
{
    [SerializeField] private MenuSelect _pauseMenuSelect;
    [SerializeField] private GameObject _menuFishingRod;
    [SerializeField] private GUIContainer _pauseGUI;
    [SerializeField] private Volume _pauseVolume;

    protected override void OnRegister()
    {
        LevelManager.Instance.GamePaused.AddListener(OnGamePaused);
    }

    private void OnGamePaused(bool isPaused)
    {
        CameraController.Instance.SetPriorityMenuView(isPaused);

        _pauseMenuSelect.Show(isPaused);
        _menuFishingRod.SetActive(isPaused);
        _pauseGUI.Show(isPaused);
        _pauseVolume.enabled = isPaused;
    }
}
