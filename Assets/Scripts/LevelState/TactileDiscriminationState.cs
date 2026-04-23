using System.Collections.Generic;
using UnityEngine;

public class TactileDiscriminationState : LevelState
{
    public class PatternDefinition
    {
        public string SequenceName;
        public string DisplayName;

        public PatternDefinition(string sequenceName, string displayName)
        {
            SequenceName = sequenceName;
            DisplayName = displayName;
        }
    }

    private enum DiscriminationStage
    {
        Exploration,
        TestWaitingForChoices,
        TestChoosing,
        Results
    }

    private readonly List<PatternDefinition> _patterns = new()
    {
        new PatternDefinition("WaveIn", "Wave In"),
        new PatternDefinition("WaveOut", "Wave Out"),
        new PatternDefinition("RotateCircle", "Circular Rotation"),
        new PatternDefinition("Ripple", "Ripple"),
        new PatternDefinition("BasicPulse", "Basic Pulse")
    };

    private readonly List<string> _trialOrder = new();
    private readonly List<string> _trialPatterns = new();
    private readonly List<string> _userAnswers = new();
    private readonly List<bool> _correctAnswers = new();

    private DiscriminationStage _stage;
    private int _currentTrialIndex;
    private int _score;
    private string _currentCorrectPattern;
    private string _currentlyExploringPattern;
    private float _choiceDelayTimer;

    private const int PATTERN_REPETITIONS = 2;

    // How long the user gets to feel the test pattern before choices appear.
    // Increase this if the pattern still feels too short.
    private const float CHOICE_DELAY_SECONDS = 3.0f;

    public TactileDiscriminationState(LevelManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Tactile Discrimination State");

        UserTestConfig.CurrentPhase = 3;
        UserTestConfig.HapticsEnabled = true;

        _stage = DiscriminationStage.Exploration;
        _currentTrialIndex = 0;
        _score = 0;
        _choiceDelayTimer = 0f;
        _currentCorrectPattern = null;
        _currentlyExploringPattern = null;

        _trialOrder.Clear();
        _trialPatterns.Clear();
        _userAnswers.Clear();
        _correctAnswers.Clear();

        CameraController.Instance.SetCameraView(CameraController.CameraView.Gameplay);

        ShowExploration();
    }

    public override void Update()
    {
        if (_stage == DiscriminationStage.TestWaitingForChoices)
        {
            _choiceDelayTimer -= Time.deltaTime;

            if (_choiceDelayTimer <= 0f)
            {
                ShowCurrentTrialChoices();
            }
        }
    }

    public override void Exit()
    {
        if (TactileDiscriminationUI.Instance != null)
            TactileDiscriminationUI.Instance.HideAll();

        if (BraillePatternPlayer.Instance != null)
            BraillePatternPlayer.Instance.StopPatternSequence();

        Debug.Log("Exiting Tactile Discrimination State");
    }

    private void ShowExploration()
    {
        if (TactileDiscriminationUI.Instance == null)
        {
            Debug.LogError("TactileDiscriminationUI.Instance is null.");
            return;
        }

        string currentPatternDisplayName = string.Empty;

        if (!string.IsNullOrEmpty(_currentlyExploringPattern))
        {
            PatternDefinition currentPattern = _patterns.Find(p => p.SequenceName == _currentlyExploringPattern);
            currentPatternDisplayName = currentPattern != null ? currentPattern.DisplayName : string.Empty;
        }

        TactileDiscriminationUI.Instance.ShowExploration(
            _patterns,
            currentPatternDisplayName,
            OnExplorationPatternPressed,
            StartActualTest
        );
    }

    private void OnExplorationPatternPressed(string sequenceName)
    {
        if (_stage != DiscriminationStage.Exploration)
            return;

        if (BraillePatternPlayer.Instance == null)
        {
            Debug.LogError("BraillePatternPlayer.Instance is null.");
            return;
        }

        // Toggle same pattern off
        if (_currentlyExploringPattern == sequenceName)
        {
            _currentlyExploringPattern = null;
            BraillePatternPlayer.Instance.StopPatternSequence();
            ShowExploration();
            return;
        }

        // Switch to new pattern and loop it during exploration
        _currentlyExploringPattern = sequenceName;
        BraillePatternPlayer.Instance.StopPatternSequence();
        BraillePatternPlayer.Instance.PlayPatternSequence(sequenceName, true);
        ShowExploration();
    }

