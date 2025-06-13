using TMPro;
using UnityEngine;

public class FishBucket : MonoBehaviour
{
    [SerializeField] private GameObject _bucketUI;
    [SerializeField] private TextMeshProUGUI _fishCaughtNumberText;
    [SerializeField] private GameObject[] _fishes;

    void Start()
    {
        // Hide the fish bucket UI at the start
        _bucketUI.SetActive(false); 

        LevelManager.Instance.FishCaughtUpdated.AddListener(UpdateFishes);
        LevelManager.Instance.GamePaused.AddListener(OnGamePaused);
        LevelManager.Instance.GameStateEntered.AddListener(OnGameStateEntered);

        // Initialize fish related display
        UpdateFishes(LevelManager.Instance.FishCaught);
    }

    private void OnGameStateEntered(LevelState newState)
    {
        if (newState == LevelManager.Instance.PlayingState
            || newState == LevelManager.Instance.GameEndState)
        {
            // Show the fish bucket when the game is playing, and update fishes in case
            _bucketUI.SetActive(true);
            UpdateFishes(LevelManager.Instance.FishCaught);
        }
        else
        {
            // Hide the fish bucket when the game is not playing
            _bucketUI.SetActive(false);
        }
    }

    private void OnGamePaused(bool isPaused)
    {
        if (LevelManager.Instance.CurrentState != LevelManager.Instance.PlayingState) return;

        _bucketUI.SetActive(!isPaused);
        if (!isPaused) UpdateFishes(LevelManager.Instance.FishCaught);
    }

    private void UpdateFishes(int caught)
    {
        // Progress as percentage, multiplied by length
        float fishCaughtProgress = ( (float)caught / (float)LevelManager.Instance.FishTotalToCatch);

        // Determine how many fish objects should be visible based on the progress
        int visibleFishCount = Mathf.CeilToInt(fishCaughtProgress * _fishes.Length);

        // Update the visibility of each fish object
        for (int i = 0; i < _fishes.Length; i++)
        {
            _fishes[i].SetActive(i < visibleFishCount);
        }

        // Update the text to show the number of fish caught
        _fishCaughtNumberText.text = $"{caught}/{LevelManager.Instance.FishTotalToCatch}";
    }
}
