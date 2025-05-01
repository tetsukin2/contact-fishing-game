using UnityEngine;

public class MainMenuSelect : MenuSelect
{
    private const string PLAY_ACTION = "Play";
    private const string ENCYCLOPEDIA_ACTION = "OpenEncyclopedia";
    private const string EXIT_ACTION = "Exit";

    protected override void OnOptionSelected()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.MainMenuState) return;

        switch (_menuSelectOptions[_currentSelectionIndex].Action)
        {
            case PLAY_ACTION:
                GameManager.Instance.TransitionToState(GameManager.Instance.GameStartState);
                break;
            case ENCYCLOPEDIA_ACTION:
                Debug.Log("Encyclopedia not yet implemented");
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
