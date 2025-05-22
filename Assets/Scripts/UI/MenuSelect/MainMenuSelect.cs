using UnityEngine;

public class MainMenuSelect : MenuSelect
{
    private const string PLAY_ACTION = "Play";
    private const string ENCYCLOPEDIA_ACTION = "OpenEncyclopedia";
    private const string EXIT_ACTION = "Exit";

    private const string FIRST_LEVEL_NAME = "Stage1";

    protected override void Start()
    {
        base.Start();
        MainMenuUIController.Instance.ViewChanged.AddListener(HandleInputSubscription);

        ////initially listen for now at least to guarantee since we start in main menu
        //InputDeviceManager.Instance.JoystickPressed.AddListener(OnOptionSelected);
    }

    // We only want to hear input when in the main menu state
    private void HandleInputSubscription(MainMenuUIController.MainMenuView newView)
    {
        if (newView == MainMenuUIController.MainMenuView.MainMenu)
        {
            InputDeviceManager.Instance.JoystickPressed.AddListener(OnOptionSelected);
        }
        else
        {
            InputDeviceManager.Instance.JoystickPressed.RemoveListener(OnOptionSelected);
        }
    }

    protected override void OnOptionSelected()
    {
        //if (GameManager.Instance.CurrentState != GameManager.Instance.MainMenuState) return;

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case PLAY_ACTION:
                SceneSwitchHandler.Instance.LoadScene(FIRST_LEVEL_NAME);
                break;
            case ENCYCLOPEDIA_ACTION:
                MainMenuUIController.Instance.ChangeView(MainMenuUIController.MainMenuView.Encyclopedia);
                break;
            case EXIT_ACTION:
                GameManager.QuitGame();
                break;
            default:
                Debug.LogWarning($"Unknown action: {_menuSelectOptions[_currentSelectionIndex].Action}");
                break;
        }
    }
}
