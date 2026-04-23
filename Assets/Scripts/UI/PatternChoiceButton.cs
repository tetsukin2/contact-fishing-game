using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatternChoiceButton : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Button _button;

    private string _sequenceName;
    private System.Action<string> _onPressed;

    public void Setup(string displayName, string sequenceName, System.Action<string> onPressed)
    {
        _sequenceName = sequenceName;
        _onPressed = onPressed;

        if (_label != null)
            _label.text = displayName;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandlePressed);
        }
    }

    private void HandlePressed()
    {
        _onPressed?.Invoke(_sequenceName);
    }
}