using UnityEngine;

public class GameEndSelect : MenuSelect
{
    private const string REPLAY_ACTION = "Replay";
    private const string MAIN_MENU_ACTION = "MainMenu";
    private const string EXIT_ACTION = "Exit";

    protected override void Start()
    {
        base.Start();
        InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(OnOptionSelected);
    }

    protected override void OnOptionSelected()
    {
        if (LevelManager.Instance.CurrentState != LevelManager.Instance.EndScoreState) return;

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case REPLAY_ACTION:
                LevelManager.Instance.TransitionToState(LevelManager.Instance.GameStartState);
                break;
            case MAIN_MENU_ACTION:
                SceneSwitchHandler.Instance.ReturnToMainMenu();
                break;
            case EXIT_ACTION:
                LevelManager.QuitGame();
                break;
            default:
                Debug.LogWarning($"Unknown action: {_menuSelectOptions[_currentSelectionIndex].Action}");
                break;
        }
    }
}
