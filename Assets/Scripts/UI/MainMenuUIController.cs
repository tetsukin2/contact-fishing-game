using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles UI specific to the Main Menu.
/// </summary>
public class MainMenuUIController : Singleton<MainMenuUIController>
{
    public enum MainMenuView
    {
        None,
        MainMenu,
        Encyclopedia,
        LevelSelect,
        Login,
        Settings,
    }

    [Header("Input Prompts")]
    // Prompt list is out here for centralized access
    // idk if this is final
    [SerializeField] private InputPrompt _mainMenuInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _mainMenuSecondInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _encyclopediaInput;
    [SerializeField] private InputPrompt _encyclopediaSecondInput;

    public MainMenuView CurrentView { get; private set; } = MainMenuView.Login;

    public UnityEvent<MainMenuView> ViewChanged { get; private set; } = new();

    // Input prompt accessors, doing it this way allows you to see references in Visual Studio
    public InputPrompt MainMenuInput => _mainMenuInput;
    public InputPrompt MainMenuSecondInput => _mainMenuSecondInput;
    public InputPrompt EncyclopediaInput => _encyclopediaInput;
    public InputPrompt EncyclopediaSecondInput => _encyclopediaSecondInput;

    protected override void OnSetup()
    {
        InputDeviceManager.Instance.BLEDevice.RunWhenConnected(FirstTimeSetup);
    }

    private void FirstTimeSetup()
    {
        // Start in login
        ChangeView(MainMenuView.Login);

        // Start with these inputs
        UIManager.Instance.ShowMainInputPrompt(MainMenuInput);
        UIManager.Instance.ShowSecondInputPrompt(MainMenuSecondInput);
    }

    /// <summary>
    /// Handles main menu view changes and fires the ViewChanged event.
    /// Individual elements affected by view changes should subscribe to this event or use the MainMenuVisibility component.
    /// </summary>
    public void ChangeView(MainMenuView newView)
    {
        CurrentView = newView;
        ViewChanged.Invoke(newView);
    }
}
