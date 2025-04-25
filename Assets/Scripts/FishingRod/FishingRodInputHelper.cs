using System.Collections.Generic;
using UnityEngine;

public class FishingRodInputHelper : MonoBehaviour
{
    // Helper class to store rotation data
    private class RotationData
    {
        public Vector3 Rotation;  // The angle of rotation
        public float Timestamp;  // The time at which the rotation occurred
    }

    [SerializeField] private FishingManager fishingStateManager;  // Assign Hook Rigidbody in Inspector
    [SerializeField] private FishingBobber fishingBobber;  // Assign Fishing Line Controller in Inspector

    public float InputReadWindow = 0.5f;  // Time window to detect rotation (in seconds)

    private Queue<RotationData> rotationHistory = new();  // Stores rotation changes with timestamps
    private Vector3 previousRotation;  // Store the previous rotation of the IMU device
    public float LastMeasuredAngle { get; private set; } = 0f; // Last measured angle from trigger

    // Joystick rotation tracking
    private bool _trackJoystickClockwise = true; // Flag to track clockwise rotation
    private float _previousJoystickAngle = 0f; // Previous angle of the joystick
    private float _cumulativeJoystickAngle = 0f; // Cumulative angular change
    private int _rotationCount = 0; // Number of full rotations

    // So we don't undo a bunch of progress by going the other way
    public bool TrackJoystickClockwise
    {
        get => _trackJoystickClockwise;
        set => _trackJoystickClockwise = value;
    }

    void Start()
    {
        // Setup the previous rotation with the current IMU rotation
        previousRotation = InputDeviceManager.IMURotation;
    }

    void Update()
    {
        if (!InputDeviceManager.IsConnected) return;

        // Track the rotation change in the current frame
        TrackRotation();

        // Track joystick rotations
        TrackJoystickRotations();
    }

    void TrackRotation()
    {
        // Get the current rotation from the IMU device
        Vector3 currentRotation = InputDeviceManager.IMURotation;

        // Calculate the angular difference around the specified axis
        Vector3 rotationDifference = currentRotation - previousRotation;

        // Record the rotation change with a timestamp
        rotationHistory.Enqueue(new RotationData
        {
            Rotation = rotationDifference,
            Timestamp = Time.time
        });

        // Remove old entries outside the time window
        while (rotationHistory.Count > 0 && Time.time - rotationHistory.Peek().Timestamp > InputReadWindow)
        {
            rotationHistory.Dequeue();
        }

        // Update the previous rotation
        previousRotation = currentRotation;
    }

    public bool HasRotatedByDegrees(float angle, InputDeviceManager.RotationAxis axis)
    {
        float cumulativeRotation = 0f;
        float angleRadians = angle * Mathf.Deg2Rad;  // Convert angle to radians

        foreach (var rotationData in rotationHistory)
        {
            switch (axis)
            {
                case InputDeviceManager.RotationAxis.x:
                    cumulativeRotation += rotationData.Rotation.x;
                    break;
                case InputDeviceManager.RotationAxis.y:
                    cumulativeRotation += rotationData.Rotation.y;
                    break;
                case InputDeviceManager.RotationAxis.z:
                    cumulativeRotation += rotationData.Rotation.z;
                    break;
            }

            if ((angle < 0 && cumulativeRotation <= angleRadians) || (angle >= 0 && cumulativeRotation >= angleRadians))
            {
                LastMeasuredAngle = cumulativeRotation;  // Store the last measured angle
                return true;  // Threshold reached  
            }
        }

        return false;  // Threshold not reached  
    }

    public void ClearRotationHistory()
    {
        rotationHistory.Clear();  // Clear the rotation history
    }

    /// <summary>
    /// Tracks the number of full circular rotations made by the joystick.
    /// </summary>
    private void TrackJoystickRotations()
    {
        // Get the current joystick input
        Vector2 joystickInput = InputDeviceManager.joystickInput;

        // Ignore if the joystick is not being moved
        if (joystickInput == Vector2.zero) return;

        // Calculate the current angle of the joystick relative to its center
        float currentAngle = Mathf.Atan2(joystickInput.y, joystickInput.x) * Mathf.Rad2Deg;

        // Handle angle wrapping (e.g., from 179 to -180 degrees)
        float angleDelta = Mathf.DeltaAngle(_previousJoystickAngle, currentAngle);

        //Debug.Log(angleDelta);

        // Accumulate the angular change
        _cumulativeJoystickAngle += angleDelta;

        // Check if a full rotation (360 degrees) has been completed
        if (Mathf.Abs(_cumulativeJoystickAngle) >= 360f)
        {
            // Determine the direction of the rotation
            bool isClockwise = Mathf.Sign(_cumulativeJoystickAngle) < 0;

            // Increment or decrement the rotation count based on the direction and TrackJoystickClockwise
            // We don't want to undo progress by going the other way
            if ((isClockwise && _trackJoystickClockwise) || (!isClockwise && !_trackJoystickClockwise))
            {
                _rotationCount += (int)Mathf.Sign(_cumulativeJoystickAngle);
            }

            // Reset the cumulative angle, keeping the overflow
            _cumulativeJoystickAngle %= 360f;

            Debug.Log($"Joystick Rotations: {_rotationCount}");
        }

        // Update the previous angle for the next frame
        _previousJoystickAngle = currentAngle;
    }

    /// <summary>
    /// Gets the total number of full joystick rotations.
    /// </summary>
    public int GetJoystickRotationCount(bool isClockwise)
    {
        return (isClockwise) ? -_rotationCount : _rotationCount;
    }

    /// <summary>
    /// Resets the joystick rotation count and cumulative angle.
    /// </summary>
    public void ResetJoystickRotationCount()
    {
        _rotationCount = 0;
        _cumulativeJoystickAngle = 0f;
        _previousJoystickAngle = 0f;
    }
}
