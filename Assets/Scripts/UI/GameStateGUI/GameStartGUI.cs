using UnityEngine;

/// <summary>
/// Overall GUI for game start (right before playing)
/// </summary>
public class GameStartGUI : GUIContainer
{
    [SerializeField] private GameObject _ready;
    [SerializeField] private GameObject _set;
    [SerializeField] private GameObject _fish;

    private void Start()
    {
        // Listening to a specific state's events here
        LevelManager.Instance.GameStartState.GameStartStageReached.AddListener(OnGameStartStageReached);
    }

    // For each game start stage being reached, show a bit more of the ready text
    private void OnGameStartStageReached(int stage)
    {
        //Debug.Log($"Game start stage reached: {stage}");
        switch (stage)
        {
            case 0:
                OnReady();
                break;
            case 1:
                OnSet();
                break;
            case 2:
                OnFish();
                break;
        }
    }

    private void OnReady()
    {
        _ready.SetActive(true);
        _set.SetActive(false);
        _fish.SetActive(false);
    }

    private void OnSet()
    {
        _ready.SetActive(true);
        _set.SetActive(true);
        _fish.SetActive(false);
    }

    private void OnFish()
    {
        _ready.SetActive(true);
        _set.SetActive(true);
        _fish.SetActive(true);
    }
}
