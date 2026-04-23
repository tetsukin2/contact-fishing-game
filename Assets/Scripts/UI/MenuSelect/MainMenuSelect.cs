using UnityEngine;

public class MainMenuSelect : MenuSelect
{
    private const string PLAY_ACTION = "Play";
    private const string USER_TEST_ACTION = "UserTest";
    private const string ENCYCLOPEDIA_ACTION = "OpenEncyclopedia";
    private const string EXIT_ACTION = "Exit";

    [Header("Onboarding")]
    [SerializeField] private OnboardingPopupUI _onboardingPopup;

    protected override void Start()
    {
        base.Start();

        MainMenuUIController.Instance.ViewChanged.AddListener(HandleInputSubscription);
        HandleInputSubscription(MainMenuUIController.Instance.CurrentView);
    }

    private void OnDestroy()
    {
        if (MainMenuUIController.Instance != null)
        {
            MainMenuUIController.Instance.ViewChanged.RemoveListener(HandleInputSubscription);
        }

        if (InputDeviceManager.Instance != null && InputDeviceManager.Instance.JoystickInput != null)
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.RemoveListener(OnOptionSelected);
        }
    }

    private void HandleInputSubscription(MainMenuUIController.MainMenuView newView)
    {
        if (InputDeviceManager.Instance == null || InputDeviceManager.Instance.JoystickInput == null)
            return;

        InputDeviceManager.Instance.JoystickInput.JoystickPressed.RemoveListener(OnOptionSelected);

        if (newView == MainMenuUIController.MainMenuView.MainMenu)
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(OnOptionSelected);
        }
    }

    protected override bool IsSelectionActive()
    {
        if (MainMenuUIController.Instance == null)
            return false;

        if (MainMenuUIController.Instance.CurrentView != MainMenuUIController.MainMenuView.MainMenu)
            return false;

        if (_onboardingPopup != null && _onboardingPopup.IsOpen)
            return false;

        return true;
    }

    protected override bool ShouldPlayMoveSfx()
    {
        return IsSelectionActive();
    }

    protected override void OnOptionSelected()
    {
        Debug.Log("MainMenuSelect.OnOptionSelected fired");

        if (_onboardingPopup != null && _onboardingPopup.IsOpen)
        {
            Debug.Log("Onboarding already open, ignoring menu select");
            return;
        }

        AudioManager.Instance?.PlaySelect();

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case PLAY_ACTION:
                Debug.Log("PLAY selected");
                HandlePlay();
                break;

            case USER_TEST_ACTION:
                Debug.Log("USER TEST selected");
                HandleUserTest();
                break;

            case ENCYCLOPEDIA_ACTION:
                Debug.Log("ENCYCLOPEDIA selected");
                MainMenuUIController.Instance.ChangeView(MainMenuUIController.MainMenuView.Encyclopedia);
                break;

            case EXIT_ACTION:
                Debug.Log("EXIT selected");
                LevelManager.Instance.QuitGame();
                break;

            default:
                Debug.LogWarning($"Unknown action: {_menuSelectOptions[_currentSelectionIndex].Action}");
                break;
        }
    }

    private void HandlePlay()
    {
        Debug.Log("HandlePlay called");

        // Ensure normal play always clears any user test settings
        UserTestConfig.ResetToNormalMode();

        if (_onboardingPopup == null)
        {
            Debug.LogWarning("OnboardingPopup not assigned!");
            return;
        }

        _onboardingPopup.OpenPopup();
    }

    private void HandleUserTest()
    {
        Debug.Log("HandleUserTest called");

        // User test flow:
        // Phase 1 = no haptics
        // Phase 2 = with haptics
        // Phase 3 = tactile discrimination test
        UserTestConfig.IsUserTestMode = true;
        UserTestConfig.CurrentPhase = 1;
        UserTestConfig.HapticsEnabled = false;

        // Use 2 fish by default for a shorter, less fatiguing test
        UserTestConfig.OverrideFishTotalToCatch = 2;

        if (_onboardingPopup == null)
        {
            Debug.LogWarning("OnboardingPopup not assigned!");
            return;
        }

        _onboardingPopup.OpenPopup();
    }

    private void OnDisable()
    {
        if (MainMenuUIController.Instance != null)
        {
            MainMenuUIController.Instance.ViewChanged.RemoveListener(HandleInputSubscription);
        }

        if (InputDeviceManager.Instance != null && InputDeviceManager.Instance.JoystickInput != null)
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.RemoveListener(OnOptionSelected);
        }
    }
}