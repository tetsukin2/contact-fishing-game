using TMPro;
using UnityEngine;

/// <summary>
/// Overall GUI for main gameplay
/// </summary>
public class GameplayGUI : GUIContainer
{
    [SerializeField] private TextMeshProUGUI _timerText;

    private void Update()
    {
        //only run if timer object exists
        if (_timerText)
            _timerText.text = GameDataHandler.ConvertToTimeFormat(LevelManager.Instance.Timer);
    }
}
