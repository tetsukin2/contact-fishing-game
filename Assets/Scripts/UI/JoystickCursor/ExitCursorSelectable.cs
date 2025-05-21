/// <summary>
/// For exiting the encyclopedia.
/// </summary>
public class ExitCursorSelectable : JoystickCursorSelectable
{
    public override void OnSelect()
    {
        if (MainMenuUIController.Instance.CurrentView != MainMenuUIController.MainMenuView.Encyclopedia) return;
        MainMenuUIController.Instance.ChangeView(MainMenuUIController.MainMenuView.MainMenu);
    }
}
