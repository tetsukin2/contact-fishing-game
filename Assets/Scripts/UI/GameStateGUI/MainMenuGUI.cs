using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Overall main menu GUI.
/// </summary>
public class MainMenuGUI : GUIContainer
{
    [SerializeField] private GUIContainer _mainMenuSelectGUI;

    private void Start()
    {
        GameManager.Instance.GameStateEntered.AddListener(OnGameStateEntered);
    }

    // Main Menu UI Setup
    private void OnGameStateEntered(GameState newState)
    {
        bool isMainMenuState = (newState == GameManager.Instance.MainMenuState);
        Show(isMainMenuState);
        _mainMenuSelectGUI.Show(isMainMenuState);

        // Only setup when main menu is shown
        if (!isMainMenuState) return;

        UIManager.Instance.ShowMainInputPrompt(UIManager.Instance.MainMenuInput);
        UIManager.Instance.ShowSecondInputPrompt(UIManager.Instance.MainMenuSecondInput);
    }
}
