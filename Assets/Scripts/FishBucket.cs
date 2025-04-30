using UnityEngine;

public class FishBucket : MonoBehaviour
{
    [SerializeField] private GameObject[] _fishes;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.FishCaughtUpdated.AddListener(UpdateFishes);
        UpdateFishes();
    }

    private void UpdateFishes()
    {
        // Progress as percentage, multiplied by length
        // Dunno why float cast is grayed out but it's important for the calc to work
        float fishCaughtProgress = ( (float)GameManager.Instance.FishCaught / (float)GameManager.Instance.FishTotalToCatch);

        // Determine how many fish objects should be visible based on the progress
        int visibleFishCount = Mathf.CeilToInt(fishCaughtProgress * _fishes.Length);

        // Update the visibility of each fish object
        for (int i = 0; i < _fishes.Length; i++)
        {
            _fishes[i].SetActive(i < visibleFishCount);
        }
    }
}
