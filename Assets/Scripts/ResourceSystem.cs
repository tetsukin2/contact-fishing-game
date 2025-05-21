using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Loads and manages scriptable objects.
/// </summary>
public class ResourceSystem : SingletonPersistent<ResourceSystem>
{
    private List<InputPrompt> _inputPrompts; // Input prompts to use

    protected override void OnAwake()
    {
        // Load all input prompts from resources
        _inputPrompts = Resources.LoadAll<InputPrompt>("InputPrompts").ToList();
    }

    /// <summary>
    /// Get an input prompt by name.
    /// </summary>
    /// <returns>InputPrompt with matching name, or null</returns>
    public InputPrompt GetInputPrompt(string promptName)
    {
        var prompt = _inputPrompts.FirstOrDefault(p => p.PromptName == promptName);
        if (prompt != null)
        {
            Debug.Log($"Prompt {promptName} found");
        }
        return prompt;
    }
}
