using UnityEngine;

public class GameEndSelect : MenuSelect
{
    private const string NEXT_LEVEL_ACTION = "NextLevel";
    private const string REPLAY_LEVEL_ACTION = "ReplayLevel";
    private const string MAIN_MENU_ACTION = "MainMenu";

    protected override void Start()
    {
        base.Start();
        InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(OnOptionSelected);
    }

    protected override void OnOptionSelected()
    {
        if (LevelManager.Instance.CurrentState != LevelManager.Instance.EndScoreState) return;

        AudioManager.Instance?.PlaySelect();

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case NEXT_LEVEL_ACTION:
                SceneSwitchHandler.Instance.LoadNextScene();
                break;
            case REPLAY_LEVEL_ACTION:
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