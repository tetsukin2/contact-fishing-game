using UnityEngine;

public class MainMenuSelect : MenuSelect
{
    private const string PLAY_ACTION = "Play";
    private const string ENCYCLOPEDIA_ACTION = "OpenEncyclopedia";
    private const string EXIT_ACTION = "Exit";

    protected override void Start()
    {
        base.Start();
        GameManager.Instance.GameStateEntered.AddListener(OnGameStateUpdated);

        //initially listen for now at least to guarantee since we start in main menu
        InputDeviceManager.Instance.JoystickPressed.AddListener(OnOptionSelected);
    }

    private void OnGameStateUpdated(GameState newState)
    {
        if (newState == GameManager.Instance.MainMenuState)
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
        if (GameManager.Instance.CurrentState != GameManager.Instance.MainMenuState) return;

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case PLAY_ACTION:
                GameManager.Instance.TransitionToState(GameManager.Instance.GameStartState);
                break;
            case ENCYCLOPEDIA_ACTION:
                GameManager.Instance.TransitionToState(GameManager.Instance.EncyclopediaState);
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
