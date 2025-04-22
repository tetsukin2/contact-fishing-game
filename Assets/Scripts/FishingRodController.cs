using System.Collections.Generic;
using UnityEngine;

public class FishingRodController : MonoBehaviour
{
    [SerializeField] private Rigidbody hookRigidbody;  // Assign Hook Rigidbody in Inspector
    [SerializeField] private FishingLineController fishingLineController;  // Assign Fishing Line Controller in Inspector

    public float castForce = 10f;  // Adjust casting strength
    public float RotationTriggerThreshold = 15f;  // Rotation threshold in degrees
    public float TriggerTimeWindow = 0.5f;  // Time window to detect rotation (in seconds)
    public Vector3 RotationAxis = Vector3.right;  // Axis to track rotation (e.g., Vector3.right for x-axis)

    private Queue<RotationData> rotationHistory = new();  // Stores rotation changes with timestamps
    private Vector3 previousRotation;  // Store the previous rotation of the IMU device
    private float lastMeasuredAngle = 0f;  // Last measured angle from trigger

    void Start()
    {
        // Initialize the previous rotation with the current IMU rotation
        previousRotation = InputDeviceManager.IMURotation;
    }

    void Update()
    {
        // Track the rotation change in the current frame
        TrackRotation();

        Debug.Log(InputDeviceManager.IMURotation);

        // Check if the rotation change reaches the trigger threshold within the time window
        if (HasRotatedByDegrees(-RotationTriggerThreshold, InputDeviceManager.RotationAxis.y))
        {
            CastLine();
            ClearRotationHistory();
        }

        // Reel in when holding Left Click
        if (InputDeviceManager.joystickPressed)
        {
            ReelIn();
        }
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
        while (rotationHistory.Count > 0 && Time.time - rotationHistory.Peek().Timestamp > TriggerTimeWindow)
        {
            rotationHistory.Dequeue();
        }

        // Update the previous rotation
        previousRotation = currentRotation;
    }    

    bool HasRotatedByDegrees(float angle, InputDeviceManager.RotationAxis axis)
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
                lastMeasuredAngle = cumulativeRotation;  // Store the last measured angle
                return true;  // Threshold reached  
            }
        }

        return false;  // Threshold not reached  
    }

    void ClearRotationHistory()
    {
        rotationHistory.Clear();
    }

    void CastLine()
    {
        // Apply velocity to the hook based on the rod tip's forward direction
        fishingLineController.Cast(castForce * Mathf.Abs(lastMeasuredAngle), 0.1f);
        Debug.Log("Casting Fishing Line!");
    }

    void ReelIn()
    {
        //Vector3 pullDirection = (rodTip.position - hookRigidbody.position).normalized;
        //hookRigidbody.AddForce(0.5f * castForce * pullDirection, ForceMode.Acceleration);
        fishingLineController.SetLimitedLength(true);
        Debug.Log("Reeling In!");
    }

    // Helper class to store rotation data
    private class RotationData
    {
        public Vector3 Rotation;  // The angle of rotation
        public float Timestamp;  // The time at which the rotation occurred
    }
}
