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

        //Debug.Log(InputDeviceManager.IMURotation);
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

}
