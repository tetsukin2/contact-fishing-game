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

        GameManager.Instance.FishCaughtUpdated.AddListener(UpdateFishes);
        GameManager.Instance.GameStateEntered.AddListener(OnGameStateEntered);

        // Initialize fish related display
        UpdateFishes(GameManager.Instance.FishCaught);
    }

    private void OnGameStateEntered(GameState newState)
    {
        if (newState == GameManager.Instance.PlayingState
            || newState == GameManager.Instance.GameEndState)
        {
            // Show the fish bucket when the game is playing, and update fishes in case
            _bucketUI.SetActive(true);
            UpdateFishes(GameManager.Instance.FishCaught);
        }
        else
        {
            // Hide the fish bucket when the game is not playing
            _bucketUI.SetActive(false);
        }
    }

    private void UpdateFishes(int caught)
    {
        // Progress as percentage, multiplied by length
        float fishCaughtProgress = ( (float)caught / (float)GameManager.Instance.FishTotalToCatch);

        // Determine how many fish objects should be visible based on the progress
        int visibleFishCount = Mathf.CeilToInt(fishCaughtProgress * _fishes.Length);

        // Update the visibility of each fish object
        for (int i = 0; i < _fishes.Length; i++)
        {
            _fishes[i].SetActive(i < visibleFishCount);
        }

        // Update the text to show the number of fish caught
        _fishCaughtNumberText.text = $"{caught}/{GameManager.Instance.FishTotalToCatch}";
    }
}
