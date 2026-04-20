using UnityEngine;

public class MainMenuSelect : MenuSelect
{
    private const string PLAY_ACTION = "Play";
    private const string ENCYCLOPEDIA_ACTION = "OpenEncyclopedia";
    private const string EXIT_ACTION = "Exit";

    private const string FIRST_LEVEL_NAME = "Stage1";

    [Header("Onboarding")]
    [SerializeField] private OnboardingPopupUI _onboardingPopup;

    protected override void Start()
    {
        base.Start();
        MainMenuUIController.Instance.ViewChanged.AddListener(HandleInputSubscription);
    }

    private void HandleInputSubscription(MainMenuUIController.MainMenuView newView)
    {
        if (newView == MainMenuUIController.MainMenuView.MainMenu)
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(OnOptionSelected);
        }
        else
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.RemoveListener(OnOptionSelected);
        }
    }

    protected override void OnOptionSelected()
    {
        // 🚫 IMPORTANT: block menu input if onboarding is open
        if (_onboardingPopup != null && _onboardingPopup.IsOpen)
            return;

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case PLAY_ACTION:
                HandlePlay();
                break;

            case ENCYCLOPEDIA_ACTION:
                MainMenuUIController.Instance.ChangeView(MainMenuUIController.MainMenuView.Encyclopedia);
                break;

            case EXIT_ACTION:
                LevelManager.Instance.QuitGame();
                break;

            default:
                Debug.LogWarning($"Unknown action: {_menuSelectOptions[_currentSelectionIndex].Action}");
                break;
        }
    }

    private void HandlePlay()
    {
        if (_onboardingPopup == null)
        {
            Debug.LogWarning("OnboardingPopup not assigned!");
            return;
        }

        // ✅ Show onboarding instead of loading scene
        _onboardingPopup.OpenPopup();
    }
}