using TMPro;
using UnityEngine;

/// <summary>
/// Handles loading screen logic.
/// </summary>
public class LoadingScreen : GUIContainer
{
    [SerializeField] private TextMeshProUGUI _loadingText;

    private void Start()
    {
        // Initialize loading screen
        Show(false);
        InputDeviceManager.Instance.BLEDevice.ConnectionAttemptStarted.AddListener(OnConnectionStarted);
    }

    private void OnConnectionStarted()
    {
        // Show loading screen and do updates
        Show(true);
        InputDeviceManager.Instance.StatusUpdated.AddListener((string message) => _loadingText.SetText(message));
        InputDeviceManager.Instance.BLEDevice.CharacteristicsLoaded.AddListener(OnCharacteristicsLoaded);
    }

    private void OnCharacteristicsLoaded()
    {
        // Hide loading screen when characteristics are loaded
        Show(false);
        InputDeviceManager.Instance.StatusUpdated.RemoveListener((string message) => _loadingText.SetText(message));
        InputDeviceManager.Instance.BLEDevice.CharacteristicsLoaded.RemoveListener(OnCharacteristicsLoaded);
    }
}