    private void StartActualTest()
    {
        if (BraillePatternPlayer.Instance != null)
            BraillePatternPlayer.Instance.StopPatternSequence();

        _currentlyExploringPattern = null;

        BuildTrialOrder();
        _currentTrialIndex = 0;
        _score = 0;

        StartNextTrial();
    }

    private void BuildTrialOrder()
    {
        _trialOrder.Clear();

        foreach (PatternDefinition pattern in _patterns)
        {
            for (int i = 0; i < PATTERN_REPETITIONS; i++)
            {
                _trialOrder.Add(pattern.SequenceName);
            }
        }

        Shuffle(_trialOrder);
    }

    private void StartNextTrial()
    {
        if (_currentTrialIndex >= _trialOrder.Count)
        {
            FinishTest();
            return;
        }

        _currentCorrectPattern = _trialOrder[_currentTrialIndex];
        _stage = DiscriminationStage.TestWaitingForChoices;
        _choiceDelayTimer = CHOICE_DELAY_SECONDS;

        if (TactileDiscriminationUI.Instance == null)
        {
            Debug.LogError("TactileDiscriminationUI.Instance is null.");
            return;
        }

        TactileDiscriminationUI.Instance.ShowTestWaiting(
            _currentTrialIndex + 1,
            _trialOrder.Count
        );

        // In test mode, loop the correct pattern while the user is feeling it.
        // It will be stopped once the answer choices appear.
        if (BraillePatternPlayer.Instance != null)
        {
            BraillePatternPlayer.Instance.StopPatternSequence();
            BraillePatternPlayer.Instance.PlayPatternSequence(_currentCorrectPattern, true);
            //Debug.Log("Playing test pattern: " + _currentCorrectPattern);
        }
        else
        {
            Debug.LogError("BraillePatternPlayer.Instance is null.");
        }
    }

    private void ShowCurrentTrialChoices()
    {
        _stage = DiscriminationStage.TestChoosing;

        // Stop the tactile playback before showing the answers.
        if (BraillePatternPlayer.Instance != null)
            BraillePatternPlayer.Instance.StopPatternSequence();

        if (TactileDiscriminationUI.Instance == null)
        {
            Debug.LogError("TactileDiscriminationUI.Instance is null.");
            return;
        }

        TactileDiscriminationUI.Instance.ShowTestChoices(
            _patterns,
            _currentTrialIndex + 1,
            _trialOrder.Count,
            OnTestPatternSelected
        );
    }

    private void OnTestPatternSelected(string selectedSequenceName)
    {
        if (_stage != DiscriminationStage.TestChoosing)
            return;

        bool isCorrect = selectedSequenceName == _currentCorrectPattern;

        _trialPatterns.Add(_currentCorrectPattern);
        _userAnswers.Add(selectedSequenceName);
        _correctAnswers.Add(isCorrect);

        if (isCorrect)
            _score++;

        _currentTrialIndex++;
        StartNextTrial();
    }

    private void FinishTest()
    {
        _stage = DiscriminationStage.Results;

        if (BraillePatternPlayer.Instance != null)
            BraillePatternPlayer.Instance.StopPatternSequence();

        LevelManager.Instance.SetDiscriminationTelemetry(
            _score,
            _trialOrder.Count,
            new List<string>(_trialPatterns),
            new List<string>(_userAnswers),
            new List<bool>(_correctAnswers)
        );

        if (TactileDiscriminationUI.Instance == null)
        {
            Debug.LogError("TactileDiscriminationUI.Instance is null.");
            return;
        }

        TactileDiscriminationUI.Instance.ShowResults(
            _score,
            _trialOrder.Count,
            OnFinishPressed
        );
    }

    private void OnFinishPressed()
    {
        gameManager.UploadTelemetryNow();
        gameManager.TransitionToState(gameManager.EndScoreState);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}