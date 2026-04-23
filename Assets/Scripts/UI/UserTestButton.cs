using UnityEngine;

public class UserTestButton : MonoBehaviour
{
    [SerializeField] private OnboardingPopupUI _onboardingPopup;

    public void OnClickUserTest()
    {
        Debug.Log("USER TEST BUTTON CLICKED");

        UserTestConfig.IsUserTestMode = true;
        UserTestConfig.CurrentPhase = 1;          // 1 = no haptics
        UserTestConfig.HapticsEnabled = false;
        UserTestConfig.OverrideFishTotalToCatch = 2;

        if (_onboardingPopup != null)
        {
            _onboardingPopup.OpenPopup();
        }
        else
        {
            Debug.LogWarning("OnboardingPopup not assigned on UserTestButton.");
        }
    }
}