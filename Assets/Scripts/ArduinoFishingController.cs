using UnityEngine;

public class ArduinoFishingController : MonoBehaviour
{
    public Transform FishingRod;
    public float sensitivity = 100f;
    public float smoothFactor = 0.2f;
    private Vector3 rodRotation = Vector3.zero;
    private Vector3 velocity = Vector3.zero; 

    void FixedUpdate()
    {
        if (!InputDeviceManager.IsConnected) return;
        ReadIMUData();
    }

    void ReadIMUData()
    {
        Vector3 imuData = InputDeviceManager.IMURotation;

        rodRotation.x = Mathf.SmoothDamp(rodRotation.x, -imuData.x * sensitivity, ref velocity.x, smoothFactor);
        rodRotation.y = Mathf.SmoothDamp(rodRotation.y, imuData.y * sensitivity, ref velocity.y, smoothFactor);
        rodRotation.z = Mathf.SmoothDamp(rodRotation.z, imuData.z * sensitivity, ref velocity.z, smoothFactor);

        rodRotation.x = Mathf.Clamp(rodRotation.x, -30f, 30f);
        rodRotation.y = Mathf.Clamp(rodRotation.y, -30f, 30f);
        rodRotation.z = Mathf.Clamp(rodRotation.z, -30f, 30f);

        FishingRod.localRotation = Quaternion.Euler(-rodRotation.y, 0, -rodRotation.x);
    }
}

