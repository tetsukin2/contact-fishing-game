using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaPanel : GUIPanel
{
    [SerializeField] private JoystickCursorSelectable[] _fishSelectables;
    [SerializeField] private ButtonCursorSelectable _nextButton;
    [SerializeField] private ButtonCursorSelectable _previousButton;

    private int _currentFishIndex = 0;

    private void Start()
    {
        _nextButton.onSelect.AddListener(OnSetNextFish);
        _previousButton.onSelect.AddListener(OnSetPreviousFish);
        RefreshFishes();
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
    /// Refreshes the fish selectables, hiding all but the current one.
    /// </summary>
    private void RefreshFishes()
    {
        foreach (JoystickCursorSelectable fishSelectable in _fishSelectables)
        {
            fishSelectable.isSelectable = false;
            fishSelectable.Show(false);
        }
        _fishSelectables[_currentFishIndex].Show(true);
        _fishSelectables[_currentFishIndex].isSelectable = true;
    }
}
