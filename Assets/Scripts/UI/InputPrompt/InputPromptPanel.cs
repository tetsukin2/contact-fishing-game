using TMPro;
using UnityEngine;

/// <summary>
/// Dynamic Video Panel for Input prompts.
/// Can be main or secondary input prompt, and depends on UIManager prompt events.
/// </summary>
public class InputPromptPanel : DynamicVideoPanel
{
    public enum InputPromptType
    {
        Main,
        Secondary
    }

    [SerializeField] private TextMeshProUGUI _message;
    [SerializeField] private InputPromptType _inputPromptType = InputPromptType.Main;

    private void Start()
    {
        // Checks for input prompt type
        UIManager.Instance.MainInputPromptShown.AddListener(OnMainInputPromptShown);
        UIManager.Instance.SecondInputPromptShown.AddListener(OnSecondInputPromptShown);
    }

    private void OnMainInputPromptShown(InputPrompt inputPrompt)
    {
        if (_inputPromptType == InputPromptType.Main)
        SetInputPrompt(inputPrompt);
    }

    private void OnSecondInputPromptShown(InputPrompt inputPrompt)
    {
        if (_inputPromptType == InputPromptType.Secondary)
        SetInputPrompt(inputPrompt);
    }

    /// <summary>
    /// Sets the video and message for the input prompt. Hides the panel if inputPrompt is null.
    /// </summary>
    /// <param name="inputPrompt"></param>
    public void SetInputPrompt(InputPrompt inputPrompt)
    {
        if (inputPrompt == null)
        {
            Show(false);
            return;
        }
        Show(true);
        SetVideo(inputPrompt.Video);
        _message.text = inputPrompt.Message;
    }
}