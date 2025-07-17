using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Loads and manages scriptable objects.
/// </summary>
public class ResourceSystem : SingletonPersistent<ResourceSystem>
{
    public GameConfig GameConfig { get; private set; } // Game configuration scriptable object

    private List<InputPrompt> _inputPrompts; // Input prompts to use

    private const string InputPromptsPath = "InputPrompts"; // Path to input prompts in Resources

    protected override void OnAwake()
    {
        // Load all input prompts from resources
        _inputPrompts = Resources.LoadAll<InputPrompt>(InputPromptsPath).ToList();
        GameConfig = Resources.Load<GameConfig>("GameConfig");

        // a test
        Debug.Log($"Loaded {GameConfig.name} with {GameConfig.ReelActionSequence.Count} reel actions and {GameConfig.BaitPreparationSteps} bait preparation steps.");
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
