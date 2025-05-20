using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Handles logic and UI of reel progress, somewhat a violation but eh
/// </summary>
public class ReelProgressBar : GUIContainer
{
    [SerializeField] private Slider _progressSlider;

    private float _currentReelProgress;
    private bool _isReeling = false; // Flag for reel updates

    /// <summary>
    /// Event triggered when the reel progress is updated but not completed.
    /// </summary>
    public UnityEvent ReelProgressed { get; private set; } = new();
    /// <summary>
    /// Event triggered when the reel progress is completed.
    /// </summary>
    public UnityEvent ReelCompleted { get; private set; } = new();

    private void Update()
    {
        // Update progress values
        if (_isReeling)
        {
            _currentReelProgress -= Mathf.Max(0f, Time.deltaTime * FishingManager.Instance.ReelDecayRate); // Decay progress over time
            _progressSlider.value = Mathf.Min(_currentReelProgress, _progressSlider.maxValue);
        }
    }

    /// <summary>
    /// Sets up the progress slider for reeling and shows the UI.
    /// </summary>
    public void StartReel()
    {
        if (_progressSlider == null)
        {
            Debug.LogWarning("Reel progress slider is not assigned in the inspector.");
            return;
        }
        
        _isReeling = true;
        _currentReelProgress = 0f;

        // Slider Setup
        Show(true);
        _progressSlider.maxValue = FishingManager.Instance.ReelTotalProgress;
        _progressSlider.value = 0f;
    }

    /// <summary>
    /// Increase reel progress by an amount
    /// </summary>
    public void ProgressReel()
    {
        // Update reel progress
        _currentReelProgress += FishingManager.Instance.ReelProgressAmount;

        // Check if the reel progress is complete
        if (_currentReelProgress >= FishingManager.Instance.ReelTotalProgress)
        {
            StopReel();
            ReelCompleted.Invoke();
        }
        else
        {
            ReelProgressed.Invoke();
        }
    }

    /// <summary>
    /// Stops the reel progress and hides the UI.
    /// </summary>
    public void StopReel()
    {
        if (_progressSlider == null)
        {
            Debug.LogWarning("Reel progress slider is not assigned in the inspector.");
            return;
        }
        Show(false);
        _isReeling = false;
        _progressSlider.value = 0f;
    }
}
