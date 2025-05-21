using UnityEngine;

public class GameEndSelect : MenuSelect
{
    private const string REPLAY_ACTION = "Replay";
    private const string MAIN_MENU_ACTION = "MainMenu";
    private const string EXIT_ACTION = "Exit";

    protected override void Start()
    {
        base.Start();
        InputDeviceManager.Instance.JoystickPressed.AddListener(OnOptionSelected);
    }

    protected override void OnOptionSelected()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.EndScoreState) return;

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case REPLAY_ACTION:
                GameManager.Instance.TransitionToState(GameManager.Instance.GameStartState);
                break;
            case MAIN_MENU_ACTION:
                //GameManager.Instance.TransitionToState(GameManager.Instance.MainMenuState);
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
