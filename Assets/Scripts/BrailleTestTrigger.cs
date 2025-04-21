using UnityEngine;

public class BrailleTestTrigger : MonoBehaviour
{
    private bool lastPressedState = false;

    void Update()
    {
        if (!WindowsBLEScanner.IsConnected) return;

        // Only send when state changes
        if (WindowsBLEScanner.joystickPressed != lastPressedState)
        {
            lastPressedState = WindowsBLEScanner.joystickPressed;

            if (WindowsBLEScanner.joystickPressed)
            {
                Debug.Log("🧪 Thumbstick Pressed → Sending <255255>");
                WindowsBLEScanner.SendBrailleASCII(255, 255);
            }
            else
            {
                Debug.Log("🧪 Thumbstick Released → Sending <000000>");
                WindowsBLEScanner.SendBrailleASCII(0, 0);
            }
        }
    }
}
