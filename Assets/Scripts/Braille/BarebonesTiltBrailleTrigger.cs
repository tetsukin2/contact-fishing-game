using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarebonesTiltBrailleTrigger : MonoBehaviour
{
    private enum RotationDirection
    {
        Forward, Back, Left, Right
    }

    [SerializeField] private float rotationThreshold = 15f; // Threshold for rotation detection

    private RotationDirection _currentDirection;
    private void OnTilt(RotationDirection direction)
    {
        if (_currentDirection == direction) return; // Ignore if the same direction
        _currentDirection = direction; // Update current direction

        BraillePatternPlayer.Instance.StopPatternSequence(); // Stop any ongoing patterns
        switch (direction)
        {
            case RotationDirection.Forward:
                BraillePatternPlayer.Instance.PlayPatternSequence("WaveOut", true);
                break;
            case RotationDirection.Back:
                BraillePatternPlayer.Instance.PlayPatternSequence("WaveIn", true);
                break;
            case RotationDirection.Left:
                BraillePatternPlayer.Instance.PlayPatternSequence("WaveLeft", true);
                break;
            case RotationDirection.Right:
                BraillePatternPlayer.Instance.PlayPatternSequence("WaveRight", true);
                break;
        }
    }

    private void Update()
    {
        if (!InputDeviceManager.Instance.BLEDevice.IsConnected) return;

        if (InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(rotationThreshold))
        {
            OnTilt(RotationDirection.Forward);
        }
        else if (InputDeviceManager.Instance.RotationHelper.HasReachedRotationX(-rotationThreshold))
        {
            OnTilt(RotationDirection.Back);
        }
        else if (InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(rotationThreshold))
        {
            OnTilt(RotationDirection.Right);
        }
        else if (InputDeviceManager.Instance.RotationHelper.HasReachedRotationY(-rotationThreshold))
        {
            OnTilt(RotationDirection.Left);
        }
    }
}
