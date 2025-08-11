using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Loads and manages scriptable objects.
/// </summary>
public class ResourceSystem : SingletonPersistent<ResourceSystem>
{
    /// <summary>
    /// The currently loaded gameplay configuration.
    /// </summary>
    public GameplayConfig GameplayConfig { get; private set; }

    private List<InputPrompt> _inputPrompts; // Input prompts to use

    // === Resource Paths ===
    private const string InputPromptsPath = "InputPrompts"; // Path to input prompt directory
    private const string GameplayConfigPath = "GameplayConfig"; // Path to gameplay config

    protected override void OnAwake()
    {
        // Load all input prompts from resources
        _inputPrompts = Resources.LoadAll<InputPrompt>(InputPromptsPath).ToList();
        GameplayConfig = Resources.Load<GameplayConfig>(GameplayConfigPath);

        // a test
        //Debug.Log($"Loaded {GameplayConfig.name} with {GameplayConfig.ReelActionSequence.Count} reel actions and {GameplayConfig.BaitPreparationSteps} bait preparation steps.");
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
