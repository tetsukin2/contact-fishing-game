using System.Collections;
using UnityEngine;

public class BrailleTestTrigger : MonoBehaviour
{
    private bool lastPressedState = false;

    private bool patternActive = false;

    void Update()
    {
        if (!InputDeviceManager.IsConnected) return;

        // Only send when state changes
        if (InputDeviceManager.joystickPressed != lastPressedState)
        {
            lastPressedState = InputDeviceManager.joystickPressed;

            //toggle
            if (InputDeviceManager.joystickPressed) patternActive = !patternActive;

            if (patternActive)
                BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut");
            else
                BraillePatternPlayer.Instance.StopPatternSequence();

            //if (InputDeviceManager.joystickPressed)
            //{
            //    Debug.Log("🧪 Thumbstick Pressed → Sending <255255>");
            //    InputDeviceManager.SendBrailleASCII(255, 255);
            //}
            //else
            //{
            //    Debug.Log("🧪 Thumbstick Released → Sending <000000>");
            //    InputDeviceManager.SendBrailleASCII(0, 0);
            //}
        }
    }
}
