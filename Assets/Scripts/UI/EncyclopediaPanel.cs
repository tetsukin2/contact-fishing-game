using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class EncyclopediaPanel : GUIPanel
{
    [SerializeField] private FishSelectable[] _fishSelectables;
    [SerializeField] private ButtonCursorSelectable _nextButton;
    [SerializeField] private ButtonCursorSelectable _previousButton;
    [SerializeField] private ButtonCursorSelectable _deleteDataButton;

    private int _currentFishIndex = 0;

    private void Start()
    {
        _nextButton.onSelect.AddListener(OnSetNextFish);
        _previousButton.onSelect.AddListener(OnSetPreviousFish);
        _deleteDataButton.onSelect.AddListener(OnDeleteData);
        RefreshFishes();
    }

    public override void Show(bool show)
    {
        base.Show(show);
        if (show)
        {
            UpdateFishData();
        }
    }

    private void UpdateFishData()
    {
        foreach (FishSelectable fishSelectable in _fishSelectables)
        {
            GameData gameData = GameManager.Instance.CurrentGameData;
            if (gameData != null) {

                // DEBUG TEMP UNLOCK
                //if (!gameData.UnlockedFish.Contains(fishSelectable.FishID))
                //    gameData.UnlockedFish.Add(fishSelectable.FishID);

                fishSelectable.SetDiscovered(gameData.UnlockedFish.Contains(fishSelectable.FishID));
            }
            else
            {
                fishSelectable.SetDiscovered(false);
            }
            
        }
    }

    private void OnDeleteData()
    {
        GameManager.Instance.DeleteData();
        UpdateFishData();
    }

    private void OnSetNextFish()
    {
        _currentFishIndex = (_currentFishIndex + 1) % _fishSelectables.Length;
        RefreshFishes();
    }

    private void OnSetPreviousFish()
    {
        _currentFishIndex = (_currentFishIndex - 1 + _fishSelectables.Length) % _fishSelectables.Length;
        RefreshFishes();
    }

    /// <summary>
    /// Refreshes the fish selectables, hiding all but the current one and updating discovered status.
    /// </summary>
    private void RefreshFishes()
    {
        foreach (FishSelectable fishSelectable in _fishSelectables)
        {
            fishSelectable.IsSelectable = false;
            fishSelectable.Show(false);
            fishSelectable.SetDiscovered(GameManager.Instance.CurrentGameData.HasDiscoveredFish(fishSelectable.FishID));
        }
        _fishSelectables[_currentFishIndex].Show(true);
        _fishSelectables[_currentFishIndex].IsSelectable = true;
    }
}
