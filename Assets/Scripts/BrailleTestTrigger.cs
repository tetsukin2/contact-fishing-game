using System.Collections;
using UnityEngine;

public class BrailleTestTrigger : MonoBehaviour
{
    private bool lastPressedState = false;

    private bool patternActive = false;
    [SerializeField] private float _patternInterval = 0.1f;

    private Coroutine _patternCoroutine;

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
                StartCoroutine(WavePattern());
            else
                StopCoroutine(WavePattern());

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

    private IEnumerator WavePattern()
    {
        WaitForSeconds interval = new(_patternInterval);

        while (true) 
        {
            InputDeviceManager.SendBrailleASCII(0, 184);
            yield return interval;
            InputDeviceManager.SendBrailleASCII(0, 71);
            yield return interval;
            InputDeviceManager.SendBrailleASCII(184, 0);
            yield return interval;
            InputDeviceManager.SendBrailleASCII(71, 0);
            yield return interval;
            
        }
        
    }
}
