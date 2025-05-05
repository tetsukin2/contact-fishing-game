using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishTargeting : MonoBehaviour
{
    public bool CanChangeSelection = false;
    
    [SerializeField] private List<FishEntity> fishes = new();
    [Min(1)]
    [SerializeField] private int _fishSpawnCount;
    [SerializeField] private Vector2 _spawnCorner1;
    [SerializeField] private Vector2 _spawnCorner2;
    [SerializeField] private float _minSpawnDistance = 2f;
    [SerializeField] private GameObject _fishPrefab;

    private FishEntity selectedFish = null;
    private Vector2 _lastJoystickInput = Vector2.zero;
    private Vector2 _defaultJoystickinput = Vector2.zero;

    public FishEntity Selection
    {
        get {
            // For safety, such as during initialization
            if (selectedFish == null)
                SetRandomFishAsSelected();
            return selectedFish;
        }
    }

    private void Start()
    {
        SpawnFishes();
    }

    private void Update()
    {
        if (CanChangeSelection)
            HandleJoystickInput();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(_spawnCorner1.x, 0, _spawnCorner1.y), new Vector3(_spawnCorner2.x, 0, _spawnCorner1.y));
        Gizmos.DrawLine(new Vector3(_spawnCorner2.x, 0, _spawnCorner1.y), new Vector3(_spawnCorner2.x, 0, _spawnCorner2.y));
        Gizmos.DrawLine(new Vector3(_spawnCorner2.x, 0, _spawnCorner2.y), new Vector3(_spawnCorner1.x, 0, _spawnCorner2.y));
        Gizmos.DrawLine(new Vector3(_spawnCorner1.x, 0, _spawnCorner2.y), new Vector3(_spawnCorner1.x, 0, _spawnCorner1.y));
    }

    private void SpawnFishes()
    {
        for (int i = 0; i < _fishSpawnCount; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            GameObject fishObject = Instantiate(_fishPrefab, spawnPosition, Quaternion.identity);
            FishEntity fishEntity = fishObject.GetComponent<FishEntity>();
            fishes.Add(fishEntity);
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPosition;
        int maxAttempts = 100; // Prevent infinite loops
        int attempts = 0;

        do
        {
            float x = Random.Range(_spawnCorner1.x, _spawnCorner2.x);
            float z = Random.Range(_spawnCorner1.y, _spawnCorner2.y);
            spawnPosition = new Vector3(x, 0, z);
            attempts++;
        } while (!IsPositionValid(spawnPosition) && attempts < maxAttempts);

        if (attempts >= maxAttempts)
            Debug.LogWarning("Could not find a valid spawn position after multiple attempts.");

        return spawnPosition;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (var fish in fishes)
        {
            if (Vector3.Distance(position, fish.transform.position) < _minSpawnDistance)
                return false;
        }
        return true;
    }

    public void HandleJoystickInput()
    {
        Vector2 input = InputDeviceManager.JoystickInput;
        if (input != _defaultJoystickinput && input != _lastJoystickInput)
        {
            // Find direction and select nearest fish in that direction
            FishEntity selected = GetFishInDirection(input);
            if (selected != null)
            {
                CameraController.Instance.FishSelectVCam.LookAt = selected.transform;
                //StartCoroutine(CastBobberToFish(selected));
                SelectNewFish(selected);    
            }
        }

        _lastJoystickInput = input;
    }

    public void SetRandomFishAsSelected()
    {
        SelectNewFish(fishes[Random.Range(0, fishes.Count)]);
    }

    private void SelectNewFish(FishEntity fish)
    {
        selectedFish?.SetSelected(false);
        selectedFish = fish;
        selectedFish?.SetSelected(true);
    }

    public void LureFish()
    {
        if (selectedFish != null)
        {
            selectedFish.LureFish();
            selectedFish.SetSelected(false);
            //selectedFish = null;
        }
    }

    public void CatchSelected()
    {
        if (selectedFish == null) return;

        Vector3 newPosition = GetValidSpawnPosition();
        selectedFish.transform.position = newPosition;
        selectedFish.SetSelected(false); // Deselect the fish after moving
        selectedFish.ReleaseFish();
        selectedFish = null; // Clear the selection
    }

    /// <summary>
    /// Looks for the physical fish, not the entity
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    FishEntity GetFishInDirection(Vector2 input)
    {
        if (fishes == null || fishes.Count == 0) return null;

        Vector3 inputDir = new Vector3(input.x, 0, input.y).normalized;

        // If no fish is selected yet, use the targeting system's position as fallback
        Vector3 origin = selectedFish != null ? selectedFish.Fish.transform.position : transform.position;

        FishEntity bestMatch = null;
        float bestDot = -1f;
        float closestDistance = Mathf.Infinity;

        foreach (var fish in fishes)
        {
            if (fish == selectedFish) continue; // don't reselect the same fish

            Vector3 toFish = (fish.Fish.transform.position - origin).normalized;
            float dot = Vector3.Dot(inputDir, toFish);

            if (dot > 0.5f) // ensure it's roughly in the desired direction (avoid side/back selection)
            {
                float dist = Vector3.Distance(fish.Fish.transform.position, origin);
                if (dot > bestDot || (Mathf.Approximately(dot, bestDot) && dist < closestDistance))
                {
                    bestDot = dot;
                    closestDistance = dist;
                    bestMatch = fish;
                }
            } 
        }
        return bestMatch;
    }

    //IEnumerator CastBobberToFish(FishEntity fish)
    //{
    //    Transform bobberTransform = /* your bobber transform reference here */;
    //    bobberTransform.position = fish.transform.position;

    //    yield return new WaitForSeconds(1.5f); // delay before attach

    //    // Optional: visual effect for attaching
    //    fish.gameObject.SetActive(false); // hide caught fish

    //    SpawnNewFish();
    //    hasSelected = false; // allow selection again if needed
    //}

    //void SpawnNewFish()
    //{
    //    // Instantiate new fish elsewhere
    //    Vector3 newCenter = /* some logic for new location */;
    //    Instantiate(_fishPrefab, newCenter, Quaternion.identity).GetComponent<FishEntity>().centerPoint = newCenter;
    //}

    
}
