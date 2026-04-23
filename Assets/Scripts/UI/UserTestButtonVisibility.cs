using UnityEngine;

public class UserTestButtonVisibility : MonoBehaviour
{
    [SerializeField] private GameObject _userTestButton;
    [SerializeField] private MainMenuUIController _mainMenuUIController;

    private void Start()
    {
        if (_userTestButton != null)
            _userTestButton.SetActive(false);

        if (_mainMenuUIController != null)
        {
            _mainMenuUIController.ViewChanged.AddListener(OnViewChanged);
            OnViewChanged(_mainMenuUIController.CurrentView);
        }
    }

    private void OnViewChanged(MainMenuUIController.MainMenuView view)
    {
        if (_userTestButton == null) return;

        _userTestButton.SetActive(view == MainMenuUIController.MainMenuView.MainMenu);
    }

    private void OnDestroy()
    {
        if (_mainMenuUIController != null)
            _mainMenuUIController.ViewChanged.RemoveListener(OnViewChanged);
    }
}