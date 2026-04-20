using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Scriptable object for input prompts.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Object/Input Prompt")]
public class InputPrompt : ScriptableObject
{
    public enum PromptPulseType
    {
        None,
        FastOutward,
        SlowInward
    }

    [Header("Content")]
    public string PromptName;
    [TextArea(2, 4)]
    public string Message;
    public VideoClip Video;

    [Header("Progress")]
    public bool UseProgress = false;

    [Header("Visual Feedback")]
    public PromptPulseType PulseType = PromptPulseType.None;
}