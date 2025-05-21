using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles Common UI for all scenes.
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Header("Loading Screen")]
    [SerializeField] private GUIContainer _loadingScreen;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Space]
    [SerializeField] private JoystickCursor _joystickCursor;

    // There are separate explicit input prompts because of how the video render textures work
    public UnityEvent<InputPrompt> MainInputPromptShown { get; private set; } = new();
    public UnityEvent<InputPrompt> SecondInputPromptShown { get; private set; } = new();

    public JoystickCursor JoystickCursor => _joystickCursor;

    protected override void OnRegister()
    {
        // Loading screen things
        _loadingScreen.Show(true);
        InputDeviceManager.Instance.ConnectionStatusLog.AddListener((string message) => _loadingText.SetText(message));
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => _loadingScreen.Show(false));
    }

    #region Input Prompts
    //Ideally, only handle prompt showing or hiding at the start or end of each game or fishing state.

    /// <summary>
    /// Show primary input prompt
    /// </summary>
    /// <param name="name">Input prompt to show. Values with no match are treated as null</param>
    public void ShowMainInputPrompt(string name)
    {
        Debug.Log($"Showing prompt {name}");
        MainInputPromptShown.Invoke(ResourceSystem.Instance.GetInputPrompt(name));
    }

    /// <summary>
    /// Show primary input prompt
    /// </summary>
    /// <param name="prompt">Input prompt to show. Pass null to hide the input prompt panel</param>
    public void ShowMainInputPrompt(InputPrompt prompt)
    {
        MainInputPromptShown.Invoke(prompt);
    }

    /// <summary>
    /// Show secondary input prompt
    /// </summary>
    /// <param name="name">Input prompt to show. Values with no match are treated as null</param>
    public void ShowSecondInputPrompt(string name)
    {
        Debug.Log($"2nd prompt {name}");
        SecondInputPromptShown.Invoke(ResourceSystem.Instance.GetInputPrompt(name));
    }

    /// <summary>
    /// Show secondary input prompt
    /// </summary>
    /// <param name="prompt">Input prompt to show. Pass null to hide the input prompt panel</param>
    public void ShowSecondInputPrompt(InputPrompt prompt)
    {
        SecondInputPromptShown.Invoke(prompt);
    }
    #endregion
}

