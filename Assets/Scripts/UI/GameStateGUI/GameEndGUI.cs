/// <summary>
/// Overall GUI for game end (right after all fishes are caught)
/// </summary>
public class GameEndGUI : GUIContainer
{
    private void Start()
    {
        GameManager.Instance.GameStateEntered.AddListener(OnGameStateEntered);
    }

    // Game Start UI Setup
    private void OnGameStateEntered(GameState newState)
    {
        Show(newState == GameManager.Instance.GameEndState);
    }
}
