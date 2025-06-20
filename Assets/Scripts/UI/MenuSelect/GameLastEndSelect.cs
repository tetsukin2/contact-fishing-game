using UnityEngine;

public class GameLastEndSelect : MenuSelect
{
    private const string RESTART_ACTION = "Restart";
    private const string REPLAY_LEVEL_ACTION = "ReplayLevel";
    private const string MAIN_MENU_ACTION = "MainMenu";

    private const string STAGE_1_NAME = "Stage1";

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
            case RESTART_ACTION:
                SceneSwitchHandler.Instance.LoadScene(STAGE_1_NAME);
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
