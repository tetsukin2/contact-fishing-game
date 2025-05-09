using System.Collections;
using UnityEngine;

public class BrailleTestTrigger : MonoBehaviour
{
    [SerializeField] private string _thumbPatternSequence;
    [SerializeField] private string _indexPatternSequence;
    [SerializeField] private bool _active = false;

    private bool lastPressedState = false;

    private bool patternActive = false;

    void Update()
    {
        if (!InputDeviceManager.IsConnected) return;

        if (_active && !patternActive)
        {
            patternActive = true;
            //BraillePatternPlayer.Instance.PlayPatternSequence(_thumbPatternSequence, true);
            BraillePatternPlayer.Instance.PlayPatternSequence(_thumbPatternSequence, BraillePatternPlayer.Finger.THUMB, true);
            BraillePatternPlayer.Instance.PlayPatternSequence(_indexPatternSequence, BraillePatternPlayer.Finger.INDEX, true);
        }
        else if (!_active && patternActive)
        {
            patternActive = false;
            BraillePatternPlayer.Instance.StopPatternSequence(BraillePatternPlayer.Finger.BOTH);
            //BraillePatternPlayer.Instance.StopPatternSequence();
        }

        // Only send when state changes
        if (InputDeviceManager.JoystickHeld != lastPressedState)
        {
            lastPressedState = InputDeviceManager.JoystickHeld;

            //toggle
            //if (InputDeviceManager.JoystickHeld) patternActive = !patternActive;

            //if (patternActive)
            //{
            //    BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", BraillePatternPlayer.Finger.THUMB, true);
            //    BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", BraillePatternPlayer.Finger.INDEX, true);
            //}
                
            //else
            //{
            //    BraillePatternPlayer.Instance.StopPatternSequence(BraillePatternPlayer.Finger.BOTH);
            //}
                

            //if (InputDeviceManager.JoystickHeld)
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
