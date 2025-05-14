using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Prompt list is out here for centralized access
    // idk if this is final
    [Header("Input Prompts")]
    [SerializeField] private List<InputPrompt> _inputPrompts; // Input prompts to use
    [SerializeField] private InputPrompt _mainMenuInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _mainMenuSecondInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _encyclopediaInput;
    [SerializeField] private InputPrompt _encyclopediaSecondInput;

    [Space]
    [Header("Loading Screen")]
    [SerializeField] private GUIContainer _loadingScreen;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Space]
    [SerializeField] private JoystickCursor _joystickCursor;

    // There are separate explicit input prompts because of how the video render textures work
    public UnityEvent<InputPrompt> MainInputPromptShown { get; private set; } = new();
    public UnityEvent<InputPrompt> SecondInputPromptShown { get; private set; } = new();

    // Input prompt accessors, doing it this way allows you to see references in Visual Studio
    public InputPrompt MainMenuInput => _mainMenuInput;
    public InputPrompt MainMenuSecondInput => _mainMenuSecondInput;
    public InputPrompt EncyclopediaInput => _encyclopediaInput;
    public InputPrompt EncyclopediaSecondInput => _encyclopediaSecondInput;

    public JoystickCursor JoystickCursor => _joystickCursor;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
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
    /// <param name="name">Input prompt to show. Pass null to hide the input prompt panel</param>
    public void ShowMainInputPrompt(string name)
    {
        foreach (var prompt in _inputPrompts)
        {
            if (prompt.PromptName == name)
            {
                MainInputPromptShown.Invoke(prompt);
                return;
            }
        }
        MainInputPromptShown.Invoke(null);
        return;
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
    /// <param name="name">Input prompt to show. Pass null to hide the input prompt panel</param>
    public void ShowSecondInputPrompt(string name)
    {
        Debug.Log($"2nd prompt {name}");
        foreach (var prompt in _inputPrompts)
        {
            if (prompt.PromptName == name)
            {
                SecondInputPromptShown.Invoke(prompt);
                return;
            }
        }
        SecondInputPromptShown.Invoke(null);
        return;
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

