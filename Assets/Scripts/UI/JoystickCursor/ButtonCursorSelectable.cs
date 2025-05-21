using UnityEngine.Events;

public class ButtonCursorSelectable : JoystickCursorSelectable
{
    public UnityEvent onSelect { get; private set; } = new();

    public override void OnSelect()
    {
        if (MainMenuUIController.Instance.CurrentView != MainMenuUIController.MainMenuView.Encyclopedia) return;
        onSelect.Invoke();
    }
}
