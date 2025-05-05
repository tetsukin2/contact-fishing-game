using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class InputPromptPanel : DynamicVideoPanel
{
    [SerializeField] private TextMeshProUGUI _message;

    public void SetInputPrompt(InputPrompt inputPrompt)
    {
        Show(inputPrompt != null);
        if (inputPrompt == null)
        {
            _message.text = string.Empty;
            return;
        }
        SetVideo(inputPrompt.Video);
        _message.text = inputPrompt.Message;
    }
}