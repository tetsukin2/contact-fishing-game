using UnityEngine;

/// <summary>
/// 
/// </summary>
public class EncyclopediaGUI : GUIContainer
{
    [SerializeField] private FishSelectable[] _fishSelectables;
    [SerializeField] private ButtonCursorSelectable _nextButton;
    [SerializeField] private ButtonCursorSelectable _previousButton;
    [SerializeField] private ButtonCursorSelectable _deleteDataButton;

    private int _currentFishIndex = 0;

    private void Start()
    {
        // Button setup
        _nextButton.onSelect.AddListener(OnSetNextFish);
        _previousButton.onSelect.AddListener(OnSetPreviousFish);
        _deleteDataButton.onSelect.AddListener(OnDeleteData);
    }

    public override void Show(bool show)
    {
        base.Show(show);
        if (show)
        {
            // Update & refresh fishes in case data is different the next time this is shown
            UpdateFishData();
            RefreshFishes();

            // Show navigation instructions
            UIManager.Instance.ShowMainInputPrompt(UIManager.Instance.EncyclopediaInput);
            UIManager.Instance.ShowSecondInputPrompt(UIManager.Instance.EncyclopediaSecondInput);
        }
    }

    /// <summary>
    /// Updates the discovered status of each fish selectable based on the current game data.
    /// </summary>
    private void UpdateFishData()
    {
        foreach (FishSelectable fishSelectable in _fishSelectables)
        {
            GameData gameData = GameManager.Instance.CurrentGameData;
            if (gameData != null)
            {
                fishSelectable.SetDiscovered(gameData.HasDiscoveredFish(fishSelectable.FishID));
            }
            else
            {
                fishSelectable.SetDiscovered(false);
            }

        }
    }

    /// <summary>
    /// Handles the deletion of game data when the delete data button is pressed.
    /// </summary>
    private void OnDeleteData()
    {
        GameManager.Instance.DeleteData();
        UpdateFishData();
        RefreshFishes();
    }

    /// <summary>
    /// Sets the next fish as visible and selectable.
    /// </summary>
    private void OnSetNextFish()
    {
        _currentFishIndex = (_currentFishIndex + 1) % _fishSelectables.Length;
        RefreshFishes();
    }

    /// <summary>
    /// Sets the previous fish as visible and selectable.
    /// </summary>
    private void OnSetPreviousFish()
    {
        _currentFishIndex = (_currentFishIndex - 1 + _fishSelectables.Length) % _fishSelectables.Length;
        RefreshFishes();
    }

    /// <summary>
    /// Refreshes the fish selectables, hiding all from selection but the current one.
    /// </summary>
    private void RefreshFishes()
    {
        foreach (FishSelectable fishSelectable in _fishSelectables)
        {
            fishSelectable.IsSelectable = false;
            fishSelectable.Show(false);
        }
        _fishSelectables[_currentFishIndex].Show(true);
        _fishSelectables[_currentFishIndex].IsSelectable = true;
    }
}
