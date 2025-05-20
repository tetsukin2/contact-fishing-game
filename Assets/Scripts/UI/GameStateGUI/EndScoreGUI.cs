using TMPro;
using UnityEngine;

/// <summary>
/// Overall GUI for end score (After playing when final score is shown)
/// </summary>
public class EndScoreGUI : GUIContainer
{
    [SerializeField] private GUIContainer _gameEndSelectGUI;
    [SerializeField] private TextMeshProUGUI _gameEndSessionText;
    [SerializeField] private TextMeshProUGUI _gameEndBestText;
    [SerializeField] private Color _gameEndBestTextColorNormal;
    [SerializeField] private Color _gameEndBestTextColorNew;

    private void Start()
    {
        GameManager.Instance.ScoreProcessed.AddListener(OnScoreProcessed);
    }

    public override void Show(bool show)
    {
        base.Show(show);
        _gameEndSelectGUI.Show(show);
    }

    // Setup New Best details
    private void OnScoreProcessed()
    {
        Debug.Log("attempting to show score");
        _gameEndSessionText.text = $"Nice Haul! {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.Timer)}";
        if (GameManager.Instance.NewBestAchieved)
        {
            _gameEndBestText.color = _gameEndBestTextColorNew;
            _gameEndBestText.text = "New personal best!";
        }
        else
        {
            _gameEndBestText.color = _gameEndBestTextColorNormal;
            _gameEndBestText.text = $"Can you top your best of {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.CurrentGameData.BestTime)}?";
        }
    }
}
