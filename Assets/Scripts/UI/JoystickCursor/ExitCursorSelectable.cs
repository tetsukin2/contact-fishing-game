/// <summary>
/// For exiting the encyclopedia.
/// </summary>
public class ExitCursorSelectable : JoystickCursorSelectable
{
    public override void OnSelect()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.EncyclopediaState) return;
        GameManager.Instance.TransitionToState(GameManager.Instance.MainMenuState);
    }
}
