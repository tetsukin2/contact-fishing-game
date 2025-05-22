using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles UI specific to the Main Menu.
/// </summary>
public class MainMenuUIController : Singleton<MainMenuUIController>
{
    public enum MainMenuView
    {
        MainMenu,
        Encyclopedia,
        LevelSelect,
        Settings,
    }

    [Header("Input Prompts")]
    // Prompt list is out here for centralized access
    // idk if this is final
    [SerializeField] private InputPrompt _mainMenuInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _mainMenuSecondInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _encyclopediaInput;
    [SerializeField] private InputPrompt _encyclopediaSecondInput;

    [Space]
    [Header("Game State GUI")]
    // Major game "views" that are state dependent
    [SerializeField] private MainMenuGUI _mainMenuGUI;
    [SerializeField] private EncyclopediaGUI _encyclopediaGUI;

    public MainMenuView CurrentView { get; private set; } = MainMenuView.MainMenu;

    public UnityEvent<MainMenuView> ViewChanged { get; private set; } = new();

    // Input prompt accessors, doing it this way allows you to see references in Visual Studio
    public InputPrompt MainMenuInput => _mainMenuInput;
    public InputPrompt MainMenuSecondInput => _mainMenuSecondInput;
    public InputPrompt EncyclopediaInput => _encyclopediaInput;
    public InputPrompt EncyclopediaSecondInput => _encyclopediaSecondInput;

    protected override void OnRegister()
    {
        HideAllGUI();
    }

    protected override void OnSetup()
    {
        InputDeviceManager.Instance.RunWhenConnected(FirstTimeSetup);
    }

    private void FirstTimeSetup()
    {
        // Start in the main menu
        ChangeView(MainMenuView.MainMenu);

        // Start with these inputs
        UIManager.Instance.ShowMainInputPrompt(MainMenuInput);
        UIManager.Instance.ShowSecondInputPrompt(MainMenuSecondInput);
    }

    /// <summary>
    /// Handles main menu view changes
    /// </summary>
    /// <param name="newView">New view to _transitionAnimator to</param>
    public void ChangeView(MainMenuView newView)
    {
        CurrentView = newView;
        ViewChanged.Invoke(newView);
        OnViewChanged(newView);
    }

    private void HideAllGUI()
    {
        // Hide all GUI elements
        _mainMenuGUI.Show(false);
        _encyclopediaGUI.Show(false);
    }

    private void OnViewChanged(MainMenuView newView)
    {
        // Show the appropriate GUI based on the game state
        _mainMenuGUI.Show(newView == MainMenuView.MainMenu);
        _encyclopediaGUI.Show(newView == MainMenuView.Encyclopedia);
    }
}
