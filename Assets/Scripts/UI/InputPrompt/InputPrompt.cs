using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Scriptable object for input prompts.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Object/Input Prompt")]
public class InputPrompt : ScriptableObject
{
    public string PromptName; // Name of the input prompt
    public string Message; // Message to display for the input prompt
    public VideoClip Video; // Video clip associated with the input prompt
}
