using UnityEngine;

public class PauseSelect : MenuSelect
{
    private const string CONTINUE_ACTION = "Continue";
    private const string RESTART_ACTION = "Restart";
    private const string MAIN_MENU_ACTION = "MainMenu";

    protected override void Start()
    {
        base.Start();
        InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(OnOptionSelected);
    }

    protected override void OnOptionSelected()
    {
        if (!LevelManager.Instance.IsGamePaused) return;

        LevelManager.Instance.SetGamePaused(false); // All options unpause anyway

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case CONTINUE_ACTION: break;
            case RESTART_ACTION:
                SceneSwitchHandler.Instance.ReloadScene();
                break;
            case MAIN_MENU_ACTION:
                SceneSwitchHandler.Instance.ReturnToMainMenu();
                break;
            default:
                Debug.LogWarning($"Unknown action: {_menuSelectOptions[_currentSelectionIndex].Action}");
                break;
        }
    }
}
