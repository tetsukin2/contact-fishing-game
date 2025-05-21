using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Overall main menu GUI.
/// </summary>
public class MainMenuGUI : GUIContainer
{
    [SerializeField] private GUIContainer _mainMenuSelectGUI;

    public override void Show(bool show)
    {
        base.Show(show);
        _mainMenuSelectGUI?.Show(show);

        if (!show) return;

        // Show navigation instructions on show
        UIManager.Instance.ShowMainInputPrompt(UIManager.Instance.MainMenuInput);
        UIManager.Instance.ShowSecondInputPrompt(UIManager.Instance.MainMenuSecondInput);
    }
}
