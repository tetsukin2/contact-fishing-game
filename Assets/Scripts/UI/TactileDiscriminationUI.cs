using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TactileDiscriminationUI : MonoBehaviour
{
    public static TactileDiscriminationUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject _rootPanel;

    [Header("Shared")]
    [SerializeField] private TMP_Text _instructionText;

    [Header("Exploration")]
    [SerializeField] private GameObject _explorationPanel;
    [SerializeField] private TMP_Text _currentPatternText;
    [SerializeField] private Button _startTestButton;
    [SerializeField] private PatternChoiceButton[] _explorationButtons;

    [Header("Test")]
    [SerializeField] private GameObject _testPanel;
    [SerializeField] private TMP_Text _trialProgressText;
    [SerializeField] private TMP_Text _trialInstructionText;
    [SerializeField] private PatternChoiceButton[] _testButtons;

    [Header("Results")]
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private Button _finishButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideAll();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void HideAll()
    {
        if (_rootPanel != null) _rootPanel.SetActive(false);
        if (_explorationPanel != null) _explorationPanel.SetActive(false);
        if (_testPanel != null) _testPanel.SetActive(false);
        if (_resultPanel != null) _resultPanel.SetActive(false);
    }

    public void ShowExploration(
        List<TactileDiscriminationState.PatternDefinition> patterns,
        string currentPatternDisplayName,
        Action<string> onPatternPressed,
        Action onStartTest)
    {
        if (_rootPanel == null || _explorationPanel == null || _testPanel == null || _resultPanel == null)
        {
            Debug.LogError("TactileDiscriminationUI has missing panel references.");
            return;
        }

        if (_startTestButton == null)
        {
            Debug.LogError("TactileDiscriminationUI start test button is missing.");
            return;
        }

        ShowRootOnly();

        _explorationPanel.SetActive(true);
        _testPanel.SetActive(false);
        _resultPanel.SetActive(false);

        // Exploration uses the shared instruction text.
        if (_instructionText != null)
        {
            _instructionText.gameObject.SetActive(true);
            _instructionText.text = "Explore the tactile patterns freely. Press a pattern button to play or stop it.";
        }

        // Hide the test-specific instruction text so it does not overlap.
        if (_trialInstructionText != null)
            _trialInstructionText.gameObject.SetActive(false);

        if (_currentPatternText != null)
        {
            _currentPatternText.text = string.IsNullOrEmpty(currentPatternDisplayName)
                ? "Current Pattern: None"
                : $"Current Pattern: {currentPatternDisplayName}";
        }

        BindButtons(_explorationButtons, patterns, onPatternPressed);

        _startTestButton.onClick.RemoveAllListeners();
        _startTestButton.onClick.AddListener(() => onStartTest?.Invoke());
    }

    public void ShowTestWaiting(int currentTrial, int totalTrials)
    {
        ShowRootOnly();

        if (_explorationPanel != null) _explorationPanel.SetActive(false);
        if (_testPanel != null) _testPanel.SetActive(true);
        if (_resultPanel != null) _resultPanel.SetActive(false);

        // Hide the shared instruction text during the test so it does not overlap.
        if (_instructionText != null)
            _instructionText.gameObject.SetActive(false);

        if (_trialInstructionText != null)
        {
            _trialInstructionText.gameObject.SetActive(true);
            _trialInstructionText.text = "Feel the pattern first.";
        }

        if (_trialProgressText != null)
            _trialProgressText.text = $"Trial {currentTrial}/{totalTrials}";

        SetButtonsVisible(_testButtons, false);
    }

    public void ShowTestChoices(
        List<TactileDiscriminationState.PatternDefinition> patterns,
        int currentTrial,
        int totalTrials,
        Action<string> onPatternPressed)
    {
        ShowRootOnly();

        if (_explorationPanel != null) _explorationPanel.SetActive(false);
        if (_testPanel != null) _testPanel.SetActive(true);
        if (_resultPanel != null) _resultPanel.SetActive(false);

        // Hide the shared instruction text during the test so it does not overlap.
        if (_instructionText != null)
            _instructionText.gameObject.SetActive(false);

        if (_trialInstructionText != null)
        {
            _trialInstructionText.gameObject.SetActive(true);
            _trialInstructionText.text = "Choose the pattern name that matches what you felt.";
        }

        if (_trialProgressText != null)
            _trialProgressText.text = $"Trial {currentTrial}/{totalTrials}";

        BindButtons(_testButtons, patterns, onPatternPressed);
        SetButtonsVisible(_testButtons, true);
    }

    public void ShowResults(int score, int totalTrials, Action onFinish)
    {
        ShowRootOnly();

        if (_explorationPanel != null) _explorationPanel.SetActive(false);
        if (_testPanel != null) _testPanel.SetActive(false);
        if (_resultPanel != null) _resultPanel.SetActive(true);

        if (_instructionText != null)
        {
            _instructionText.gameObject.SetActive(true);
            _instructionText.text = "The tactile discrimination test is complete.";
        }

        if (_trialInstructionText != null)
            _trialInstructionText.gameObject.SetActive(false);

        if (_resultText != null)
            _resultText.text = $"Score: {score}/{totalTrials}";

        if (_finishButton != null)
        {
            _finishButton.onClick.RemoveAllListeners();
            _finishButton.onClick.AddListener(() => onFinish?.Invoke());
        }
    }

    private void ShowRootOnly()
    {
        if (_rootPanel != null)
            _rootPanel.SetActive(true);
    }

    private void BindButtons(
        PatternChoiceButton[] buttons,
        List<TactileDiscriminationState.PatternDefinition> patterns,
        Action<string> onPatternPressed)
    {
        if (buttons == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
                continue;

            if (i < patterns.Count)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].Setup(patterns[i].DisplayName, patterns[i].SequenceName, onPatternPressed);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetButtonsVisible(PatternChoiceButton[] buttons, bool visible)
    {
        if (buttons == null)
            return;

        foreach (var button in buttons)
        {
            if (button != null)
                button.gameObject.SetActive(visible);
        }
    }
}